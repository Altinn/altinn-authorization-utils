namespace Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

internal sealed class EmailAddressModelStateErrorProvider()
    : ValidationAttributeModelStateErrorProvider<EmailAddressAttribute>(StdValidationErrors.EmailAddress);
