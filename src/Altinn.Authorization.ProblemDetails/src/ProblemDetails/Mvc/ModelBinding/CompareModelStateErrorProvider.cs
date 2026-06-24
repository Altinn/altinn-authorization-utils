namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class CompareModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<CompareAttribute>(StdValidationErrors.Compare);
