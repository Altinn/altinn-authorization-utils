using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.ProblemDetails.Mvc;
using Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding ProblemDetails services to an <see cref="IServiceCollection"/>.
/// </summary>
public static class ProblemDetailsServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds ProblemDetails services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// This configures MVC to use the <see cref="AltinnValidationProblemDetailsFactory"/> for generating ProblemDetails responses for validation errors.
        /// It also registers default model state error providers for common validation attributes.
        /// </remarks>
        /// <returns>A <see cref="IProblemDetailsConfigurationBuilder"/> for further configuration.</returns>
        public IProblemDetailsConfigurationBuilder AddAltinnProblemDetails()
        {
            if (services.Contains(Marker.Descriptor))
            {
                return new Builder(services);
            }

            services.Add(Marker.Descriptor);

            services.AddProblemDetails(); // MVC problem details services
            services.AddOptions<JsonOptions>().Configure(static o =>
            {
                o.AllowInputFormatterExceptionMessages = false;
                o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, new AltinnProblemDetailsJsonContext());
            });
            services.AddOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>().Configure(static o =>
            {
                o.SerializerOptions.TypeInfoResolverChain.Insert(0, new AltinnProblemDetailsJsonContext());
            });
            services.Insert(0, ServiceDescriptor.Singleton<IProblemDetailsWriter, AltinnProblemDetailsWriter>());
            services.AddSingleton<IConfigureOptions<ApiBehaviorOptions>, ConfigureApiBehaviorOptionsForProblemDetails>();
            services.AddSingleton<AltinnValidationProblemDetailsFactory>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, RequiredModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, StringLengthModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, MinLengthModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, MaxLengthModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, LengthModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, RangeModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, RegularExpressionModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, CompareModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, EmailAddressModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, PhoneModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, UrlModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, CreditCardModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, AllowedValuesModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, DeniedValuesModelStateErrorProvider>();
            services.AddSingleton<IModelStateErrorValidationErrorProvider, Base64StringModelStateErrorProvider>();

            return new Builder(services);
        }
    }

    /// <param name="builder">The <see cref="IProblemDetailsConfigurationBuilder"/> to extend.</param>
    extension(IProblemDetailsConfigurationBuilder builder)
    {
        /// <summary>
        /// Adds a custom model state error provider to the ProblemDetails configuration.
        /// </summary>
        /// <typeparam name="TProvider">The type of the model state error provider.</typeparam>
        /// <returns>The <see cref="IProblemDetailsConfigurationBuilder"/> for further configuration.</returns>
        public IProblemDetailsConfigurationBuilder AddValidationErrorProvider<TProvider>()
            where TProvider : class, IModelStateErrorValidationErrorProvider
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IModelStateErrorValidationErrorProvider, TProvider>());

            return builder;
        }
    }

    private sealed class Builder(IServiceCollection services)
        : IProblemDetailsConfigurationBuilder
    {
        public IServiceCollection Services => services;
    }

    private sealed class Marker
    {
        public static ServiceDescriptor Descriptor { get; } = ServiceDescriptor.Singleton<Marker, Marker>();
    }
}
