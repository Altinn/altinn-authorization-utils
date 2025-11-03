using Microsoft.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Tests;

public class UrnGeneratorSnapshotTests
{
    private static Task TestKeyValueUrn(string body, DiagnosticDescriptor? expectedError = null)
    {
        DiagnosticDescriptor[] errors = expectedError is null ? [] : [expectedError];
        return TestKeyValueUrn(body, errors);
    }

    private static async Task TestKeyValueUrn(string body, DiagnosticDescriptor[] expectedErrors)
    {
        var source = $$"""
            using Altinn.Urn;
            using System;
            
            namespace My.Test.Namespace;
            
            [KeyValueUrn]
            public abstract partial record TestUrn 
            {
                {{body}}
            }
            """;

        await SourceGeneratorUtils.VerifySourceGeneratorOutput(source, expectedErrors);
    }

    [Fact]
    public async Task EmptyUrn()
    {
        // The source code to test
        var source = "";

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnRecordHasNoUrnTypeMethods);
    }

    [Fact]
    public async Task UrnKeyPrefix()
    {
        // The source code to test
        var source = """
            [UrnKey("urn:altinn:test1")]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixStartsWithUrn);
    }

    [Fact]
    public async Task UrnKeyEmpty()
    {
        // The source code to test
        var source = """
            [UrnKey("")]
            public partial bool IsTest1(out Guid type);

            [UrnKey("altinn:test1")]
            public partial bool IsTest2(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsEmpty);
    }

    [Fact]
    public async Task UrnKeyWhitespace()
    {
        // The source code to test
        var source = """
            [UrnKey("  ")]
            public partial bool IsTest1(out Guid type);

            [UrnKey("altinn:test1")]
            public partial bool IsTest2(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsEmpty);
    }

    [Fact]
    public async Task UrnKeyTrim()
    {
        // The source code to test
        var source = """
            [UrnKey(" altinn:test1 ")]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixHasWhitespace);
    }

    [Fact]
    public async Task UrnKeyColonSuffix()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1:")]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixEndsWithColon);
    }

    [Fact]
    public async Task UrnKeyTrim_UrnPrefix_ColonSuffix()
    {
        // The source code to test
        var source = """
            [UrnKey("  urn:altinn:test1:     \t")]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(
            source, 
            [
                DiagnosticDescriptors.UrnTypeMethodPrefixHasWhitespace, 
                DiagnosticDescriptors.UrnTypeMethodPrefixStartsWithUrn, 
                DiagnosticDescriptors.UrnTypeMethodPrefixEndsWithColon,
            ]);
    }

    [Fact]
    public async Task UrnExplicitNoCanonical()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1", Canonical = false)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsMissingCanonical);
    }

    [Fact]
    public async Task UrnMultipleKeys_NoCanonical()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1")]
            [UrnKey("altinn:test2")]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsMissingCanonical);
    }

    [Fact]
    public async Task UrnMultipleKeys_ExplicitNoCanonical()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1", Canonical = false)]
            [UrnKey("altinn:test2", Canonical = false)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsMissingCanonical);
    }

    [Fact]
    public async Task UrnMultipleKeys()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1")]
            [UrnKey("altinn:test2", Canonical = true)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source);
    }

    [Fact]
    public async Task UrnMultipleKeys_ExplicitCanonicalFalse()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1", Canonical = false)]
            [UrnKey("altinn:test2", Canonical = true)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source);
    }

    [Fact]
    public async Task UrnMultipleKeys_MultipleCanonical()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1", Canonical = true)]
            [UrnKey("altinn:test2", Canonical = true)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixHasMultipleCanonical);
    }

    [Fact]
    public async Task Urn_DuplicateKeys_SingleVariant()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1", Canonical = true)]
            [UrnKey("altinn:test1", Canonical = false)]
            public partial bool IsTest1(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsDuplicate);
    }

    [Fact]
    public async Task Urn_DuplicateKeys_DifferentVariant()
    {
        // The source code to test
        var source = """
            [UrnKey("altinn:test1")]
            public partial bool IsTest1(out Guid type);

            [UrnKey("altinn:test1")]
            public partial bool IsTest2(out Guid type);
            """;

        await TestKeyValueUrn(source, DiagnosticDescriptors.UrnTypeMethodPrefixIsDuplicate);
    }

    [Fact]
    public async Task Urn_Allows_Custom_Parse_For_Variant()
    {
        // The source code to test
        var source = """
            /// <summary>
            /// Try to get the urn as a party id.
            /// </summary>
            /// <param name="partyId">The resulting party id.</param>
            /// <returns><see langword="true"/> if this party reference is a party id, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:id")]
            public partial bool IsPartyId(out int partyId);

            /// <summary>
            /// Try to get the urn as a party uuid.
            /// </summary>
            /// <param name="partyUuid">The resulting party uuid.</param>
            /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:uuid")]
            public partial bool IsPartyUuid(out Guid partyUuid);

            // Manually overridden to disallow negative party ids
            private static bool TryParsePartyId(ReadOnlySpan<char> segment, IFormatProvider? provider, out int value)
                => int.TryParse(segment, NumberStyles.None, provider, out value);
            """;

        await TestKeyValueUrn(source);
    }

    [Fact]
    public async Task Urn_Allows_Custom_Format_For_Variant__FormatOnly()
    {
        // The source code to test
        var source = """
            /// <summary>
            /// Try to get the urn as a party id.
            /// </summary>
            /// <param name="partyId">The resulting party id.</param>
            /// <returns><see langword="true"/> if this party reference is a party id, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:id")]
            public partial bool IsPartyId(out int partyId);

            /// <summary>
            /// Try to get the urn as a party uuid.
            /// </summary>
            /// <param name="partyUuid">The resulting party uuid.</param>
            /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:uuid")]
            public partial bool IsPartyUuid(out Guid partyUuid);

            private static string FormatPartyId(int value, string? format, IFormatProvider? provider)
                => partyId.ToString();
            """;

        await TestKeyValueUrn(source);
    }

    [Fact]
    public async Task Urn_Allows_Custom_Format_For_Variant__TryFormatOnly()
    {
        // The source code to test
        var source = """
            /// <summary>
            /// Try to get the urn as a party id.
            /// </summary>
            /// <param name="partyId">The resulting party id.</param>
            /// <returns><see langword="true"/> if this party reference is a party id, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:id")]
            public partial bool IsPartyId(out int partyId);

            /// <summary>
            /// Try to get the urn as a party uuid.
            /// </summary>
            /// <param name="partyUuid">The resulting party uuid.</param>
            /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:uuid")]
            public partial bool IsPartyUuid(out Guid partyUuid);

            private static bool TryFormatPartyId(int value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => partyId.TryFormat(value, destination, charsWritten, format, provider);
            """;

        await TestKeyValueUrn(source, [DiagnosticDescriptors.UrnTypeMethodHasTryFormatButNoFormat, DiagnosticDescriptors.UrnValueMustBeFormattable]);
    }

    [Fact]
    public async Task Urn_Allows_Custom_Format_For_Variant__FormatAndTryFormat()
    {
        // The source code to test
        var source = """
            /// <summary>
            /// Try to get the urn as a party id.
            /// </summary>
            /// <param name="partyId">The resulting party id.</param>
            /// <returns><see langword="true"/> if this party reference is a party id, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:id")]
            public partial bool IsPartyId(out int partyId);

            /// <summary>
            /// Try to get the urn as a party uuid.
            /// </summary>
            /// <param name="partyUuid">The resulting party uuid.</param>
            /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
            [UrnKey("altinn:party:uuid")]
            public partial bool IsPartyUuid(out Guid partyUuid);

            private static string FormatPartyId(int value, string? format, IFormatProvider? provider)
                => partyId.ToString();

            private static bool TryFormatPartyId(int value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => partyId.TryFormat(value, destination, charsWritten, format, provider);
            """;

        await TestKeyValueUrn(source);
    }

    [Fact]
    public async Task NestedTypes()
    {
        // The source code to test
        var source = """
            using Altinn.Urn;
            using System;
            
            namespace MyNamespace;

            public partial class PersonUrnTests
            {
                [KeyValueUrn]
                public abstract partial record PersonUrn
                {
                    [UrnKey("altinn:party:id")]
                    public partial bool IsPartyId(out int partyId);

                    [UrnKey("altinn:party:uuid")]
                    public partial bool IsPartyUuid(out Guid partyUuid);
                }
            }
            """;

        await SourceGeneratorUtils.VerifySourceGeneratorOutput(source, []);
    }
}
