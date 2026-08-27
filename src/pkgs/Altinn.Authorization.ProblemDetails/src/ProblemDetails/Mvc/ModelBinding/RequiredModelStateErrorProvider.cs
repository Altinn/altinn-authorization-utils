namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class RequiredModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<RequiredAttribute>(StdValidationErrors.Required);
