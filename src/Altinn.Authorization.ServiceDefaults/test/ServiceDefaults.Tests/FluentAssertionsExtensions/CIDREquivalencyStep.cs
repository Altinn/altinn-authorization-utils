using Altinn.Authorization.ServiceDefaults.Utils;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using System.Diagnostics;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests.FluentAssertionsExtensions;

internal class CIDREquivalencyStep
        : IEquivalencyStep
{
    public EquivalencyResult Handle(
        Comparands comparands,
        IEquivalencyValidationContext context,
        IEquivalencyValidator nestedValidator)
    {
        {
            if (comparands.Subject is IPNetwork subject
                && comparands.Expectation is Microsoft.AspNetCore.HttpOverrides.IPNetwork expectationRaw)
            {
                context.Tracer.WriteLine(member
                    => $"Converting expectation for {member.Description} from 'Microsoft.AspNetCore.HttpOverrides.IPNetwork' to 'System.IPNetwork'");

                if (!IPNetworkUtils.TryFrom(expectationRaw, out var expectation))
                {
                    Execute.Assertion
                        .BecauseOf(context.Reason.FormattedMessage, context.Reason.Arguments)
                        .FailWith("Failed to convert 'Microsoft.AspNetCore.HttpOverrides.IPNetwork' to 'System.IPNetwork'.");

                    throw new UnreachableException();
                }

                subject.Should().Be(expectation, context.Reason.FormattedMessage, context.Reason.Arguments);
                return EquivalencyResult.AssertionCompleted;
            }
        }

        {
            if (comparands.Subject is Microsoft.AspNetCore.HttpOverrides.IPNetwork subjectRaw
                && comparands.Expectation is IPNetwork expectation)
            {
                context.Tracer.WriteLine(member
                    => $"Converting {member.Description} from 'Microsoft.AspNetCore.HttpOverrides.IPNetwork' to 'System.IPNetwork'");

                if (!IPNetworkUtils.TryFrom(subjectRaw, out var subject))
                {
                    Execute.Assertion
                        .BecauseOf(context.Reason.FormattedMessage, context.Reason.Arguments)
                        .FailWith("Failed to convert 'Microsoft.AspNetCore.HttpOverrides.IPNetwork' to 'System.IPNetwork'.");

                    throw new UnreachableException();
                }

                subject.Should().Be(expectation, context.Reason.FormattedMessage, context.Reason.Arguments);
                return EquivalencyResult.AssertionCompleted;
            }
        }
        
        return EquivalencyResult.ContinueWithNext;
    }
}
