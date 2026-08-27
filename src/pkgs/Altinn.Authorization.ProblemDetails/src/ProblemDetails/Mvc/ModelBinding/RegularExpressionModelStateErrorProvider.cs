namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class RegularExpressionModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<RegularExpressionAttribute>(StdValidationErrors.RegularExpression);
