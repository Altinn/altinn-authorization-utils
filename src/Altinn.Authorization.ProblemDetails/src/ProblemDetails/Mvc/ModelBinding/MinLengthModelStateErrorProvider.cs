namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class MinLengthModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<MinLengthAttribute>(StdValidationErrors.MinLength);
