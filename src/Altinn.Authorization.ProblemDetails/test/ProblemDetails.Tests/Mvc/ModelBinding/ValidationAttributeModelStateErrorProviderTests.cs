using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Altinn.Authorization.ProblemDetails.Mvc;
using Altinn.Authorization.ProblemDetails.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Altinn.Authorization.ProblemDetails.Tests.Mvc.ModelBinding;

public class ValidationAttributeModelStateErrorProviderTests
{
    public static TheoryData<IModelStateErrorValidationErrorProvider, string, Type, ValidationErrorDescriptor> Providers
        => new()
        {
            { new StringLengthModelStateErrorProvider(), nameof(Input.StringLength), typeof(StringLengthAttribute), StdValidationErrors.StringLength },
            { new MinLengthModelStateErrorProvider(), nameof(Input.MinLength), typeof(MinLengthAttribute), StdValidationErrors.MinLength },
            { new MaxLengthModelStateErrorProvider(), nameof(Input.MaxLength), typeof(MaxLengthAttribute), StdValidationErrors.MaxLength },
            { new LengthModelStateErrorProvider(), nameof(Input.Length), typeof(LengthAttribute), StdValidationErrors.Length },
            { new RangeModelStateErrorProvider(), nameof(Input.Range), typeof(RangeAttribute), StdValidationErrors.Range },
            { new RegularExpressionModelStateErrorProvider(), nameof(Input.RegularExpression), typeof(RegularExpressionAttribute), StdValidationErrors.RegularExpression },
            { new CompareModelStateErrorProvider(), nameof(Input.Compare), typeof(CompareAttribute), StdValidationErrors.Compare },
            { new EmailAddressModelStateErrorProvider(), nameof(Input.EmailAddress), typeof(EmailAddressAttribute), StdValidationErrors.EmailAddress },
            { new PhoneModelStateErrorProvider(), nameof(Input.Phone), typeof(PhoneAttribute), StdValidationErrors.Phone },
            { new UrlModelStateErrorProvider(), nameof(Input.Url), typeof(UrlAttribute), StdValidationErrors.Url },
            { new CreditCardModelStateErrorProvider(), nameof(Input.CreditCard), typeof(CreditCardAttribute), StdValidationErrors.CreditCard },
            { new AllowedValuesModelStateErrorProvider(), nameof(Input.AllowedValues), typeof(AllowedValuesAttribute), StdValidationErrors.AllowedValues },
            { new DeniedValuesModelStateErrorProvider(), nameof(Input.DeniedValues), typeof(DeniedValuesAttribute), StdValidationErrors.DeniedValues },
            { new Base64StringModelStateErrorProvider(), nameof(Input.Base64String), typeof(Base64StringAttribute), StdValidationErrors.Base64String },
        };

    [Theory]
    [MemberData(nameof(Providers))]
    public void BuildValidationError_MatchingAttributeError_SetsInvalidValue(
        IModelStateErrorValidationErrorProvider provider,
        string propertyName,
        Type attributeType,
        ValidationErrorDescriptor expectedDescriptor)
    {
        var property = GetProperty(propertyName);
        var attribute = GetAttribute(property, attributeType);
        var context = CreateContext(property, attribute.FormatErrorMessage(property.Name));

        provider.BuildValidationError(context);

        context.Descriptor.ShouldNotBeNull();
        context.Descriptor.ErrorCode.ShouldBe(expectedDescriptor.ErrorCode);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void BuildValidationError_NonMatchingError_DoesNotSetDescriptor(
        IModelStateErrorValidationErrorProvider provider,
        string propertyName,
        Type attributeType,
        ValidationErrorDescriptor expectedDescriptor)
    {
        _ = attributeType;
        _ = expectedDescriptor;

        var property = GetProperty(propertyName);
        var context = CreateContext(property, "Some other validation error.");

        provider.BuildValidationError(context);

        context.Descriptor.ShouldBeNull();
    }

    private static ValidationErrorContext CreateContext(MemberInfo member, string errorMessage)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(member.Name, errorMessage);

        var context = new ValidationErrorContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), modelState),
            modelState,
            member.Name,
            modelState[member.Name]!,
            modelState[member.Name]!.Errors[0]);

        context.MemberInfo = member;
        context.DisplayName = member.Name;
        context.Paths.Add($"/{member.Name}");

        return context;
    }

    private static PropertyInfo GetProperty(string propertyName)
        => typeof(Input).GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Could not find property '{propertyName}'.");

    private static ValidationAttribute GetAttribute(MemberInfo member, Type attributeType)
        => member.GetCustomAttributes(attributeType, inherit: true)
            .OfType<ValidationAttribute>()
            .Single();

    private sealed class Input
    {
        [StringLength(3)]
        public string? StringLength { get; init; }

        [MinLength(3)]
        public string? MinLength { get; init; }

        [MaxLength(3)]
        public string? MaxLength { get; init; }

        [Length(2, 3)]
        public string? Length { get; init; }

        [Range(1, 3)]
        public int Range { get; init; }

        [RegularExpression("^a+$")]
        public string? RegularExpression { get; init; }

        [Compare(nameof(CompareTo))]
        public string? Compare { get; init; }

        public string? CompareTo { get; init; }

        [EmailAddress]
        public string? EmailAddress { get; init; }

        [Phone]
        public string? Phone { get; init; }

        [Url]
        public string? Url { get; init; }

        [CreditCard]
        public string? CreditCard { get; init; }

        [AllowedValues("allowed")]
        public string? AllowedValues { get; init; }

        [DeniedValues("denied")]
        public string? DeniedValues { get; init; }

        [Base64String]
        public string? Base64String { get; init; }
    }
}
