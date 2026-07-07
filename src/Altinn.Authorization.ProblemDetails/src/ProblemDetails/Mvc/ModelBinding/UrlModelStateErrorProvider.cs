namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class UrlModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<UrlAttribute>(StdValidationErrors.Url);
