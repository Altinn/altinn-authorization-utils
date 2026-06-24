using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altinn.Authorization.ProblemDetails.Mvc;

/// <summary>
/// Context for building a <see cref="ValidationErrorInstance"/> from a <see cref="Microsoft.AspNetCore.Mvc.ModelBinding.ModelError"/>.
/// </summary>
public sealed class ValidationErrorContext
{
    private ImmutableArray<string>.Builder _paths;
    private ProblemExtensionDataBuilder _extensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorContext"/> class.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    /// <param name="modelState">The model state dictionary.</param>
    /// <param name="modelStateKey">The model state key.</param>
    /// <param name="modelStateEntry">The model state entry.</param>
    /// <param name="modelError">The model error.</param>
    public ValidationErrorContext(
        ActionContext actionContext,
        ModelStateDictionary modelState,
        string modelStateKey,
        ModelStateEntry modelStateEntry,
        ModelError modelError)
    {
        ActionContext = actionContext;
        ModelState = modelState;
        ModelStateKey = modelStateKey;
        ModelStateEntry = modelStateEntry;
        ModelError = modelError;
        _paths = ImmutableArray.CreateBuilder<string>(1);
        _extensions = default;
    }

    /// <summary>
    /// Gets the <see cref="ActionContext"/> for this validation error.
    /// </summary>
    public ActionContext ActionContext { get; }

    /// <summary>
    /// Gets the model state dictionary that contains this validation error.
    /// </summary>
    public ModelStateDictionary ModelState { get; }

    /// <summary>
    /// Gets the model state key for this validation error.
    /// </summary>
    public string ModelStateKey { get; }

    /// <summary>
    /// Gets the model state entry that contains this validation error.
    /// </summary>
    public ModelStateEntry ModelStateEntry { get; }

    /// <summary>
    /// Gets the model error being converted to a validation error.
    /// </summary>
    public ModelError ModelError { get; }

    /// <summary>
    /// Gets the action parameter descriptor associated with this validation error, if any.
    /// </summary>
    public ParameterDescriptor? ParameterDescriptor
    {
        get => field;
        internal set
        {
            if (field is not null)
            {
                ThrowHelper.ThrowInvalidOperationException("ParameterDescriptor has already been set.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets JSON type metadata associated with this validation error, if any.
    /// </summary>
    public JsonTypeInfo? JsonTypeInfo
    {
        get => field;
        internal set
        {
            if (field is not null)
            {
                ThrowHelper.ThrowInvalidOperationException("JsonTypeInfo has already been set.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets JSON property metadata associated with this validation error, if any.
    /// </summary>
    public JsonPropertyInfo? JsonPropertyInfo
    {
        get => field;
        internal set
        {
            if (field is not null)
            {
                ThrowHelper.ThrowInvalidOperationException("JsonPropertyInfo has already been set.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets member metadata associated with this validation error, if any.
    /// </summary>
    public MemberInfo? MemberInfo
    {
        get => field;
        internal set
        {
            if (field is not null)
            {
                ThrowHelper.ThrowInvalidOperationException("MemberInfo has already been set.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the property type associated with this validation error, if any.
    /// </summary>
    public Type? PropertyType
    {
        get => field;
        internal set
        {
            if (field is not null)
            {
                ThrowHelper.ThrowInvalidOperationException("PropertyType has already been set.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the descriptor used to create the validation error.
    /// </summary>
    [DisallowNull]
    public ValidationErrorDescriptor? Descriptor
    {
        get => field;
        set
        {
            Guard.IsNotNull(value);

            field = value;
        }
    }

    /// <summary>
    /// Gets the JSON pointer paths associated with this validation error.
    /// </summary>
    public IList<string> Paths
        => _paths;

    /// <summary>
    /// Gets or sets additional validation error detail text.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Sets an extension value on the validation error.
    /// </summary>
    /// <param name="key">The extension key.</param>
    /// <param name="value">The extension value.</param>
    public void SetExtension(string key, string value)
    {
        _extensions[key] = value;
    }

    internal ValidationErrorInstance ToValidationErrorInstance()
    {
        var descriptor = Descriptor ?? StdValidationErrors.InvalidValue;

        return descriptor.Create(paths: _paths.ToImmutable(), detail: Detail, extensions: _extensions.ToImmutable());
    }
}
