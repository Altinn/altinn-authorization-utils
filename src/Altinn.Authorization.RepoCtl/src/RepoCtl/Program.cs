using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.RepoCtl.Binding;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Formatting;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Options;
using Altinn.Authorization.RepoCtl.Solutions;
using Microsoft.Extensions.DependencyInjection;

var builder = CliApplication.CreateBuilder("Altinn Authorization Repository Manager (repoctl)");
builder.Services.AddSingleton<AltinnRepositoryResolver>();
builder.Services.AddSingleton<IConfigureOption, ConfigureAltinnVerticalKindOptions>();
builder.Services.AddSingleton<ICommandHandlerParameterBinderResolver, AltinnRepositoryBinderResolver>();
builder.Services.AddSingleton<AltinnRepositoryLoader>();
builder.Services.AddSingleton<SolutionService>();

builder.Services.AddSingleton<IRepositoryCheck>(s => s.GetRequiredService<SolutionService>());

builder.Services.AddOutputFormatter<RichFormat, AltinnVerticalSetFormatter>();
builder.Services.AddOutputFormatter<JsonFormat, AltinnVerticalSetFormatter>();
builder.Services.AddOutputFormatter<RichFormat, CheckResultListFormatter>();
builder.Services.AddOutputFormatter<JsonFormat, CheckResultListFormatter>();

var cli = builder.Build();
cli.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>().Configure(cli);

cli.AddCommand("verticals", "Operate on verticals", (builder) =>
{
    builder.AddCommand("list", "List all verticals in the repository", (AltinnVerticalSet verticals) => verticals);
});

cli.AddCommand("solutions", "Operate on solutions", (builder) =>
{
    builder.AddCommand("update", "Update the solutions in this repository", async (AltinnRepository repository, SolutionService solutionService, CancellationToken cancellationToken) =>
    {
        await solutionService.UpdateSolutions(repository, cancellationToken);
    });

    builder.AddCommand("check", "Check the solutions in this repository", async (CommandInvocationContext ctx, AltinnRepository repository, SolutionService solutionService, CancellationToken cancellationToken) =>
    {
        var result = await solutionService.Check(repository, cancellationToken);
        if (!result.IsSuccess)
        {
            ctx.ReturnCode = 1;
        }

        return result;
    });
});

cli.AddCommand("check", "Pre-commit/pre-merge checks for the repository", async (CommandInvocationContext ctx, AltinnRepository repository, IEnumerable<IRepositoryCheck> checks, CancellationToken cancellationToken) =>
{
    var results = new List<CheckResult>();
    foreach (var check in checks)
    {
        var checkResult = await check.Check(repository, cancellationToken);
        results.Add(checkResult);

        if (!checkResult.IsSuccess)
        {
            ctx.ReturnCode += 1;
        }
    }

    return results;
});

return await cli.RunAsync(args);
