namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class AllowedValuesModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<AllowedValuesAttribute>(StdValidationErrors.AllowedValues);
