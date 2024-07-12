using Altinn.Authorization.ServiceDefaults.Utils;
using FluentAssertions.Equivalency;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests.FluentAssertionsExtensions;

[ExcludeFromCodeCoverage]
internal class IPNetworkConversionStep
    : IEquivalencyStep
{
    public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context, IEquivalencyValidator nestedValidator)
    {
        {
            if (comparands.Subject is IPNetwork
                && comparands.Expectation is Microsoft.AspNetCore.HttpOverrides.IPNetwork expectation)
            {
                if (IPNetworkUtils.TryFrom(expectation, out var converted))
                {
                    context.Tracer.WriteLine(member => string.Create(CultureInfo.InvariantCulture,
                        $"Converted expectation {comparands.Expectation} at {member.Description} to Microsoft.AspNetCore.HttpOverrides.IPNetwork"));

                    comparands.Expectation = converted;
                }
                else
                {
                    context.Tracer.WriteLine(member => string.Create(CultureInfo.InvariantCulture,
                        $"Expectation {comparands.Expectation} at {member.Description} could not be converted to Microsoft.AspNetCore.HttpOverrides.IPNetwork"));
                }
            }
        }

        {
            if (comparands.Subject is Microsoft.AspNetCore.HttpOverrides.IPNetwork subject
                && comparands.Expectation is IPNetwork)
            {
                if (IPNetworkUtils.TryFrom(subject, out var converted))
                {
                    context.Tracer.WriteLine(member => string.Create(CultureInfo.InvariantCulture,
                        $"Converted subject {comparands.Subject} at {member.Description} to Microsoft.AspNetCore.HttpOverrides.IPNetwork"));

                    comparands.Subject = converted;
                }
                else
                {
                    context.Tracer.WriteLine(member => string.Create(CultureInfo.InvariantCulture,
                        $"Subject {comparands.Subject} at {member.Description} could not be converted to Microsoft.AspNetCore.HttpOverrides.IPNetwork"));
                }
            }
        }

        return EquivalencyResult.ContinueWithNext;
    }
}
