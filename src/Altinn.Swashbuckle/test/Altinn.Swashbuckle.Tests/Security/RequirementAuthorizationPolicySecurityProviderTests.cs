using Altinn.Swashbuckle.Security;
using Microsoft.AspNetCore.Authorization;

namespace Altinn.Swashbuckle.Tests.Security;

public class RequirementAuthorizationPolicySecurityProviderTests
{
    private static OpenApiSecurityContext Context { get; } = new() { DocumentName = "test-doc" };
    private static IAuthorizationRequirement FooReq { get; } = new FooRequirement();
    private static IOpenApiAuthorizationRequirementConditionProvider FooSchemeNoScope { get; } = new TestProvider<FooRequirement>(SecurityRequirementCondition.Create("foo-scheme"));
    private static IOpenApiAuthorizationRequirementConditionProvider FooSchemeAdminScope { get; } = new TestProvider<FooRequirement>(SecurityRequirementCondition.Create("foo-scheme", "admin"));
    private static IOpenApiAuthorizationRequirementConditionProvider FooSchemeReadScope { get; } = new TestProvider<FooRequirement>(SecurityRequirementCondition.Create("foo-scheme", "read"));
    private static IAuthorizationRequirement BarReq { get; } = new BarRequirement();
    private static IOpenApiAuthorizationRequirementConditionProvider BarSchemeNoScope { get; } = new TestProvider<BarRequirement>(SecurityRequirementCondition.Create("bar-scheme"));

    [Fact]
    public async Task UnknownRequirement_Ignored()
    {
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        var sut = new RequirementAuthorizationPolicySecurityProvider([
            FooSchemeNoScope,
            BarSchemeNoScope,
        ]);

        var requirements = await sut.GetSecurityRequirementsForAuthorizationPolicy(policy, Context, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        requirements.ShouldBeEmpty();
    }

    [Fact]
    public async Task No_Provider_EmptyRequirements()
    {
        var policy = new AuthorizationPolicyBuilder().AddRequirements(FooReq, BarReq).Build();
        var sut = new RequirementAuthorizationPolicySecurityProvider([]);

        var requirements = await sut.GetSecurityRequirementsForAuthorizationPolicy(policy, Context, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        requirements.ShouldBeEmpty();
    }

    [Fact]
    public async Task ProviderResponses_Are_Combined()
    {
        var policy = new AuthorizationPolicyBuilder().AddRequirements(FooReq).Build();
        var sut = new RequirementAuthorizationPolicySecurityProvider([
            FooSchemeAdminScope,
            FooSchemeReadScope,
        ]);

        var requirements = await sut.GetSecurityRequirementsForAuthorizationPolicy(policy, Context, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        requirements.Count.ShouldBe(1);
        requirements[0].ShouldSatisfyAllConditions(
            r => r.Display.ShouldStartWith(nameof(FooRequirement)),
            r => r.Count.ShouldBe(2),
            r => r.ShouldContain(SecurityRequirementCondition.Create("foo-scheme", "admin")),
            r => r.ShouldContain(SecurityRequirementCondition.Create("foo-scheme", "read")));
    }

    [Fact]
    public async Task Multiple_Requirements()
    {
        var policy = new AuthorizationPolicyBuilder().AddRequirements(FooReq, BarReq).Build();
        var sut = new RequirementAuthorizationPolicySecurityProvider([
            FooSchemeAdminScope,
            FooSchemeReadScope,
            BarSchemeNoScope,
        ]);

        var requirements = await sut.GetSecurityRequirementsForAuthorizationPolicy(policy, Context, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        requirements.Count.ShouldBe(2);
        requirements[0].ShouldSatisfyAllConditions(
            r => r.Display.ShouldStartWith(nameof(FooRequirement)),
            r => r.Count.ShouldBe(2),
            r => r.ShouldContain(SecurityRequirementCondition.Create("foo-scheme", "admin")),
            r => r.ShouldContain(SecurityRequirementCondition.Create("foo-scheme", "read")));

        requirements[1].ShouldSatisfyAllConditions(
            r => r.Display.ShouldStartWith(nameof(BarRequirement)),
            r => r.Count.ShouldBe(1),
            r => r.ShouldContain(SecurityRequirementCondition.Create("bar-scheme")));
    }

    private record FooRequirement : IAuthorizationRequirement { }
    private record BarRequirement : IAuthorizationRequirement { }

    private sealed class TestProvider<T>(SecurityRequirementCondition condition)
        : OpenApiAuthorizationRequirementConditionProvider<T>
        where T : IAuthorizationRequirement
    {
        protected override IAsyncEnumerable<SecurityRequirementCondition> GetCandidatesForAuthorizationRequirement(T requirement, OpenApiSecurityContext context, CancellationToken cancellationToken = default)
            => AsyncEnumerable.ToAsyncEnumerable([condition]);
    }
}
