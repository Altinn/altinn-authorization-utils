using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ProblemDetails.Validation;

public interface IValidatableModel
{
    void Validate(ref ValidationContext context);
}

public readonly ref struct ValidationContext
{
    [ThreadStatic]
    private static StringBuilder? _sb;

    private readonly Path _path;
    private readonly ref ValidationErrorBuilder _builder;

    internal ValidationContext(Path path, ref ValidationErrorBuilder builder)
    {
        _path = path;
        _builder = ref builder;
    }

    public void Add(ValidationErrorInstance error)
        => _builder.Add(error);

    internal string FullPath(object relPath)
        => _path.FullPath(relPath);

    internal ValidationContext Child(object relPath)
    {
        var childPath = _path.Child(relPath);
        return new(childPath, ref _builder);
    }

    internal readonly ref struct Path
    {
        private readonly Span<object> _buffer;
        private readonly byte _length;

        internal Path(Span<object> buffer, byte length)
        {
            _buffer = buffer;
            _length = length;
        }

        internal Path Child(object relPath)
        {
            if (_length == _buffer.Length)
            {
                ThrowHelper.ThrowInvalidOperationException($"Validation depth exceeded {_buffer.Length}.");
            }

            _buffer[_length] = relPath;
            return new(_buffer, (byte)(_length + 1));
        }

        internal string FullPath(object relPath)
        {
            _sb ??= new(128);
            _sb.Clear();

            for (var i = 0; i < _length; i++)
            {
                _sb.Append('/');
                _sb.Append(_buffer[i]);
            }

            _sb.Append('/');
            _sb.Append(relPath);

            return _sb.ToString();
        }
    }
}

public static class ValidationContextExtensions
{
    public delegate void ValidationDelegate<T>(ref ValidationContext context, T model);

    public static void Check(ref this ValidationContext context, bool condition, ValidationErrorDescriptor descriptor, object relPath)
    {
        if (!condition)
        {
            var path = context.FullPath(relPath);
            var error = descriptor.Create(path);
            context.Add(error);
        }
    }

    public static void Validate<T>(ref this ValidationContext context, T model, object relPath, ValidationDelegate<T> validate)
    {
        var childContext = context.Child(relPath);
        validate(ref childContext, model);
    }

    public static void Validate<T>(ref this ValidationContext context, T? model, object relPath)
        where T : IValidatableModel
    {
        if (model is null)
        {
            var path = context.FullPath(relPath);
            var error = StdValidationErrors.Required.Create(path);
            context.Add(error);
            return;
        }

        var childContext = context.Child(relPath);
        model.Validate(ref childContext);
    }

    public static void ValidateItems<T>(ref this ValidationContext context, IReadOnlyList<T> models, object relPath, ValidationDelegate<T> validate)
    {
        var listContext = context.Child(relPath);
        for (int i = 0, l = models.Count; i < l; i++)
        {
            var model = models[i];
            var itemContext = listContext.Child(i);
            validate(ref itemContext, model);
        }
    }
}

public static class Validator
{
    public static Result<T> Validate<T>(T model, string? rootPath = null)
        where T : IValidatableModel
    {
        ValidationErrorBuilder builder = default;
        var pathScratch = ArrayPool<object>.Shared.Rent(16);
        var pathStruct = new ValidationContext.Path(pathScratch, 0);

        if (!string.IsNullOrEmpty(rootPath))
        {
            Debug.Assert(!rootPath.EndsWith('/'));

            pathScratch[0] = rootPath;
            pathStruct = new ValidationContext.Path(pathScratch, 1);
        }

        try
        {
            var context = new ValidationContext(pathStruct, ref builder);
            model.Validate(ref context);
        }
        finally
        {
            pathScratch.AsSpan().Clear();
            ArrayPool<object>.Shared.Return(pathScratch);
        }

        if (builder.TryBuild(out var error))
        {
            return error;
        }

        return model;
    }
}
