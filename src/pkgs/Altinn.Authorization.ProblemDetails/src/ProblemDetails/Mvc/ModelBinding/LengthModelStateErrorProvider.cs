namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class LengthModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<LengthAttribute>(StdValidationErrors.Length);
