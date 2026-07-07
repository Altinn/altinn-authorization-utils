namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

internal abstract class ValidationAttributeModelStateErrorProvider<TAttribute>(ValidationErrorDescriptor descriptor)
    : IModelStateErrorValidationErrorProvider
    where TAttribute : ValidationAttribute
{
    public void BuildValidationError(ValidationErrorContext context)
    {
        if (context.Descriptor is not null || context.ModelError.Exception is not null)
        {
            return;
        }

        if (IsValidationAttributeError(context))
        {
            context.Descriptor = descriptor;
        }
    }

    private static bool IsValidationAttributeError(ValidationErrorContext context)
    {
        if (context.DisplayName is null)
        {
            return false;
        }

        if (context.ModelMetadata is { } metadata)
        {
            return IsValidationAttributeError(
                context,
                metadata.ValidatorMetadata.OfType<TAttribute>(),
                context.DisplayName,
                context.ModelError);
        }

        if (context.MemberInfo is { } memberInfo)
        {
            return IsValidationAttributeError(
                context,
                GetValidationAttributes(memberInfo),
                context.DisplayName,
                context.ModelError);
        }

        if (context.ParameterDescriptor is ControllerParameterDescriptor { ParameterInfo: { } parameterInfo })
        {
            return IsValidationAttributeError(
                context,
                GetValidationAttributes(parameterInfo),
                context.DisplayName,
                context.ModelError);
        }

        return false;
    }

    private static bool IsValidationAttributeError(
        ValidationErrorContext context,
        IEnumerable<TAttribute> attributes,
        string displayName,
        ModelError error)
    {
        foreach (var attribute in attributes)
        {
            if (string.Equals(
                error.ErrorMessage,
                attribute.FormatErrorMessage(displayName),
                StringComparison.Ordinal))
            {
                return true;
            }

            if (TryGetAdapterErrorMessage(context, attribute, out var adapterErrorMessage)
                && string.Equals(error.ErrorMessage, adapterErrorMessage, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<TAttribute> GetValidationAttributes(ICustomAttributeProvider attributeProvider)
        => attributeProvider
            .GetCustomAttributes(typeof(TAttribute), inherit: true)
            .OfType<TAttribute>();

    private static bool TryGetAdapterErrorMessage(
        ValidationErrorContext context,
        TAttribute attribute,
        out string errorMessage)
    {
        var services = context.ActionContext.HttpContext.RequestServices;
        if (services is null)
        {
            errorMessage = string.Empty;
            return false;
        }

        var adapterProvider = services.GetService<IValidationAttributeAdapterProvider>();
        var metadataProvider = context.ModelMetadataProvider ?? services.GetService<IModelMetadataProvider>();
        var metadata = context.ModelMetadata;

        if (adapterProvider is null || metadataProvider is null || metadata is null)
        {
            errorMessage = string.Empty;
            return false;
        }

        var adapter = adapterProvider.GetAttributeAdapter(attribute, GetStringLocalizer(context, services));
        if (adapter is null)
        {
            errorMessage = string.Empty;
            return false;
        }

        var validationContext = new ModelValidationContextBase(context.ActionContext, metadata, metadataProvider);
        errorMessage = adapter.GetErrorMessage(validationContext);
        return true;
    }

    private static IStringLocalizer? GetStringLocalizer(ValidationErrorContext context, IServiceProvider services)
    {
        if (services.GetService<IStringLocalizerFactory>() is not { } stringLocalizerFactory)
        {
            return null;
        }

        var modelType = context.ModelMetadata?.ContainerType
            ?? context.MemberInfo?.DeclaringType
            ?? context.ModelMetadata?.ModelType
            ?? context.PropertyType;

        if (modelType is null)
        {
            return null;
        }

        return services
            .GetService<IOptions<MvcDataAnnotationsLocalizationOptions>>()?
            .Value
            .DataAnnotationLocalizerProvider(modelType, stringLocalizerFactory);
    }
}
