using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.Utils;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckIssue
    : JustInTimeSizeDependentRenderable
{
    public static CheckIssue CreateFile(string file, string message)
        => new(file: file, message: message);

    public static CheckIssue CreateCommand(string command)
        => new(command: command);

    private CheckIssue(string file, string message)
    {
        _file = file;
        _message = message;
    }

    private CheckIssue(string command)
    {
        _command = command;
    }

    private string? _file;
    private string? _message;
    private string? _command;

    /// <summary>
    /// Gets the relative path to the file that the issue is related to, or null if the issue is not related to a specific file.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? File
        => _file;

    /// <summary>
    /// Gets the message describing the issue, or null if there is no message.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message
        => _message;

    /// <summary>
    /// Gets the command that caused the issue, or null if the issue is not related to a specific command.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Command
        => _command;

    protected override IRenderable Build(int maxWidth)
    {
        var paragraph = new Paragraph();
        if (!string.IsNullOrEmpty(_file))
        {
            paragraph.Append("[");
            paragraph.Append(_file, Color.Cyan);
            paragraph.Append("] ");
        }

        if (!string.IsNullOrEmpty(_command))
        {
            paragraph.Append("`");
            paragraph.Append(_command, Color.Yellow);
            paragraph.Append("` ");
        }

        if (!string.IsNullOrEmpty(_message))
        {
            paragraph.Append(_message);
        }

        return paragraph;
    }
}
