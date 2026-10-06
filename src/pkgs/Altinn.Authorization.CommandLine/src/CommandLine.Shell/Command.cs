using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// A command that can be executed.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class Command
    : IFormattable
{
    /// <summary>
    /// Creates a new command with the specified filename and arguments.
    /// </summary>
    /// <param name="filename">The name of the executable file.</param>
    /// <param name="args">The arguments to pass to the executable.</param>
    /// <returns>A new <see cref="Command"/> instance.</returns>
    public static Command Create(string filename, params ReadOnlySpan<string> args)
    {
        Guard.IsNotNullOrEmpty(filename);

        return new Command(filename, ImmutableArray.Create(args), null);
    }

    private readonly string _filename;
    private readonly ImmutableArray<string> _arguments;
    private readonly string? _workingDirectory;

    /// <summary>
    /// Gets the filename of the command to execute.
    /// </summary>
    public string Filename
        => _filename;

    /// <summary>
    /// Gets the arguments to pass to the command when executing it.
    /// </summary>
    public ImmutableArray<string> Arguments
        => _arguments;

    private Command(string filename, ImmutableArray<string> arguments, string? workingDirectory)
    {
        Guard.IsNotNullOrEmpty(filename);

        if (workingDirectory is not null)
        {
            Guard.IsNotEmpty(workingDirectory);
        }

        _filename = filename;
        _arguments = arguments;
        _workingDirectory = workingDirectory;
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> instance with the specified working directory.
    /// </summary>
    /// <param name="workingDirectory">The working directory for the command.</param>
    /// <returns>A new <see cref="Command"/> instance with the specified working directory.</returns>
    public Command WithWorkingDirectory(string workingDirectory)
    {
        Guard.IsNotNullOrEmpty(workingDirectory);

        return new Command(_filename, _arguments, workingDirectory);
    }

    /// <inheritdoc/>
    public override string ToString()
        => ToString(format: null);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        return format switch
        {
            "display" => string.Join(' ', Parts(this)),
            _ => _filename,
        };

        static IEnumerable<string> Parts(Command command)
        {
            yield return Part(command._filename);

            foreach (var arg in command._arguments)
            {
                yield return Part(arg);
            }
        }

        static string Part(string value)
        {
            if (value.Contains(' ') || value.Contains('"'))
            {
                return $"\"{value.Replace("\"", "\\\"")}\"";
            }

            return value;
        }
    }

    /// <summary>
    /// Executes the command and throws an exception if the process exits with a non-zero exit code.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var process = Start();
        await ExecuteCore(process, cancellationToken);

        if (process.ExitCode != 0)
        {
            ThrowHelper.ThrowProcessFailedException(process, _filename);
        }

        // Note: We don't dispose the process on exception, because the exception carries the process as a property,
        // and a caller may want to inspect it.
        process.Dispose();
    }

    /// <summary>
    /// Executes the command and yields each line of output as it is produced.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{CommandOutputLine}"/> representing the asynchronous stream of output lines.</returns>
    public async IAsyncEnumerable<CommandOutputLine> ExecuteLineCaptured(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var process = Start(captureOutput: true);
        var disableDispose = false;

        Task? readerTask = null, executeTask = null;

        try
        {
            var channel = Channel.CreateBounded<CommandOutputLine>(new BoundedChannelOptions(capacity: 2)
            {
                SingleReader = true,
                SingleWriter = false,
            });

            // This is important, because `ExecuteCore` watches the cancellation token,
            // and attempts to kill the process if it's cancelled. As such, this has to
            // be started before we setup the reading loop.
            executeTask = ExecuteCore(process, cts.Token);
            readerTask = ReadLinesAsync(process, channel.Writer, cts.Token);

            await foreach (var line in channel.Reader.ReadAllAsync(cts.Token))
            {
                yield return line;
            }

            await readerTask;
            await executeTask;

            if (process.ExitCode != 0)
            {
                disableDispose = true;
                ThrowHelper.ThrowProcessFailedException(process, _filename);
            }
        }
        finally
        {
            await cts.CancelAsync();

            // wait for both tasks to settle
            if (readerTask is not null)
            {
                await readerTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }

            if (executeTask is not null)
            {
                await executeTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }

            // Note: We don't dispose the process on exception, because the exception carries the process as a property,
            // and a caller may want to inspect it.
            if (!disableDispose)
            {
                process.Dispose();
            }
        }

        static async Task ReadLinesAsync(Process process, ChannelWriter<CommandOutputLine> writer, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                await Task.WhenAll(
                    ReadReaderLinesAsync(process.StandardOutput, CommandOutputSource.Stdout, writer, cts),
                    ReadReaderLinesAsync(process.StandardError, CommandOutputSource.Stderr, writer, cts));

                writer.TryComplete();
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
        }

        static async Task ReadReaderLinesAsync(
            StreamReader? reader,
            CommandOutputSource source,
            ChannelWriter<CommandOutputLine> writer,
            CancellationTokenSource cts)
        {
            if (reader is null)
            {
                return;
            }

            try
            {
                string? line;
                while ((line = await reader.ReadLineAsync(cts.Token)) is not null)
                {
                    await writer.WriteAsync(new CommandOutputLine(source, line), cts.Token);
                }
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                // Ignore cancellation exceptions.
            }
            catch
            {
                await cts.CancelAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// Executes the command and captures its complete output without splitting it into lines.
    /// Throws an exception if the process exits with a non-zero exit code.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The exit code, standard output, and standard error.</returns>
    public async Task<(int ExitCode, string StandardOutput, string StandardError)> ExecuteCaptured(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var process = Start(captureOutput: true);
        var disableDispose = false;
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await ExecuteCore(process, cancellationToken);
            var output = await outputTask;
            var error = await errorTask;
            if (process.ExitCode != 0)
            {
                disableDispose = true;
                ThrowHelper.ThrowProcessFailedException(process, _filename);
            }

            return (process.ExitCode, output, error);
        }
        finally
        {
            await ((Task)Task.WhenAll(outputTask, errorTask)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            // The failure exception carries the process for the caller to inspect.
            if (!disableDispose)
            {
                process.Dispose();
            }
        }
    }

    private Process Start(bool captureOutput = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = CommandHelper.Which(_filename),
            WorkingDirectory = _workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardInput = false,
            RedirectStandardOutput = captureOutput,
            RedirectStandardError = captureOutput,

#if NET11_0_OR_GREATER
            KillOnParentExit = true,
#endif
        };

        if (captureOutput)
        {
            startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }

        foreach (var arg in _arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        var process = Process.Start(startInfo);
        if (process is null)
        {
            ThrowHelper.ThrowInvalidOperationException($"Failed to start process '{_filename}'.");
        }

        return process;
    }

    private async Task ExecuteCore(Process process, CancellationToken cancellationToken)
    {
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException) when (process.HasExited)
            {
                // The process exited between checking HasExited and calling Kill.
            }
            catch (System.ComponentModel.Win32Exception) when (process.HasExited)
            {
                // The process exited between checking HasExited and calling Kill.
            }

            // Cancellation stops waiting; it does not guarantee the process has exited.
            await process.WaitForExitAsync(CancellationToken.None);
            throw;
        }
    }
}
