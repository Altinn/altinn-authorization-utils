namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        if (context.MemberInfo is { } memberInfo)
        {
            return IsValidationAttributeError(memberInfo, GetDisplayName(memberInfo), context.ModelError);
        }

        if (context.ParameterDescriptor is ControllerParameterDescriptor { ParameterInfo: { } parameterInfo })
        {
            return IsValidationAttributeError(parameterInfo, GetDisplayName(parameterInfo), context.ModelError);
        }

        return false;
    }

    private static bool IsValidationAttributeError(
        ICustomAttributeProvider attributeProvider,
        string displayName,
        ModelError error)
    {
        foreach (var attribute in GetValidationAttributes(attributeProvider))
        {
            if (string.Equals(
                error.ErrorMessage,
                attribute.FormatErrorMessage(displayName),
                StringComparison.Ordinal))
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

    private static string GetDisplayName(MemberInfo member)
        => member.GetCustomAttribute<DisplayAttribute>()?.GetName()
            ?? member.Name;

    private static string GetDisplayName(ParameterInfo parameter)
        => parameter.GetCustomAttribute<DisplayAttribute>()?.GetName()
            ?? parameter.Name
            ?? string.Empty;
}
