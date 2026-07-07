using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Altinn.Authorization.ProblemDetails.PathUtils;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ProblemDetails.Mvc;

internal sealed class AltinnValidationProblemDetailsFactory
{
    private readonly ImmutableArray<IModelStateErrorValidationErrorProvider> _modelStateErrorValidationErrorProviders;
    private readonly Lazy<Func<JsonSerializerOptions>> _jsonSerializerOptions;

    public AltinnValidationProblemDetailsFactory(
        IEnumerable<IModelStateErrorValidationErrorProvider> modelStateErrorValidationErrorProviders,
        IServiceProvider serviceProvider)
    {
        Guard.IsNotNull(modelStateErrorValidationErrorProviders);

        _modelStateErrorValidationErrorProviders = modelStateErrorValidationErrorProviders.ToImmutableArray();

        _jsonSerializerOptions = new(() =>
        {
            if (serviceProvider.GetService<IOptionsMonitor<Microsoft.AspNetCore.Mvc.JsonOptions>>() is { } mvcJsonOptions)
            {
                return () => mvcJsonOptions.CurrentValue.JsonSerializerOptions;
            }

            if (serviceProvider.GetService<IOptionsMonitor<Microsoft.AspNetCore.Http.Json.JsonOptions>>() is { } httpJsonOptions)
            {
                return () => httpJsonOptions.CurrentValue.SerializerOptions;
            }

            return () => JsonSerializerOptions.Web;
        });
    }

    public AltinnValidationProblemDetails CreateValidationProblemDetails(ActionContext context)
    {
        Guard.IsNotNull(context);

        var modelState = context.ModelState;
        if (modelState is null)
        {
            ThrowHelper.ThrowArgumentException(nameof(context), "context.ModelState cannot be null.");
        }

        ValidationProblemBuilder builder = default;
        foreach (var (key, state) in modelState)
        {
            var errors = state.Errors;
            if (errors is { Count: > 0 })
            {
                foreach (var error in errors)
                {
                    builder.Add(CreateValidationError(key, error, state, modelState, context));
                }
            }
        }

        if (builder.Count == 0)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Add(StdValidationErrors.CatchAll, path: "/");
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!builder.TryBuild(out var result))
        {
            Unreachable();
        }

