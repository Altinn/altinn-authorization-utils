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

### Example

Here's a basic example demonstrating how to use the `Altinn.Authorization.ProblemDetails` library:

```csharp
internal static class MyAppErrors
{
    private static readonly AltinnProblemDetailsFactory _factory
        = AltinnProblemDetailsFactory.New("APP");

    public static AltinnProblemDetails InvalidUser
        => _factory.Create(1, HttpStatusCode.BadRequest, "Provided user is not valid");

    public static AltinnProblemDetails OrganizationNotFound
        => _factory.Create(2, HttpStatusCode.NotFound, "The specified organization was not found");

    public static AltinnProblemDetails InternalServerError
        => _factory.Create(3, HttpStatusCode.InternalServerError, "Internal server error");

    public static AltinnProblemDetails NotImplemented
        => _factory.Create(4, HttpStatusCode.NotImplemented, "Not implemented");
}
```

### Explanation

- `AltinnProblemDetailsFactory`: This class provides a factory method `New()` to create a new instance of `AltinnProblemDetailsFactory`.

- `Create()`: This method is used to create a new `ProblemDetails` object with a custom error code, HTTP status code, and error message.

### Customization

You can customize the prefix used for error codes by passing a custom prefix to the `New()` method of `AltinnProblemDetailsFactory`.

```csharp
AltinnProblemDetailsFactory.New("PFX");
```

The prefix is required to be only uppercase ASCII letters of either 2, 3, or 4 characters in length.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
