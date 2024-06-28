using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Binders;

[ExcludeFromCodeCoverage]
internal class RequiredServiceDescriptor<T>
    : BinderBase<T>
    where T : notnull
{
    protected override T GetBoundValue(BindingContext bindingContext)
        => bindingContext.GetRequiredService<T>();
}
