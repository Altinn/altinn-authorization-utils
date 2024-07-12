using FluentAssertions.Formatting;
using System.Net;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Tests.FluentAssertionsExtensions;

internal static class Initializer
{
    [ModuleInitializer]
    public static void SetDefaults()
    {
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options.ComparingByValue<IPAddress>();
            options.ComparingByValue<IPNetwork>();
            options.Using(new IPNetworkConversionStep());
            options.Using(new JsonDocumentConversionStep());
            options.Using(new JsonStringConversionStep());
            options.Using(new JsonElementEquivalencyStep());

            return options;
        });

        Formatter.AddFormatter(new CIDRValueFormatter());
    }
}
