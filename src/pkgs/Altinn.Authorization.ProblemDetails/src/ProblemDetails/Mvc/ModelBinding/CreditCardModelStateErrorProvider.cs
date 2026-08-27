namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class CreditCardModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<CreditCardAttribute>(StdValidationErrors.CreditCard);
