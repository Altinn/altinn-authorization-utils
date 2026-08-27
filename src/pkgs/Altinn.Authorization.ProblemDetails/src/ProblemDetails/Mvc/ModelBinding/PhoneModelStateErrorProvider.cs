namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class PhoneModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<PhoneAttribute>(StdValidationErrors.Phone);
