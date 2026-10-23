using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.DependencyInjection;

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

        if (parameter.ParameterType == typeof(GitHubRepositoryContext))
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
            DefaultValueProvider? defaultValueProvider,
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
            DefaultValueProvider? defaultValueProvider,
            string? description)
        {
            var factory = context.ApplicationServices.GetRequiredService<OptionFactory>();
            var kindOption = factory.Create<AltinnVerticalKindSet>(
                name: "--kind",
                aliases: [],
                description: "Limit the verticals to those of the specified kinds.",
                isRequired: false,
                defaultValueProvider: DefaultValueProvider.FromValue(AltinnVerticalKindSet.All));

            kindOption.HelpCustomization.GetDefaultValue = context => [new("all", context.DefaultArgumentColor)];

            var dirOption = factory.Create<string?>(
                name: "--dir",
                aliases: [],
                description: "Sub-directory of the repository (either relative or absolute) to limit the verticals to.",
                isRequired: false,
                defaultValueProvider: DefaultValueProvider.FromValue<string?>(null));

            dirOption.HelpCustomization.GetDefaultValue = context => [new("./", context.DefaultArgumentColor)];

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
            DefaultValueProvider? defaultValueProvider,
            string? description)
        {
            var arguments = context.ApplicationServices.GetRequiredService<ArgumentFactory>();
            var options = context.ApplicationServices.GetRequiredService<OptionFactory>();
            var verticalArgument = arguments.Create<string?>(
                name: "vertical",
                description: "Vertical selector: ID or directory path. Mutually exclusive with other vertical selectors; exactly one is required.",
                defaultValueProvider: DefaultValueProvider.FromValue<string?>(null));
            verticalArgument.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(["vertical"], arity: ArgumentArity.ZeroOrOne);

            var idOption = options.Create<string?>(
                name: "--vertical-id",
                aliases: [],
                description: "Vertical selector: exact ID. Mutually exclusive with other vertical selectors; exactly one is required.",
                isRequired: false,
                defaultValueProvider: DefaultValueProvider.FromValue<string?>(null));
            idOption.Arity = ArgumentArity.ExactlyOne;
            idOption.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(["id"]);

            var tagOption = options.Create<string?>(
                name: "--vertical-tag",
                aliases: [],
                description: "Vertical selector: release tag. Mutually exclusive with other vertical selectors; exactly one is required.",
                isRequired: false,
                defaultValueProvider: DefaultValueProvider.FromValue<string?>(null));
            tagOption.Arity = ArgumentArity.ExactlyOne;
            tagOption.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(["tag"]);

            IReadOnlyList<(VerticalLookup.LookupKind Kind, Option<string?> Option)> lookupOptions = [
                (VerticalLookup.LookupKind.Id, idOption),
                (VerticalLookup.LookupKind.Tag, tagOption),
            ];

            // Collect positionals first so OnlyTake can forward the remainder in order. This is a workaround for https://github.com/dotnet/command-line-api/issues/2734
            verticalArgument.Arity = ArgumentArity.ZeroOrMore;
            verticalArgument.CustomParser = result =>
            {
                // With an explicit selector, positionals belong to the forwarded arguments.
                var parent = result.Parent;
                if (parent is not null && lookupOptions.Any(tuple => parent.GetValue(tuple.Option) is not null))
                {
                    result.OnlyTake(0);
                    return null;
                }

                var value = result.Tokens.Count == 0 ? null : result.Tokens[0].Value;
                result.OnlyTake(value is null ? 0 : 1);
                return value;
            };

            context.Add(verticalArgument);
            foreach (var (_, option) in lookupOptions)
            {
                context.Add(option);
            }

            return new VerticalResolver(verticalArgument, lookupOptions);
        }
    }

    private sealed class VerticalResolver(Argument<string?> verticalArgument, IReadOnlyList<(VerticalLookup.LookupKind Kind, Option<string?> Option)> lookupOptions)
        : ICommandHandlerParameterResolver
    {
        public async Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
        {
            var kind = VerticalLookup.LookupKind.IdOrPath;
            var value = context.ParseResult.GetValue(verticalArgument);

            foreach (var (optionKind, option) in lookupOptions)
            {
                var optionValue = context.ParseResult.GetValue(option);
                if (optionValue is not null && value is not null)
                {
                    Error(context);
                    return;
                }

                if (optionValue is not null)
                {
                    kind = optionKind;
                    value = optionValue;
                }
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                context.AddError("The vertical selector must not be empty or whitespace.");
                return;
            }

            var repoResolver = context.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>();
            var repo = await repoResolver.GetRepository(context.InvocationContext, cancellationToken);

            if (repo.IsProblem)
            {
                context.AddError(repo.Problem.ToString());
                return;
            }

            if (!VerticalLookup.TryLookup(repo.Value.Verticals, kind, value, repoResolver.GetWorkingDirectory(context.InvocationContext), out var result, out var error))
            {
                context.AddError(error);
                return;
            }

            context.SetParameterValue(result);
        }

        private void Error(CommandHandlerParameterResolverContext context)
        {
            var builder = new StringBuilder("Specify exactly one of <vertical>");
            foreach (var option in lookupOptions.SkipLast(1))
            {
                builder.Append(", ").Append(option.Option.HelpName ?? option.Option.Name);
            }

            var lastOption = lookupOptions.Last();
            builder.Append(", or ").Append(lastOption.Option.HelpName ?? lastOption.Option.Name).Append('.');
            context.AddError(builder.ToString());
        }
    }

    private sealed class GitHubContextBinder
        : ICommandHandlerParameterBinder
    {
        public ICommandHandlerParameterResolver Bind(
            ICommandHandlerParameterBinderContext context,
            DefaultValueProvider? defaultValueProvider,
            string? description)
        {
            var repoOption = (Option<string?>)new OptionAttribute("--repo")
            {
                Description = "The name of the GitHub repository.",
                IsRequired = false,
                DefaultFromEnvironment = "GITHUB_REPOSITORY",
            }.CreateOption(context.ApplicationServices, typeof(string), "repo", DefaultValueProvider.FromValue<string?>(null));

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

            context.SetParameterValue(new GitHubRepositoryContext
            {
                RepositoryOwner = parts[0],
                RepositoryName = parts[1],
            });

            return Task.CompletedTask;
        }
    }
}
