using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
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
            var option = factory.Create<AltinnVerticalKindSet>(
                name: "--kind",
                aliases: [],
                description: "Limit the verticals to those of the specified kinds.",
                isRequired: false,
                defaultValueBox: new(AltinnVerticalKindSet.All));

            option.HelpCustomization.GetDefaultValue = () => ["all"];

            context.Add(option);
            return new VerticalSetResolver(option);
        }
    }

    private sealed class VerticalSetResolver(Option<AltinnVerticalKindSet> option)
        : ICommandHandlerParameterResolver
    {
        public async Task<object?> ResolveParameterValue(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var repoResolver = invocationContext.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>();
            var repo = await repoResolver.ResolveParameterValue(invocationContext, cancellationToken);
            repo.EnsureSuccess();

            var kinds = invocationContext.ParseResult.GetRequiredValue(option);
            return repo.Value.Verticals.OfKind(kinds);
        }
    }
}
