# Altinn.Authorization.ProblemDetails Library

The `Altinn.Authorization.ProblemDetails` library provides a convenient way to generate `ProblemDetails` objects with custom
error codes. These `ProblemDetails` objects can be used to represent errors in ASP.NET Core applications following the
[RFC 7807](https://tools.ietf.org/html/rfc7807) specification.

## Installation

You can install the library via NuGet Package Manager Console:

```pwsh
Install-Package Altinn.Authorization.ProblemDetails
```

Or via the .NET CLI:

```bash
dotnet add package Altinn.Authorization.ProblemDetails
```

## Usage

### ProblemDetails

This library allows for defining custom errors that contain error codes usable by clients to determine what went wrong. This is done by creating custom `ProblemDescriptor`s, which can trivially be converted into `ProblemDetail`s by calling `ToProblemDetails()` on them.

#### ProblemDetails Example

Here's a basic example demonstrating how to use the `Altinn.Authorization.ProblemDetails` library:

```csharp
internal static class MyAppErrors
{
    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New("APP");

    public static ProblemDescriptor BadRequest { get; }
        = _factory.Create(0, HttpStatusCode.BadRequest, "Bad request");

    public static ProblemDescriptor NotFound { get; }
        = _factory.Create(1, HttpStatusCode.NotFound, "Not found");

    public static ProblemDescriptor InternalServerError { get; }
        = _factory.Create(2, HttpStatusCode.InternalServerError, "Internal server error");

    public static ProblemDescriptor NotImplemented { get; }
        = _factory.Create(3, HttpStatusCode.NotImplemented, "Not implemented");
}
```

#### Explanation

- `ProblemDescriptorFactory`: This class provides a factory method `New()` to create a new instance of `ProblemDescriptorFactory`.

- `Create()`: This method is used to create a new `ProblemDescriptor` object with a custom error code, HTTP status code, and error message. These can then be turned into `ProblemDetails` objects by calling `ToProblemDetails()`.

### Validation Errors

A predefined `AltinnValidationProblemDetails` is provided for the case where you have one or more validation errors that should be returned to the client. This variant of `ProblemDetails` takes a list of validation errors, which can be created in a similar fasion to `ProblemDescriptor`s.

#### ValidationErrors Example

Here's a basic example demonstrating how to create custom validation errors:

```csharp
internal static class MyAppValidationDescriptors
{
    private static readonly ValidationErrorDescriptorFactory _factory
        = ValidationErrorDescriptorFactory.New("APP");

    public static ValidationErrorDescriptor FieldRequired { get; }
        = _factory.Create(0, "Field is required.");

    public static ValidationErrorDescriptor FieldOutOfRange { get; }
        = _factory.Create(1, "Field is out of range.");

    public static ValidationErrorDescriptor PasswordsMustMatch { get; }
        = _factory.Create(2, "Passwords must match.");
}
```

And how to use them:

```csharp
var details = new AltinnValidationProblemDetails([
    MyAppValidationDescriptors.FieldRequired.ToValidationError("/field1"),
    MyAppValidationDescriptors.FieldRequired.ToValidationError("/field2"),
    MyAppValidationDescriptors.PasswordsMustMatch.ToValidationError(["/password", "/confirmPassword"]),
]);
```

A set of common validation errors are also provided through the `StdValidationErrors` class.

### Customization

You can customize the prefix used for error codes by passing a custom prefix to the `New()` method of `ProblemDescriptorFactory`. All application-domains should have their own prefix.

```csharp
ProblemDescriptorFactory.New("PFX");
```

The prefix is required to be only uppercase ASCII letters of either 2, 3, or 4 characters in length.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
