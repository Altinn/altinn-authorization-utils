using Altinn.Authorization.TestUtils.Shouldly;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class JsonSnapshotTests
{
    private static readonly ProblemDescriptorFactory _problems = ProblemDescriptorFactory.New("SNAP");
    private static readonly ValidationErrorDescriptorFactory _validations = ValidationErrorDescriptorFactory.New("SNAP");

    private static readonly ProblemDescriptor _simple = _problems.Create(0, HttpStatusCode.InsufficientStorage, "Simple error");
    private static readonly ValidationErrorDescriptor _invalid = _validations.Create(0, "Invalid value");

    protected static JsonSerializerOptions Options { get; }
#if NET9_0_OR_GREATER
        = JsonSerializerOptions.Web;
#else
        = new JsonSerializerOptions(JsonSerializerDefaults.Web);
#endif

    private static JsonWriterOptions WriterOptions { get; }
        = new JsonWriterOptions
        {
            Indented = true,
            SkipValidation = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

    [Fact]
    public async Task Simple()
    {
        await VerifyJson(
            _simple.Create().ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:SNAP-00000",
                "title": "Simple error",
                "status": 507,
                "code": "SNAP-00000",
                "statusDescription": "Insufficient Storage"
            }
            """);
    }

    [Fact]
    public async Task Simple_WithDetails()
    {
        await VerifyJson(
            _simple.ToProblemDetails(detail: "Instance detail"),
            """
            {
                "type": "urn:altinn:error:SNAP-00000",
                "title": "Simple error",
                "detail": "Instance detail",
                "status": 507,
                "code": "SNAP-00000",
                "statusDescription": "Insufficient Storage"
            }
            """);
    }

    [Fact]
    public async Task Simple_WithExtensions()
    {
        await VerifyJson(
            _simple.Create([new("ext1", "1"), new("ext2", "2")]).ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:SNAP-00000",
                "title": "Simple error",
                "status": 507,
                "code": "SNAP-00000",
                "statusDescription": "Insufficient Storage",
                "ext1": "1",
                "ext2": "2"
            }
            """);
    }

    [Fact]
    public async Task Simple_WithDetailsAndExtensions()
    {
        await VerifyJson(
            _simple.Create(detail: "Instance detail", [new("ext1", "1"), new("ext2", "2")]).ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:SNAP-00000",
                "title": "Simple error",
                "detail": "Instance detail",
                "status": 507,
                "code": "SNAP-00000",
                "statusDescription": "Insufficient Storage",
                "ext1": "1",
                "ext2": "2"
            }
            """);
    }

    [Fact]
    public async Task Validation_Single()
    {
        ValidationProblemBuilder builder = default;
        builder.Add(_invalid);
        builder.TryBuild(out var instance).ShouldBeTrue();

        await VerifyJson(
            instance.ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:STD-00000",
                "title": "One or more validation errors occurred.",
                "status": 400,
                "code": "STD-00000",
                "statusDescription": "Bad Request",
                "validationErrors": [
                    {
                      "code": "SNAP.VLD-00000",
                      "title": "Invalid value",
                      "paths": []
                    }
                ]
            }
            """);
    }

    [Fact]
    public async Task Validation_Single_WithPaths()
    {
        ValidationProblemBuilder builder = default;
        builder.Add(_invalid, ["/foo", "/bar"]);
        builder.TryBuild(out var instance).ShouldBeTrue();

        await VerifyJson(
            instance.ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:STD-00000",
                "title": "One or more validation errors occurred.",
                "status": 400,
                "code": "STD-00000",
                "statusDescription": "Bad Request",
                "validationErrors": [
                    {
                      "code": "SNAP.VLD-00000",
                      "title": "Invalid value",
                      "paths": ["/foo", "/bar"]
                    }
                ]
            }
            """);
    }

    [Fact]
    public async Task Validation_Single_WithPathsAndDetailsAndExtensions()
    {
        ValidationProblemBuilder builder = default;
        builder.Add(_invalid, ["/foo", "/bar"], [new("ext1", "1"), new("ext2", "2")], "Invalid cause reasons");
        builder.TryBuild(out var instance).ShouldBeTrue();

        await VerifyJson(
            instance.ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:STD-00000",
                "title": "One or more validation errors occurred.",
                "status": 400,
                "code": "STD-00000",
                "statusDescription": "Bad Request",
                "validationErrors": [
                    {
                      "code": "SNAP.VLD-00000",
                      "title": "Invalid value",
                      "paths": ["/foo", "/bar"],
                      "detail": "Invalid cause reasons",
                      "ext1": "1",
                      "ext2": "2"
                    }
                ]
            }
            """);
    }

    [Fact]
    public async Task Validation_WithDetailAndExtensions_Single_WithPaths()
    {
        ValidationProblemBuilder builder = default;
        builder.Add(_invalid, ["/foo", "/bar"]);
        builder.AddExtension("ext1", "1");
        builder.AddExtension("ext2", "2");
        builder.Detail = "Failure detail";
        builder.TryBuild(out var instance).ShouldBeTrue();

        await VerifyJson(
            instance.ToProblemDetails(),
            """
            {
                "type": "urn:altinn:error:STD-00000",
                "title": "One or more validation errors occurred.",
                "status": 400,
                "detail": "Failure detail",
                "code": "STD-00000",
                "statusDescription": "Bad Request",
                "validationErrors": [
                    {
                      "code": "SNAP.VLD-00000",
                      "title": "Invalid value",
                      "paths": ["/foo", "/bar"]
                    }
                ],
                "ext1": "1",
                "ext2": "2"
            }
            """);
    }

    private async Task VerifyJson(
        AltinnProblemDetails value,
        [StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        var type = value.GetType();
        using var doc = JsonSerializer.SerializeToDocument(value, type, Options);

        doc.RootElement.ShouldBeStructurallyEquivalentTo(json);

        var deserialized = (AltinnProblemDetails)JsonSerializer.Deserialize(doc, type, Options)!;
        deserialized.ShouldNotBeNull();
        deserialized.ShouldBe(value, AltinnProblemDetailsComparer.Instance);

        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, WriterOptions);
        doc.WriteTo(writer);
        writer.Flush();
        ms.WriteByte((byte)'\n'); // insert final newline

        await Verify(ms, extension: "json");
    }

    private static class AltinnProblemDetailsComparer
    {
        public static IEqualityComparer<AltinnProblemDetails> Instance => Dynamic.Instance;

        private static void HashCommon(ref HashCode hashCode, AltinnProblemDetails problemDetails)
        {
            hashCode.Add(problemDetails.Type, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(problemDetails.Title);
            hashCode.Add(problemDetails.Status);
            hashCode.Add(problemDetails.Detail);
            hashCode.Add(problemDetails.Instance);
            hashCode.Add(problemDetails.ErrorCode);
            hashCode.Add(problemDetails.TraceId);
            hashCode.Add(problemDetails.StatusDescription);
            hashCode.Add(problemDetails.Extensions, Extensions.Instance);
        }

        private static bool EqualsCommon(AltinnProblemDetails x, AltinnProblemDetails y)
        {
            return string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Title, y.Title)
                && x.Status == y.Status
                && string.Equals(x.Detail, y.Detail)
                && string.Equals(x.Instance, y.Instance)
                && x.ErrorCode == y.ErrorCode
                && string.Equals(x.TraceId, y.TraceId)
                && string.Equals(x.StatusDescription, y.StatusDescription)
                && Extensions.Instance.Equals(x.Extensions, y.Extensions);
        }

        private sealed class Multiple
            : IEqualityComparer<AltinnMultipleProblemDetails>
        {
            public static Multiple Instance { get; } = new();

            public bool Equals(AltinnMultipleProblemDetails? x, AltinnMultipleProblemDetails? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (!EqualsCommon(x, y))
                {
                    return false;
                }

                if (ReferenceEquals(x.Problems, y.Problems))
                {
                    return true;
                }

                if (x.Problems is null || y.Problems is null)
                {
                    return false;
                }

                return x.Problems.SequenceEqual(y.Problems);
            }

            public int GetHashCode([DisallowNull] AltinnMultipleProblemDetails obj)
            {
                HashCode hash = default;
                HashCommon(ref hash, obj);

                if (obj.Problems is { } problems)
                {
                    hash.Add(problems.Count);
                    foreach (var problem in problems)
                    {
                        hash.Add(problem, Dynamic.Instance);
                    }
                }

                return hash.ToHashCode();
            }
        }

        private sealed class Validation
            : IEqualityComparer<AltinnValidationProblemDetails>
        {
            public static Validation Instance { get; } = new();

            public bool Equals(AltinnValidationProblemDetails? x, AltinnValidationProblemDetails? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (!EqualsCommon(x, y))
                {
                    return false;
                }

                if (ReferenceEquals(x.Errors, y.Errors))
                {
                    return true;
                }

                if (x.Errors is null || y.Errors is null)
                {
                    return false;
                }

                return x.Errors.SequenceEqual(y.Errors, ValidationError.Instance);
            }

            public int GetHashCode([DisallowNull] AltinnValidationProblemDetails obj)
            {
                HashCode hash = default;
                HashCommon(ref hash, obj);

                if (obj.Errors is { } errors)
                {
                    hash.Add(errors.Count);
                    foreach (var err in errors)
                    {
                        hash.Add(err, ValidationError.Instance);
                    }
                }

                return hash.ToHashCode();
            }
        }

        private sealed class Base
            : IEqualityComparer<AltinnProblemDetails>
        {
            public static Base Instance { get; } = new();

            public bool Equals(AltinnProblemDetails? x, AltinnProblemDetails? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return EqualsCommon(x, y);
            }

            public int GetHashCode([DisallowNull] AltinnProblemDetails obj)
            {
                HashCode hash = default;
                HashCommon(ref hash, obj);
                return hash.ToHashCode();
            }
        }

        private sealed class Dynamic
            : IEqualityComparer<AltinnProblemDetails>
        {
            public static Dynamic Instance { get; } = new();

            public bool Equals(AltinnProblemDetails? x, AltinnProblemDetails? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                var type = x.GetType();
                if (type != y.GetType())
                {
                    return false;
                }

                if (type == typeof(AltinnMultipleProblemDetails))
                {
                    return Multiple.Instance.Equals((AltinnMultipleProblemDetails)x, (AltinnMultipleProblemDetails)y);
                }

                if (type == typeof(AltinnValidationProblemDetails))
                {
                    return Validation.Instance.Equals((AltinnValidationProblemDetails)x, (AltinnValidationProblemDetails)y);
                }

                if (type == typeof(AltinnProblemDetails))
                {
                    return Base.Instance.Equals(x, y);
                }

                throw new InvalidOperationException($"Unknown type: {type}");
            }

            public int GetHashCode([DisallowNull] AltinnProblemDetails obj)
            {
                var type = obj.GetType();

                if (type == typeof(AltinnMultipleProblemDetails))
                {
                    return Multiple.Instance.GetHashCode((AltinnMultipleProblemDetails)obj);
                }

                if (type == typeof(AltinnValidationProblemDetails))
                {
                    return Validation.Instance.GetHashCode((AltinnValidationProblemDetails)obj);
                }

                if (type == typeof(AltinnProblemDetails))
                {
                    return Base.Instance.GetHashCode(obj);
                }

                throw new InvalidOperationException($"Unknown type: {type}");
            }
        }

        private sealed class ValidationError
            : IEqualityComparer<AltinnValidationError>
        {
            public static ValidationError Instance { get; } = new();

            public bool Equals(AltinnValidationError? x, AltinnValidationError? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (!(string.Equals(x.Title, y.Title)
                    && string.Equals(x.Detail, y.Detail)
                    && x.ErrorCode == y.ErrorCode
                    && Extensions.Instance.Equals(x.Extensions, y.Extensions)))
                {
                    return false;
                }

                if (x.Paths.IsDefault != y.Paths.IsDefault)
                {
                    return false;
                }

                if (x.Paths.IsDefault)
                {
                    return true;
                }

                return x.Paths.SequenceEqual(y.Paths);
            }

            public int GetHashCode([DisallowNull] AltinnValidationError obj)
            {
                HashCode hash = default;
                hash.Add(obj.Title);
                hash.Add(obj.Detail);
                hash.Add(obj.ErrorCode);
                hash.Add(obj.Extensions, Extensions.Instance);

                if (!obj.Paths.IsDefault)
                {
                    hash.Add(obj.Paths.Length);
                    foreach (var path in obj.Paths)
                    {
                        hash.Add(path);
                    }
                }

                return hash.ToHashCode();
            }
        }

        private sealed class Extensions
            : IEqualityComparer<IDictionary<string, object?>>
        {
            public static Extensions Instance { get; } = new();

            public bool Equals(IDictionary<string, object?>? x, IDictionary<string, object?>? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (x.Count != y.Count)
                {
                    return false;
                }

                foreach (var kvp in x)
                {
                    if (!y.TryGetValue(kvp.Key, out var yVal))
                    {
                        return false;
                    }

                    var xVal = kvp.Value;
                    if (!ValueEquals(xVal, yVal))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode([DisallowNull] IDictionary<string, object?> obj)
            {
                // we can't produce a proper hash-code here, because the representation is lacking type-information in the JSON case
                // just using the dictionary-size is still valid though.
                return HashCode.Combine(obj.Count);
            }

            private static bool ValueEquals(object? x, object? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                var xType = x.GetType();
                var yType = y.GetType();
                if (xType == yType)
                {
                    return SameTypeEquals(x, y);
                }

                if (x is JsonElement xJson)
                {
                    if (!TryParseAs(xJson, yType, out var xAsY))
                    {
                        return false;
                    }

                    return SameTypeEquals(xAsY, y);
                }

                if (y is JsonElement yJson)
                {
                    if (!TryParseAs(yJson, xType, out var yAsX))
                    {
                        return false;
                    }

                    return SameTypeEquals(x, yAsX);
                }

                return false;
            }

            private static bool SameTypeEquals(object x, object y)
            {
                var type = x.GetType();
                var comparer = typeof(EqualityComparer<>).MakeGenericType(type).GetProperty(nameof(EqualityComparer<>.Default), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
                var method = typeof(IEqualityComparer<>).MakeGenericType(type).GetMethod(nameof(IEqualityComparer<>.Equals), BindingFlags.Public | BindingFlags.Instance)!;
                var result = method.Invoke(comparer, invokeAttr: BindingFlags.DoNotWrapExceptions, binder: null, parameters: [x, y], culture: null)!;
                return (bool)result;
            }

            private static bool TryParseAs(JsonElement json, Type type, [NotNullWhen(true)] out object? result)
            {
                try
                {
                    result = JsonSerializer.Deserialize(json, type, Options);
                    return result is not null;
                }
                catch (JsonException)
                {
                    result = null;
                    return false;
                }
            }
        }
    }
}
