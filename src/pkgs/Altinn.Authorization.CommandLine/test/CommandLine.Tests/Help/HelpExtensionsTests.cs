using System.CommandLine;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine.Tests.Help;

public class HelpExtensionsTests
{
    [Fact]
    public void InvocationPath_WhenCommandIsShared_UsesInvokedParent()
    {
        var root = new RootCommand("executable");
        var parentA = new Command("parent-a");
        var parentB = new Command("parent-b");
        var child = new Command("child");

        parentA.Subcommands.Add(child);
        parentB.Subcommands.Add(child);
        root.Subcommands.Add(parentA);
        root.Subcommands.Add(parentB);

        var parseResultA = root.Parse("parent-a child --help");
        var parseResultB = root.Parse("parent-b child --help");

        parseResultA.CommandResult.InvocationPath()
            .ShouldBe([child, parentA, root]);

        parseResultB.CommandResult.InvocationPath()
            .ShouldBe([child, parentB, root]);
    }
}
