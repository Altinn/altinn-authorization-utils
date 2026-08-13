using System.Collections.Frozen;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using Altinn.Authorization.RepoCtl.Model;
using CommunityToolkit.Diagnostics;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Formatting;

internal sealed class AltinnVerticalSetFormatter
    : IFormatter<RichFormat>
    , IFormatter<JsonFormat>
{
    private static readonly FrozenDictionary<AltinnProjectType, string> ProjectTypeNames = GetProjectTypeNames();
    private static readonly Notation ListItemPrefix = Notation.Text("- ", Color.Blue);

    private static FrozenDictionary<AltinnProjectType, string> GetProjectTypeNames()
    {
        var values = Enum.GetValues<AltinnProjectType>();
        var dict = new Dictionary<AltinnProjectType, string>(values.Length);

        foreach (var value in values)
        {
            var name = value.ToString();
            var field = typeof(AltinnProjectType).GetField(name);
            if (field?.GetCustomAttributes(inherit: false).OfType<JsonStringEnumMemberNameAttribute>().FirstOrDefault() is { } attr)
            {
                name = attr.Name;
            }

            dict.Add(value, name);
        }

        return dict.ToFrozenDictionary();
    }

    public bool CanFormat(Type type)
        => type == typeof(AltinnVerticalSet);

    public ValueTask Format(
        object value,
        RichFormat format,
        IFormatWriter writer,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAssignableToType<AltinnVerticalSet>(value);
        var verticals = (AltinnVerticalSet)value;

        var output = ((IEnumerable<AltinnVertical>)verticals).Aggregate(Notation.Empty, (acc, vertical) => acc + Notation.Newline + ToListNotation(vertical));
        return format.Write(Notation.Indent(1, output), writer, cancellationToken);
    }

    public ValueTask Format(
        object value,
        JsonFormat format,
        IFormatWriter writer,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAssignableToType<AltinnVerticalSet>(value);
        var verticals = (IEnumerable<AltinnVertical>)value;

        var items = verticals.Select(static v => new
        {
            id = v.Id,
            kind = v.Kind,
            name = v.FullName,
            version = v.Version.ToString(),
            deps = ((IEnumerable<AltinnVertical>)v.AllDependencies).Select(static d => d.Id),
            projects = v.Projects.Select(static p => new
            {
                name = p.Name,
                type = ProjectTypeNames[p.Type],
            }),
        });

        return format.Write(items, writer, cancellationToken);
    }

    private static Notation ToListNotation(AltinnVertical vertical)
    {
        var versionDisplay = Notation.Text("(") + Notation.Text("v" + vertical.Version.ToString(), Color.Gray) + Notation.Text(")");
        return ListItemPrefix + Notation.Text("Vertical: ") + VerticalDisplay(vertical) + Notation.Text(" ") + versionDisplay + Notation.Indent(2, VerticalDeps(vertical) + Projects(vertical));
    }

    private static Notation VerticalDisplay(AltinnVertical vertical)
        => Notation.Text(vertical.Kind.ToString(), Color.Cyan) + Notation.Text(": ") + Notation.Text(vertical.DisplayName, Color.Green); // TODO: shortname

    private static Notation VerticalDeps(AltinnVertical vertical)
    {
        if (vertical.AllDependencies.Count == 0)
        {
            return Notation.Empty;
        }

        var prefix = Notation.Newline + Notation.Text("Depends on:");
        Notation? inner = null;

        foreach (var dep in vertical.AllDependencies)
        {
            var item = ListItemPrefix + VerticalDisplay(dep);

            inner = inner is null ? item : inner + Notation.Newline + item;
        }

        return prefix + Notation.Indent(2, Notation.Newline + inner!);
    }

    private static Notation Projects(AltinnVertical vertical)
    {
        if (vertical.Projects.Length == 0)
        {
            return Notation.Empty;
        }

        var prefix = Notation.Newline + Notation.Text("Projects:");
        Notation? inner = null;

        foreach (var project in vertical.Projects)
        {
            var typeName = ProjectTypeNames[project.Type];
            var item = ListItemPrefix + Notation.Text(project.Name, Color.Yellow) + Notation.Text(" (") + Notation.Text(typeName, Color.Magenta) + Notation.Text(")");

            inner = inner is null ? item : inner + Notation.Newline + item;
        }

        return prefix + Notation.Indent(2, Notation.Newline + inner!);
    }
}
