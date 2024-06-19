using FluentAssertions.Formatting;
using System.Net;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Tests.FluentAssertionsExtensions;

internal static class Initializer
{
    [ModuleInitializer]
    public static void SetDefaults()
    {
        AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByValue<IPAddress>());
        AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByValue<IPNetwork>());
        AssertionOptions.AssertEquivalencyUsing(options => options.Using(new CIDREquivalencyStep()));

        Formatter.AddFormatter(new CIDRValueFormatter());
    }
}
