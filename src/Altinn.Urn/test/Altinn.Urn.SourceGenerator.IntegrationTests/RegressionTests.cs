using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

internal partial class RegressionTests
{
    public sealed record AccessPackageIdentifier(string Value)
        : IFormattable
        , IParsable<AccessPackageIdentifier>
    {
        public static AccessPackageIdentifier Parse(string s, IFormatProvider? provider)
            => new(s);

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccessPackageIdentifier result)
        {
            result = new(s!);
            return true;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
            => Value;
    }

    [KeyValueUrn]
    public abstract partial record AccessPackageUrn1
    {
        [UrnKey("altinn:accesspackage", Canonical = true)]
        public partial bool IsAccessPackage(out string packageId);

        private static bool TryParseAccessPackage(ReadOnlySpan<char> segment, IFormatProvider? provider, out string value)
        {
            value = new(segment);
            return true;
        }
        private static string FormatAccessPackage(string value, string? format, IFormatProvider? provider)
        {
            return value;
        }
    }

    [KeyValueUrn]
    public abstract partial record AccessPackageUrn2
    {
        [UrnKey("altinn:accesspackage", Canonical = true)]
        public partial bool IsAccessPackage(out AccessPackageIdentifier packageId);
    }
}
