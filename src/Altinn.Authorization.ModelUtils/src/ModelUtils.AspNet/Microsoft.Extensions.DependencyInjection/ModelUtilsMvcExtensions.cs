using Altinn.Authorization.ModelUtils.AspNet;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for <see cref="IMvcBuilder"/>.
/// </summary>
public static class ModelUtilsMvcExtensions
{
    /// <summary>
    /// Configures the MVC builder for model-binding of models from Altinn.Authorization.ModelUtils.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static IMvcBuilder AddAuthorizationModelUtilsBinders(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(o =>
        {
            o.ModelBinderProviders.RemoveType<FlagsEnum.ModelBinderProvider>();
            o.ModelBinderProviders.Insert(0, new FlagsEnum.ModelBinderProvider());
        });

        return builder;
    }
}
