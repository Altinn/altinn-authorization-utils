namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class MaxLengthModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<MaxLengthAttribute>(StdValidationErrors.MaxLength);
