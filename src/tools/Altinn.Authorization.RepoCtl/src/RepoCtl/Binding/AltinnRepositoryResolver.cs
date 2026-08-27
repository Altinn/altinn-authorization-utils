using System.CommandLine;
using System.Diagnostics;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Binding;

internal sealed class AltinnRepositoryResolver
    : ICommandHandlerParameterResolver
{
    private readonly Option<DirectoryInfo> _workingDirectoryOption;

    public AltinnRepositoryResolver(OptionFactory optionFactory)
    {
        var cwd = new DirectoryInfo(Environment.CurrentDirectory);
        _workingDirectoryOption = optionFactory.Create<DirectoryInfo>(
            name: "--working-directory",
            aliases: ["-w", "--cwd"],
            description: "The working directory of the Altinn repository.",
            isRequired: false,
            defaultValueBox: new(cwd));

        _workingDirectoryOption.Recursive = true;
        _workingDirectoryOption.HelpCustomization.GetDefaultValue = () => [new("./", Color.Cyan)];
        _workingDirectoryOption.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(["PATH"]);

        _workingDirectoryOption.Validators.Add(result =>
        {
            var dir = result.GetValueOrDefault<DirectoryInfo>();
            if (dir is null)
            {
                throw new UnreachableException();
            }

            if (!dir.Exists)
            {
                result.AddError($"The specified working directory '{dir.FullName}' does not exist.");
            }
        });
    }

    internal void Configure(ICommandBuilder commandBuilder)
    {
        commandBuilder.Options.Add(_workingDirectoryOption);
    }

    public DirectoryInfo GetWorkingDirectory(CommandInvocationContext invocationContext)
    {
        return invocationContext.ParseResult.GetRequiredValue(_workingDirectoryOption);
    }

    public Task<Result<AltinnRepository>> GetRepository(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
    {
        var loader = invocationContext.ApplicationServices.GetRequiredService<AltinnRepositoryLoader>();
        var cwd = GetWorkingDirectory(invocationContext);

        var state = invocationContext.Extensions.GetOrAdd((loader, cwd), static arg => new AltinnRepositoryLoaderState(arg.loader, arg.cwd));
        return state.RepositoryTask.WaitAsync(cancellationToken);
    }

    async Task ICommandHandlerParameterResolver.ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
    {
        var result = await GetRepository(context.InvocationContext, cancellationToken);
        if (result.IsProblem)
        {
            context.AddError(result.Problem.ToString());
        }
        else
        {
            context.SetParameterValue(result.Value);
        }
    }

    private sealed class AltinnRepositoryLoaderState(AltinnRepositoryLoader loader, DirectoryInfo cwd)
    {
        public Task<Result<AltinnRepository>> RepositoryTask { get; }
            = loader.Load(cwd);
    }
}
