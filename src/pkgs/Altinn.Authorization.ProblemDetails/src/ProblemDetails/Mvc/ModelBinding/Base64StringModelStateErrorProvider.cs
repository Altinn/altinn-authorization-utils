namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class Base64StringModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<Base64StringAttribute>(StdValidationErrors.Base64String);
