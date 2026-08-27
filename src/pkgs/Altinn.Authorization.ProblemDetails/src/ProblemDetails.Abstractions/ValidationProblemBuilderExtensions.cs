using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extensions for <see cref="ValidationProblemBuilder"/>.
/// </summary>
public static class ValidationProblemBuilderExtensions
{
    /// <param name="builder">The error collection.</param>
    extension(ref ValidationProblemBuilder builder)
    {
        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        public void Add(ValidationErrorDescriptor descriptor)
            => builder.Add(descriptor.Create());

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path)
            => builder.Add(descriptor.Create(path));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path, string? detail)
            => builder.Add(descriptor.Create(path, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
            => builder.Add(descriptor.Create(paths));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, string? detail)
            => builder.Add(descriptor.Create(paths, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
            => builder.Add(descriptor.Create(paths));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, string? detail)
            => builder.Add(descriptor.Create(paths, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        public void Add(ValidationErrorDescriptor descriptor, ProblemExtensionData extensions)
            => builder.Add(descriptor.Create(extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        public void Add(ValidationErrorDescriptor descriptor, ProblemExtensionData extensions, string? detail)
            => builder.Add(descriptor.Create(extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        public void Add(ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
            => builder.Add(descriptor.Create(extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        public void Add(ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions, string? detail)
            => builder.Add(descriptor.Create(extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions)
            => builder.Add(descriptor.Create(path, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions, string? detail)
            => builder.Add(descriptor.Create(path, extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
            => builder.Add(descriptor.Create(path, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="path">The path.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions, string? detail)
            => builder.Add(descriptor.Create(path, extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions)
            => builder.Add(descriptor.Create(paths, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions, string? detail)
            => builder.Add(descriptor.Create(paths, extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
            => builder.Add(descriptor.Create(paths, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
            => builder.Add(descriptor.Create(paths, extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions)
            => builder.Add(descriptor.Create(paths, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions, string? detail)
            => builder.Add(descriptor.Create(paths, extensions, detail));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
            => builder.Add(descriptor.Create(paths, extensions));

        /// <summary>
        /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="detail">The error detail.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        public void Add(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
            => builder.Add(descriptor.Create(paths, extensions, detail));

        /// <summary>
        /// Merges the contents of two <see cref="ValidationProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="arg1">The first <see cref="ValidationProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg2">The second <see cref="ValidationProblemBuilder"/> to merge into this one.</param>
        /// <remarks>
        /// This clears <paramref name="arg1"/> and <paramref name="arg2"/> after merging.
        /// </remarks>
        public void MergeWith(ref ValidationProblemBuilder arg1, ref ValidationProblemBuilder arg2)
        {
            builder.MergeWith(ref arg1);
            builder.MergeWith(ref arg2);
        }

        /// <summary>
        /// Merges the contents of two <see cref="ValidationProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="arg1">The first <see cref="ValidationProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg2">The second <see cref="ValidationProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg3">The third <see cref="ValidationProblemBuilder"/> to merge into this one.</param>
        /// <remarks>
        /// This clears <paramref name="arg1"/>, <paramref name="arg2"/>, and <paramref name="arg3"/> after merging.
        /// </remarks>
        public void MergeWith(ref ValidationProblemBuilder arg1, ref ValidationProblemBuilder arg2, ref ValidationProblemBuilder arg3)
        {
            builder.MergeWith(ref arg1);
            builder.MergeWith(ref arg2);
            builder.MergeWith(ref arg3);
        }

        /// <summary>
        /// Merges the contents of multiple <see cref="ValidationProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="others">The other <see cref="ValidationProblemBuilder"/> instances to merge into this one.</param>
        /// <remarks>
        /// This <strong>may</strong> clear the <paramref name="others"/> after merging.
        /// </remarks>
        public void MergeWith(params Span<ValidationProblemBuilder> others)
        {
            foreach (ref var other in others)
            {
                builder.MergeWith(ref other);
            }
        }
    }
}
