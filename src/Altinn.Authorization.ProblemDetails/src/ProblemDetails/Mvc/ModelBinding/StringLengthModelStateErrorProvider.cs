namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class StringLengthModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<StringLengthAttribute>(StdValidationErrors.StringLength);
