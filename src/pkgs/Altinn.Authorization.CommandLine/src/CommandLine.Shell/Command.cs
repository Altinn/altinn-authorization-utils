using System.Collections.Immutable;
using System.Diagnostics;
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

    private Process Start()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = CommandHelper.Which(_filename),
            WorkingDirectory = _workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,

#if NET11_0_OR_GREATER
            KillOnParentExit = true,
#endif
        };

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
        using var registration = cancellationToken.Register(static p =>
        {
            var proc = (Process)p!;
            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignore exceptions when killing the process.
            }
        }, process, useSynchronizationContext: false);

        await process.WaitForExitAsync(cancellationToken);
    }
}
