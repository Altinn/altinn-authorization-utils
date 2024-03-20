namespace Altinn.Urn.SourceGenerator.Tests;

public class UrnGeneratorSnapshotTests
{
    [Fact]
    public async Task EmptyUrn()
    {
        // The source code to test
        var source = """
            using Altinn.Urn;
            
            namespace MyNamespace;

            [Urn]
            public abstract partial record MyUrn 
            {
            }
            """;

        await SourceGeneratorUtils.VerifySourceGeneratorOutput(source);
    }

    [Fact]
    public async Task GenerateUrnAttribute()
    {
        // The source code to test
        var source = """
            using Altinn.Urn;
            using System;
            
            namespace MyNamespace;

            [Urn]
            public abstract partial record MyUrn 
            {
                [UrnType("altinn:test1")]
                public partial bool IsTest1(out Guid type);

                [UrnType("altinn:test2")]
                public partial bool IsTest2(out int type);

                [UrnType("altinn:test2:sub")]
                [UrnType("altinn:test2:sub1")]
                [UrnType("altinn:test2:sub2")]
                public partial bool IsTest2Sub(out float type);

                [UrnType("notaltinn")]
                public partial bool IsTest3(out long type);

                [UrnType("nroot")]
                public partial bool IsTest4(out uint type);
            }
            """;

        await SourceGeneratorUtils.VerifySourceGeneratorOutput(source);
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
                [Urn]
                public abstract partial record PersonUrn
                {
                    [UrnType("altinn:party:id")]
                    public partial bool IsPartyId(out int partyId);

                    [UrnType("altinn:party:uuid")]
                    public partial bool IsPartyUuid(out Guid partyUuid);
                }
            }
            """;

        await SourceGeneratorUtils.VerifySourceGeneratorOutput(source);
    }
}
