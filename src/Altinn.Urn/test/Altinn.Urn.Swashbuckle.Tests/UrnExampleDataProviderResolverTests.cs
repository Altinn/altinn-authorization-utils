using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Swashbuckle.Tests.Fixtures;

namespace Altinn.Urn.Swashbuckle.Tests;

public class UrnExampleDataProviderResolverTests
{
    [Fact]
    public void CanResolveUrnsVariants()
    {
        var options = new ExampleDataOptions();
        options.ProviderResolverChain.Add(new UrnExampleDataProviderResolver());

        var intExamples = ExampleData.GetExamples<int>(options)!.ToList();

        var examples = ExampleData.GetExamples<PersonUrn.PartyId>(options);
        Assert.NotNull(examples);

        examples.ShouldNotBeEmpty();
        var examplesList = examples.ToList();

        examplesList.Count.ShouldBe(intExamples.Count);

        for (var i = 0; i < intExamples.Count; i++)
        {
            examplesList[i].Value.ShouldBe(intExamples[i]);
        }
    }

    [Fact]
    public void UrnExamplesAreBalanced()
    {
        var options = new ExampleDataOptions();
        options.ProviderResolverChain.Add(new UrnExampleDataProviderResolver());
        options.ProviderResolverChain.Add(new PersonIdentifierExampleProviderResolver());

        var urnExamples = ExampleData.GetExamples<PersonUrn>(options)?.Take(PersonUrn.Variants.Length);
        Assert.NotNull(urnExamples);

        HashSet<PersonUrn.Type> missing = [.. PersonUrn.Variants];

        foreach (var example in urnExamples)
        {
            missing.Remove(example.UrnType);
        }

        missing.ShouldBeEmpty(customMessage: "The first N examples of PersonUrn should have N different types");
    }
}
