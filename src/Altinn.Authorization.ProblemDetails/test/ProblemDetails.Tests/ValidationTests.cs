using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationTests
{
    [Fact]
    public void Foo()
    {
        var model = new Root { };

        var result = Validator.Validate(model);
    }

    public record Root
        : IValidatableModel
    {
        public Child? Child { get; init; }

        public List<Child>? Children { get; init; }

        public void Validate(ref ValidationContext context)
        {
            context.Validate(Child, "child");

            context.Check(Children is not null, StdValidationErrors.Required, "children");
            if (Children is not null)
            {
                context.Check(Children.Count > 0, TestValidationErrors.Empty, "children");
                context.ValidateItems(Children, "children", static (ref ValidationContext ctx, Child child) =>
                {
                    child.Validate(ref ctx);
                });
            }
        }
    }

    public record Child
        : IValidatableModel
    {
        public string? Required { get; init; }

        public void Validate(ref ValidationContext context)
        {
            context.Check(!string.IsNullOrEmpty(Required), StdValidationErrors.Required, "required");
        }
    }
}
