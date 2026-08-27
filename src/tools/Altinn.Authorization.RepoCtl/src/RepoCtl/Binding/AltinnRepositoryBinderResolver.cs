using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Binding;

internal sealed class AltinnRepositoryBinderResolver(AltinnRepositoryResolver repositoryResolver)
    : ICommandHandlerParameterBinderResolver
{
    public bool TryResolve(
        ParameterInfo parameter,
        [NotNullWhen(true)] out ICommandHandlerParameterBinder? parameterBinder)
    {
        if (parameter.ParameterType == typeof(AltinnRepository))
        {
            parameterBinder = new RepositoryBinder(repositoryResolver);
            return true;
        }

        if (parameter.ParameterType == typeof(AltinnVerticalSet))
        {
            parameterBinder = new VerticalSetBinder();
            return true;
        }

        if (parameter.ParameterType == typeof(AltinnVertical))
        {
            parameterBinder = new VerticalBinder();
            return true;
        }

        if (parameter.ParameterType == typeof(GitHubContext))
        {
            parameterBinder = new GitHubContextBinder();
            return true;
        }

        parameterBinder = null;
        return false;
    }

    private sealed class RepositoryBinder
        : ICommandHandlerParameterBinder
    {
        private readonly AltinnRepositoryResolver _repositoryResolver;

        public RepositoryBinder(AltinnRepositoryResolver repositoryResolver)
        {
            _repositoryResolver = repositoryResolver;
        }

        public ICommandHandlerParameterResolver Bind(
            ICommandHandlerParameterBinderContext context,
            StrongBox<object?>? defaultValueBox,
            string? description)
        {
            return _repositoryResolver;
        }
    }

    private sealed class VerticalSetBinder
        : ICommandHandlerParameterBinder
    {
        public ICommandHandlerParameterResolver Bind(
            ICommandHandlerParameterBinderContext context,
            StrongBox<object?>? defaultValueBox,
            string? description)
        {
            var factory = context.ApplicationServices.GetRequiredService<OptionFactory>();
            var kindOption = factory.Create<AltinnVerticalKindSet>(
                name: "--kind",
                aliases: [],
                description: "Limit the verticals to those of the specified kinds.",
                isRequired: false,
                defaultValueBox: new(AltinnVerticalKindSet.All));

            kindOption.HelpCustomization.GetDefaultValue = () => [new("all", Color.Cyan)];

            var dirOption = factory.Create<string?>(
                name: "--dir",
                aliases: [],
                description: "Sub-directory of the repository (either relative or absolute) to limit the verticals to.",
                isRequired: false,
                defaultValueBox: new());

            dirOption.HelpCustomization.GetDefaultValue = () => [new("./", Color.Cyan)];

            context.Add(kindOption);
            context.Add(dirOption);
            return new VerticalSetResolver(kindOption, dirOption);
        }
    }

    private sealed class VerticalSetResolver(Option<AltinnVerticalKindSet> kindOption, Option<string?> dirOption)
        : ICommandHandlerParameterResolver
    {
        public async Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
        {
            var repoResolver = context.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>();
            var repo = await repoResolver.GetRepository(context.InvocationContext, cancellationToken);

            if (repo.IsProblem)
            {
                context.AddError(repo.Problem.ToString());
                return;
            }

            var kinds = context.ParseResult.GetRequiredValue(kindOption);
            var set = repo.Value.Verticals.OfKind(kinds);

            if (context.ParseResult.GetRequiredValue(dirOption) is { } dir)
            {
                dir = Path.GetFullPath(dir, repoResolver.GetWorkingDirectory(context.InvocationContext).FullName);
                set = set.InDirectory(new(dir));
            }

            context.SetParameterValue(set);
        }
    }

    private sealed class VerticalBinder
        : ICommandHandlerParameterBinder
    {
        public ICommandHandlerParameterResolver Bind(
            ICommandHandlerParameterBinderContext context,
            StrongBox<object?>? defaultValueBox,
            string? description)
        {
            var factory = context.ApplicationServices.GetRequiredService<ArgumentFactory>();
            var verticalArgument = factory.Create<string>(
                name: "vertical",
                description: "The vertical to operate on.",
                defaultValueBox: null);

            context.Add(verticalArgument);
            return new VerticalResolver(verticalArgument);
        }
    }

    private sealed class VerticalResolver(Argument<string> verticalArgument)
        : ICommandHandlerParameterResolver
    {
        public async Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
        {
            var repoResolver = context.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>();
            var repo = await repoResolver.GetRepository(context.InvocationContext, cancellationToken);

            if (repo.IsProblem)
            {
                context.AddError(repo.Problem.ToString());
                return;
            }

            var lookup = context.ParseResult.GetRequiredValue(verticalArgument);

            // First, we try to interpret the argument as a vertical ID
            if (AltinnVerticalId.TryParse(lookup, null, out var verticalId)
                && repo.Value.Verticals.TryGet(verticalId, out var vertical))
            {
                context.SetParameterValue(vertical);
                return;
            }

            // if not, interpret it as a directory path
            var dir = Path.GetFullPath(lookup, repoResolver.GetWorkingDirectory(context.InvocationContext).FullName);
            var candidates = repo.Value.Verticals.InDirectory(new(dir));

            if (candidates.Count == 0)
            {
                context.AddError($"No vertical found in directory '{dir}'.");
                return;
            }

            if (candidates.Count > 1)
            {
                var builder = new StringBuilder($"Multiple verticals found in directory '{dir}':");
                foreach (var candidate in candidates)
                {
                    builder.AppendLine().Append("  - ").Append(candidate.Id);
                }

                context.AddError(builder.ToString());
                return;
            }

            context.SetParameterValue(candidates.AsEnumerable().First());
            return;
        }
    }

    private sealed class GitHubContextBinder
        : ICommandHandlerParameterBinder
    {
        public ICommandHandlerParameterResolver Bind(
            ICommandHandlerParameterBinderContext context,
            StrongBox<object?>? defaultValueBox,
            string? description)
        {
            var factory = context.ApplicationServices.GetRequiredService<OptionFactory>();

            var repoOption = factory.Create<string?>(
                name: "--repo",
                aliases: [],
                description: "The name of the GitHub repository.",
                isRequired: false,
                defaultValueBox: null);

            repoOption.HelpCustomization.GetDefaultValue = () => ["From ", "$", new("GITHUB_REPOSITORY", Color.Cyan)];

            context.Add(repoOption);
            return new GitHubContextResolver(repoOption);
        }
    }

    private sealed class GitHubContextResolver(Option<string?> repoOption)
        : ICommandHandlerParameterResolver
    {
        public Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
        {
            var value = context.ParseResult.GetValue(repoOption);
            if (value is null)
            {
                value = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            }

            if (string.IsNullOrEmpty(value))
            {
                context.AddError("GitHub repository name must be specified via the --repo option or the GITHUB_REPOSITORY environment variable.");
                return Task.CompletedTask;
            }

            var parts = value.Split('/', 3);
            if (parts.Length != 2)
            {
                context.AddError($"GitHub repository name '{value}' is not in the format 'owner/repo'.");
                return Task.CompletedTask;
            }

            context.SetParameterValue(new GitHubContext
            {
                RepositoryOwner = parts[0],
                RepositoryName = parts[1],
            });

            return Task.CompletedTask;
        }
    }
}
