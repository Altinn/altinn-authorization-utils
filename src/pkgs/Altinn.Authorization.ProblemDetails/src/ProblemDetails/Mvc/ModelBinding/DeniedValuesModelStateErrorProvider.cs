namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class DeniedValuesModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<DeniedValuesAttribute>(StdValidationErrors.DeniedValues);