        return new AltinnValidationProblemDetails(result);
    }

    public ValidationErrorInstance CreateValidationError(string key, ModelError error, ModelStateEntry state, ModelStateDictionary modelState, ActionContext context)
    {
        var errorContext = new ValidationErrorContext(context, modelState, key, state, error);
        EnrichContext(errorContext);

        foreach (var provider in _modelStateErrorValidationErrorProviders)
        {
            provider.BuildValidationError(errorContext);
        }

        if (errorContext.Descriptor is null)
        {
            errorContext.Descriptor = FallbackGetDescriptorFromModelStateError(error);

            // If no provider has set a descriptor, we try to infer one from the error.
            errorContext.Detail ??= error.ErrorMessage;
        }

        return errorContext.ToValidationErrorInstance();
    }

    private void EnrichContext(ValidationErrorContext context)
    {
        if (TryGetValidationErrorFromException(context.ModelError.Exception, out var validationErrorFromException))
        {
            context.Descriptor = validationErrorFromException.Descriptor;
            context.Detail = validationErrorFromException.Detail;

            foreach (var path in validationErrorFromException.Paths)
            {
                context.Paths.Add(path);
            }

            foreach (var (key, value) in validationErrorFromException.Extensions)
            {
                context.SetExtension(key, value);
            }
        }

        if (context.Paths.Count == 0
            && context.ModelError.Exception is JsonException { Path: { Length: > 0 } jsonPath, Message: var jsonMessage })
        {
            context.Descriptor ??= StdValidationErrors.JsonError;

            if (!string.IsNullOrEmpty(jsonMessage))
            {
                context.Detail ??= jsonMessage;
            }

            context.Paths.Add(JsonPointerExtensions.CreateFromSystemTextJsonPath(jsonPath));
            return;
        }

        if (TryFindOwningParameter(context.ActionContext, context.ModelStateKey, out var parameterDescriptor, out var remainderPath))
        {
            context.ParameterDescriptor = parameterDescriptor;

            EnrichContextFromParameter(context, parameterDescriptor, remainderPath);
            EnrichContextFromModelMetadata(context);
            return;
        }

        if (context.ModelMetadata is { } metadata)
        {
            context.DisplayName ??= metadata.GetDisplayName();
        }
        else if (context.MemberInfo is { } memberInfo)
        {
            context.DisplayName ??= GetDisplayName(memberInfo);
        }
        else if (context.ParameterDescriptor is ControllerParameterDescriptor { ParameterInfo: { } parameterInfo })
        {
            context.DisplayName ??= GetDisplayName(parameterInfo);
        }

        if (context.Paths.Count > 0)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.Append('/');
        BuildJsonPointerFromPathStringOnly(context.ModelStateKey.AsSpan(), builder);
        if (builder.Length > 1)
        {
            builder.Length--; // remove trailing slash
        }

        context.Paths.Add(builder.ToString());
    }

    private static void EnrichContextFromModelMetadata(ValidationErrorContext context)
    {
        if (context.ActionContext.HttpContext.RequestServices.GetService<IModelMetadataProvider>() is not { } metadataProvider)
        {
            return;
        }

        context.ModelMetadataProvider = metadataProvider;
        context.ModelMetadata = GetModelMetadata(metadataProvider, context);
        context.DisplayName = context.ModelMetadata?.GetDisplayName();
    }

    private static ModelMetadata? GetModelMetadata(
        IModelMetadataProvider metadataProvider,
        ValidationErrorContext context)
    {
        if (metadataProvider is ModelMetadataProvider provider)
        {
            if (context.MemberInfo is PropertyInfo propertyInfo)
            {
                return provider.GetMetadataForProperty(propertyInfo, context.PropertyType ?? propertyInfo.PropertyType);
            }

            if (context.ParameterDescriptor is ControllerParameterDescriptor { ParameterInfo: { } parameterInfo }
                && context.MemberInfo is null)
            {
                return provider.GetMetadataForParameter(parameterInfo);
            }
        }

        if (context.MemberInfo is PropertyInfo { DeclaringType: { } declaringType, Name: var propertyName })
        {
            return metadataProvider
                .GetMetadataForProperties(declaringType)
                .FirstOrDefault(metadata => string.Equals(metadata.PropertyName, propertyName, StringComparison.Ordinal));
        }

        return context.PropertyType is { } propertyType
            ? metadataProvider.GetMetadataForType(propertyType)
            : null;
    }

    private void EnrichContextFromParameter(
        ValidationErrorContext context,
        ParameterDescriptor parameterDescriptor,
        ReadOnlySpan<char> path)
    {
        var builder = new StringBuilder();

        var source = parameterDescriptor.BindingInfo?.BindingSource;
        if (source is not null)
        {
            var name = parameterDescriptor.BindingInfo?.BinderModelName ?? parameterDescriptor.Name;
            if (source.CanAcceptDataFrom(BindingSource.Query))
            {
                builder.Append("/$QUERY/").AppendEscapeJsonPointer(name).Append('/');
            }
            else if (source.CanAcceptDataFrom(BindingSource.Header))
            {
                builder.Append("/$HEADER/").AppendEscapeJsonPointer(name).Append('/');
            }
            else if (source.CanAcceptDataFrom(BindingSource.Path))
            {
                builder.Append("/$PATH/").AppendEscapeJsonPointer(name).Append('/');
            }
        }

        if (builder.Length == 0)
        {
            builder.Append('/');
        }

        if (!path.IsEmpty)
        {
            BuildJsonPointerFromTypeAndPath(parameterDescriptor.ParameterType, path, builder, context);
        }

        if (builder.Length > 1)
        {
            builder.Length--; // remove trailing slash
        }

        if (context.Paths.Count == 0)
        {
            context.Paths.Add(builder.ToString());
        }
    }

    internal void BuildJsonPointerFromTypeAndPath(
        Type type,
        ReadOnlySpan<char> path,
        StringBuilder builder,
        ValidationErrorContext context)
    {
        var jsonOptions = _jsonSerializerOptions.Value();
        if (!jsonOptions.TryGetTypeInfo(type, out var typeInfo))
        {
            BuildJsonPointerFromPathStringOnly(path, builder);
            return;
        }


        JsonPropertyInfo? propertyInfo = null;
        MemberInfo? memberInfo = null;

        var iterator = PathSegmentIterator.Create(path);
        while (iterator.MoveNext())
        {
            var segment = iterator.Current;
            if (segment.Type == PathSegmentType.Property)
            {
                if (!TryFindJsonProperty(typeInfo, segment.Value, out propertyInfo, out memberInfo))
                {
                    BuildJsonPointerFromPathStringOnly(segment.Remainder, builder);
                    return;
                }

                builder.AppendEscapeJsonPointer(propertyInfo.Name).Append('/');

                type = propertyInfo.PropertyType;
                if (!jsonOptions.TryGetTypeInfo(type, out typeInfo))
                {
                    BuildJsonPointerFromPathStringOnly(iterator.Remainder, builder);
                    return;
                }
            }
            else if (segment.Type == PathSegmentType.Indexer)
            {
                propertyInfo = null;
                memberInfo = null; // TODO: get the indexer property

                builder.AppendEscapeJsonPointer(segment.Value).Append('/');
                if (typeInfo.Kind is not JsonTypeInfoKind.Enumerable
                    || typeInfo.ElementType is null
                    || !jsonOptions.TryGetTypeInfo(typeInfo.ElementType, out typeInfo))
                {
                    BuildJsonPointerFromPathStringOnly(iterator.Remainder, builder);
                    return;
                }

                type = typeInfo.ElementType!;
            }
            else
            {
                BuildJsonPointerFromPathStringOnly(segment.Remainder, builder);
                return;
            }
        }

        context.JsonTypeInfo = typeInfo;
        context.JsonPropertyInfo = propertyInfo;
        context.MemberInfo = memberInfo;
        context.PropertyType = type;
    }

    internal static bool TryFindJsonProperty(
        JsonTypeInfo typeInfo,
        ReadOnlySpan<char> propertyName,
        [NotNullWhen(true)] out JsonPropertyInfo? propertyInfo,
        [NotNullWhen(true)] out MemberInfo? memberInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (property.AttributeProvider is PropertyInfo { Name: var propertyInfoName } propertyInfoRaw
                && propertyInfoName.SequenceEqual(propertyName))
            {
                propertyInfo = property;
                memberInfo = propertyInfoRaw;
                return true;
            }

            if (property.AttributeProvider is FieldInfo { Name: var fieldInfoName } fieldInfoRaw
                && fieldInfoName.SequenceEqual(propertyName))
            {
                propertyInfo = property;
                memberInfo = fieldInfoRaw;
                return true;
            }
        }

        propertyInfo = null;
        memberInfo = null;
        return false;
    }

    internal static void BuildJsonPointerFromPathStringOnly(ReadOnlySpan<char> path, StringBuilder builder)
    {
        foreach (var segment in PathSegmentIterator.Create(path))
        {
            builder.AppendEscapeJsonPointer(segment.Value).Append('/');
        }
    }

    internal static ValidationErrorDescriptor FallbackGetDescriptorFromModelStateError(ModelError error)
    {
        // TODO:
        if (error.Exception is not null)
        {
            return StdValidationErrors.InvalidValue;
        }

        return StdValidationErrors.InvalidValue;
    }

    private static bool TryFindOwningParameter(
        ActionContext context,
        string modelStateKey,
        [NotNullWhen(true)] out ParameterDescriptor? parameterDescriptor,
        out ReadOnlySpan<char> remainderPath)
    {
        var initialSegment = PathSegmentIterator.GetInitialSegment(modelStateKey);

        // first, we check all the parameters if we find a named one that matches
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            // var bindingSource = parameter.BindingInfo?.BindingSource;
            var binderName = parameter.BindingInfo?.BinderModelName;
            var parameterName = binderName ?? parameter.Name;

            if (parameterName.SequenceEqual(initialSegment))
            {
                parameterDescriptor = parameter;
                remainderPath
                    = modelStateKey.Length > initialSegment.Length
                        ? modelStateKey.AsSpan(initialSegment.Length)
                        : [];

                return true;
            }
        }

        // if we didn't find a named parameter, see if we have a body parameter
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            var bindingSource = parameter.BindingInfo?.BindingSource;
            if (bindingSource is not null && bindingSource.CanAcceptDataFrom(BindingSource.Body))
            {
                parameterDescriptor = parameter;
                remainderPath = modelStateKey;
                return true;
            }
        }

        parameterDescriptor = null;
        remainderPath = [];
        return false;
    }

    private static string GetDisplayName(MemberInfo member)
        => member.GetCustomAttribute<DisplayAttribute>()?.GetName()
            ?? member.Name;

    private static string? GetDisplayName(ParameterInfo parameter)
        => parameter.GetCustomAttribute<DisplayAttribute>()?.GetName()
            ?? parameter.Name;

    private static bool TryGetValidationErrorFromException(Exception? exception, [NotNullWhen(true)] out ValidationErrorInstance? validationError)
    {
        const uint MAX_DEPTH = 50;
        uint depth = 0;

        while (exception is not null && depth++ < MAX_DEPTH)
        {
            if (exception is IHasValidationError { ValidationError: { } error })
            {
                validationError = error;
                return true;
            }

            exception = exception.InnerException;
        }

        validationError = null;
        return false;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Unreachable()
    {
        throw new UnreachableException();
    }
}
