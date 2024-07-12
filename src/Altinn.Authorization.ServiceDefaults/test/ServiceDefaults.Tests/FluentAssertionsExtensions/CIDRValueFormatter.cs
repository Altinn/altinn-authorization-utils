using Altinn.Authorization.ServiceDefaults.Utils;
using CommunityToolkit.Diagnostics;
using FluentAssertions.Formatting;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests.FluentAssertionsExtensions;

[ExcludeFromCodeCoverage]
internal class CIDRValueFormatter
    : IValueFormatter
{
    public bool CanHandle(object value)
        => value is IPNetwork
        || value is Microsoft.AspNetCore.HttpOverrides.IPNetwork;

    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        if (value is IPNetwork n)
        {
            Format(n, formattedGraph, context, formatChild);
        }
        else if (value is Microsoft.AspNetCore.HttpOverrides.IPNetwork n2)
        {
            Format(n2, formattedGraph, context, formatChild);
        }
        else
        {
            ThrowHelper.ThrowArgumentException("Unexpected value type.");
        }
    }

    private static void Format(IPNetwork network, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        var cidr = $"{network.BaseAddress}/{network.PrefixLength}";
        Format(cidr, network.BaseAddress, network, formattedGraph, context, formatChild);
    }

    private static void Format(Microsoft.AspNetCore.HttpOverrides.IPNetwork network, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        var cidr = $"{network.Prefix}/{network.PrefixLength}";
        var ipNetwork = IPNetworkUtils.From(network);
        Format(cidr, network.Prefix, ipNetwork, formattedGraph, context, formatChild);
    }

    private static void Format(string cidr, IPAddress address, IPNetwork network, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        formattedGraph.AddFragment(cidr);

        if (context.UseLineBreaks)
        {
            formattedGraph.AddLine("{");
            formattedGraph.AddLine($"   Address: {address},"); ;
            formattedGraph.AddLine($"   BaseAddress: {network.BaseAddress},");
            formattedGraph.AddLine($"   PrefixLength: {network.PrefixLength},");
            formattedGraph.AddLine($"   StartAddress: {network.StartAddress()},");
            formattedGraph.AddLine($"   EndAddress: {network.EndAddress()},");
            formattedGraph.AddLine("}");
        }
    }
}
