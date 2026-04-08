using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// Represents the context for validating an input model. Provides methods for adding validation errors and validating child models while maintaining the correct error paths.
/// </summary>
public ref struct ValidationContext
{
    internal static bool TryValidate<TIn, TOut>(
        ref ValidationProblemBuilder builder,
        string path,
        TIn input,
        Validator<TIn, TOut> validator,
        [NotNullWhen(true)] out TOut? validated)
        where TOut : notnull
    {
        Guard.IsNotNull(validator);
        Guard.IsValidRootPath(path);

        if (path.Length == 1)
        {
            path = string.Empty;
        }

        Root root = default;
        root.Initialize(path);

        var context = new ValidationContext(ref root, OwnerHandle.Root, parent: default, ref builder);
        try
        {
            return validator(ref context, input, out validated);
        }
        finally
        {
            context.Dispose();
            Debug.Assert(root.IsDisposed);
        }
    }

    private readonly ref Root _root;
    private readonly OwnerHandle _handle;
    private readonly ParentHandle _parent;
    private ref ValidationProblemBuilder _builder;
    private ValidationState _state;

    private ValidationContext(
        ref Root root,
        OwnerHandle handle,
        ParentHandle parent,
        ref ValidationProblemBuilder builder)
    {
        _root = ref root;
        _handle = handle;
        _parent = parent;
        _builder = ref builder;
        _state = ValidationState.None;
    }

    /// <summary>
    /// Gets a value indicating whether any validation errors have been added to this context or any of its child contexts.
    /// </summary>
    public bool HasErrors
        => _state.HasFlag(ValidationState.HasErrors);

    /// <summary>
    /// Validates a child model using the specified validator.
    /// </summary>
    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the validated model.</typeparam>
    /// <param name="path">The path to the child model.</param>
    /// <param name="input">The input model to validate.</param>
    /// <param name="validator">The validator to use.</param>
    /// <param name="validated">The validated model.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>True if the child model is valid; otherwise, false.</returns>
    public bool TryValidateChild<TIn, TOut>(
        string path,
        TIn input,
        Validator<TIn, TOut> validator,
        [NotNullWhen(true)] out TOut? validated)
        where TOut : notnull
    {
        Debug.Assert(!_state.HasFlag(ValidationState.IsDisposed));
        var parentHandle = _root.Acquire(_handle, path);

        var context = new ValidationContext(ref _root, parentHandle.Owner, parentHandle, ref _builder);
        try
        {
            var result = validator(ref context, input, out validated);
            if (!result)
            {
                _state |= ValidationState.HasErrors;
            }

            return result;
        }
        finally
        {
            context.Dispose();
        }
    }

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor. The error's path will be based on the current context's path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    public void AddProblem(ValidationErrorDescriptor descriptor)
        => AddProblem(descriptor.Create(_root.Path()));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor. The error's path will be based on the current context's path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    public void AddProblem(ValidationErrorDescriptor descriptor, string? detail)
        => AddProblem(descriptor.Create(_root.Path(), detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor. The error's path will be based on the current context's path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="extensions">The extensions for the validation error.</param>
    public void AddProblem(ValidationErrorDescriptor descriptor, ProblemExtensionData extensions)
        => AddProblem(descriptor.Create(_root.Path(), extensions));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor. The error's path will be based on the current context's path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="extensions">The extensions for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    public void AddProblem(ValidationErrorDescriptor descriptor, ProblemExtensionData extensions, string? detail)
        => AddProblem(descriptor.Create(_root.Path(), extensions, detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, string path)
        => AddProblem(descriptor.Create(_root.Path(path)));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    [OverloadResolutionPriority(1)]
    public void AddChildProblem(ValidationErrorDescriptor descriptor, scoped ReadOnlySpan<string> path)
        => AddProblem(descriptor.Create(_root.Path(path)));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, ImmutableArray<string> path)
        => AddProblem(descriptor.Create(_root.Path(path)));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, IEnumerable<string> path)
        => AddProblem(descriptor.Create(_root.Path(path)));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, string path, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    [OverloadResolutionPriority(1)]
    public void AddChildProblem(ValidationErrorDescriptor descriptor, scoped ReadOnlySpan<string> path, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, ImmutableArray<string> path, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, IEnumerable<string> path, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions)
        => AddProblem(descriptor.Create(_root.Path(path), extensions));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    [OverloadResolutionPriority(1)]
    public void AddChildProblem(ValidationErrorDescriptor descriptor, scoped ReadOnlySpan<string> path, ProblemExtensionData extensions)
        => AddProblem(descriptor.Create(_root.Path(path), extensions));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, ImmutableArray<string> path, ProblemExtensionData extensions)
        => AddProblem(descriptor.Create(_root.Path(path), extensions));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, IEnumerable<string> path, ProblemExtensionData extensions)
        => AddProblem(descriptor.Create(_root.Path(path), extensions));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), extensions, detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    [OverloadResolutionPriority(1)]
    public void AddChildProblem(ValidationErrorDescriptor descriptor, scoped ReadOnlySpan<string> path, ProblemExtensionData extensions, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), extensions, detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, ImmutableArray<string> path, ProblemExtensionData extensions, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), extensions, detail));

    /// <summary>
    /// Adds a validation error to this context with the specified descriptor and child path. The error's path will be based on the current context's path combined with the specified child path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the validation error.</param>
    /// <param name="path">The child path for the validation error.</param>
    /// <param name="extensions">The extension data for the validation error.</param>
    /// <param name="detail">The detail message for the validation error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddChildProblem(ValidationErrorDescriptor descriptor, IEnumerable<string> path, ProblemExtensionData extensions, string? detail)
        => AddProblem(descriptor.Create(_root.Path(path), extensions, detail));

    private void AddProblem(ValidationErrorInstance instance)
    {
        _state |= ValidationState.HasErrors;
        _builder.Add(instance);
    }

    internal void Dispose()
    {
        if (!_state.HasFlag(ValidationState.IsDisposed))
        {
            _state |= ValidationState.IsDisposed;
            if (_handle.IsRoot)
            {
                // root context
                Debug.Assert(_parent.PathLength == 0);
                _root.Dispose();
            }
            else
            {
                _root.Return(_handle, _parent);
            }
        }
    }

    [Flags]
    private enum ValidationState
        : byte
    {
        None = 0,

        HasErrors = 1 << 0,

        IsDisposed = 1 << 1,
    }

    private readonly record struct OwnerHandle(ushort ContextId)
    {
        public static OwnerHandle Root => new OwnerHandle(0);

        public bool IsRoot => ContextId == 0;
    }

    private readonly record struct ParentHandle(OwnerHandle Parent, OwnerHandle Owner, int PathLength);

    private struct Root
    {
        [ThreadStatic]
        private static StringBuilder? _perThreadPathBuilder;


        private StringBuilder _path;
        private OwnerHandle _owner;
        private ushort _nextOwner;

        internal bool IsDisposed => _path is null;

        internal string Path()
            => _path.Length == 0 ? "/" : _path.ToString();

        internal string Path(string childPath)
        {
            Guard.IsValidChildPath(childPath);

            return $"{_path}{childPath}";
        }

        internal ImmutableArray<string> Path(scoped ReadOnlySpan<string> childPaths)
        {
            Guard.IsNotEmpty(childPaths);

            var builder = ImmutableArray.CreateBuilder<string>(childPaths.Length);
            foreach (ref readonly string childPath in childPaths)
            {
                Guard.IsValidChildPath(childPath);

                builder.Add($"{_path}{childPath}");
            }

            return builder.MoveToImmutable();
        }

        internal ImmutableArray<string> Path(ImmutableArray<string> childPaths)
        {
            ValidationGuardExtensions.IsNotEmpty(childPaths);

            var builder = ImmutableArray.CreateBuilder<string>(childPaths.Length);
            foreach (string childPath in childPaths)
            {
                Guard.IsValidChildPath(childPath);

                builder.Add($"{_path}{childPath}");
            }

            return builder.MoveToImmutable();
        }

        internal ImmutableArray<string> Path(IEnumerable<string> childPaths)
        {
            Guard.IsNotNull(childPaths);

            var builder = ImmutableArray.CreateBuilder<string>();
            foreach (string childPath in childPaths)
            {
                Guard.IsValidChildPath(childPath);

                builder.Add($"{_path}{childPath}");
            }

            return builder.DrainToImmutable();
        }

        internal void Initialize(string path)
        {
            _owner = OwnerHandle.Root;
            _path = _perThreadPathBuilder ?? new StringBuilder();
            _perThreadPathBuilder = null;
            _nextOwner = 1;

            Debug.Assert(_path.Length == 0);
            _path.Append(path);
        }

        internal ParentHandle Acquire(OwnerHandle owner, string path)
        {
            Guard.IsValidChildPath(path);

            Debug.Assert(_owner == owner);
            OwnerHandle newOwner = new OwnerHandle(_nextOwner++);
            _owner = newOwner;

            int oldPathLength = _path.Length;
            if (!string.IsNullOrEmpty(path))
            {
                _path.Append(path);
            }

            return new ParentHandle(owner, newOwner, oldPathLength);
        }

        internal void Return(OwnerHandle owner, ParentHandle parent)
        {
            Debug.Assert(owner == _owner);
            _owner = parent.Parent;
            _path.Length = parent.PathLength;
        }

        public void Dispose()
        {
            _path.Clear();
            _perThreadPathBuilder = _path;
            _path = null!;
        }
    }
}

/// <summary>
/// A validator for constructing and validating a model of type <typeparamref name="TOut"/> from an input of type <typeparamref name="TIn"/>.
/// </summary>
/// <typeparam name="TIn">The type of the input model.</typeparam>
/// <typeparam name="TOut">The type of the output model.</typeparam>
/// <param name="context">The validation context.</param>
/// <param name="input">The input model to validate.</param>
/// <param name="validated">The validated output model.</param>
/// <returns>True if the validation is successful; otherwise, false.</returns>
public delegate bool Validator<in TIn, TOut>(
    ref ValidationContext context,
    TIn input,
    [NotNullWhen(true)] out TOut? validated)
#if NET9_0_OR_GREATER
    where TIn : allows ref struct
    where TOut : notnull, allows ref struct
#else
        where TOut : notnull
#endif
    ;
