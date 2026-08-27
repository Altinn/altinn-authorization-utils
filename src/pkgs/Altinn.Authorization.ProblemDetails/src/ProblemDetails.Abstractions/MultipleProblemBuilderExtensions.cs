namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extensions for <see cref="MultipleProblemBuilder"/>.
/// </summary>
public static class MultipleProblemBuilderExtensions
{
    /// <param name="builder">The error collection.</param>
    extension(ref MultipleProblemBuilder builder)
    {
        /// <summary>
        /// Adds a problem with the specified <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
        /// <param name="detail">The error detail.</param>
        /// <param name="source">The source problem instance.</param>
        public void Add(
            ProblemDescriptor descriptor,
            string? detail = null,
            ProblemInstance? source = null)
            => builder.Add(descriptor.Create(detail, source));

        /// <summary>
        /// Adds a problem with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="source">The source problem instance.</param>
        public void Add(
            ProblemDescriptor descriptor,
            ProblemExtensionData extensions,
            ProblemInstance? source = null)
            => builder.Add(descriptor.Create(extensions, source: source));

        /// <summary>
        /// Adds a problem with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
        /// <param name="detail">The error detail.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="source">The source problem instance.</param>
        public void Add(
            ProblemDescriptor descriptor,
            string? detail,
            ProblemExtensionData extensions,
            ProblemInstance? source = null)
            => builder.Add(descriptor.Create(detail, extensions, source: source));

        /// <summary>
        /// Adds a problem with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="source">The source problem instance.</param>
        public void Add(
            ProblemDescriptor descriptor,
            IReadOnlyDictionary<string, string> extensions,
            ProblemInstance? source = null)
            => builder.Add(descriptor.Create(extensions, source: source));

        /// <summary>
        /// Adds a problem with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
        /// <param name="detail">The error detail.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="source">The source problem instance.</param>
        public void Add(
            ProblemDescriptor descriptor,
            string? detail,
            IReadOnlyDictionary<string, string> extensions,
            ProblemInstance? source = null)
            => builder.Add(descriptor.Create(detail, extensions, source: source));

        /// <summary>
        /// Merges the contents of two <see cref="MultipleProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="arg1">The first <see cref="MultipleProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg2">The second <see cref="MultipleProblemBuilder"/> to merge into this one.</param>
        /// <remarks>
        /// This clears <paramref name="arg1"/> and <paramref name="arg2"/> after merging.
        /// </remarks>
        public void MergeWith(ref MultipleProblemBuilder arg1, ref MultipleProblemBuilder arg2)
        {
            builder.MergeWith(ref arg1);
            builder.MergeWith(ref arg2);
        }

        /// <summary>
        /// Merges the contents of two <see cref="MultipleProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="arg1">The first <see cref="MultipleProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg2">The second <see cref="MultipleProblemBuilder"/> to merge into this one.</param>
        /// <param name="arg3">The third <see cref="MultipleProblemBuilder"/> to merge into this one.</param>
        /// <remarks>
        /// This clears <paramref name="arg1"/>, <paramref name="arg2"/>, and <paramref name="arg3"/> after merging.
        /// </remarks>
        public void MergeWith(ref MultipleProblemBuilder arg1, ref MultipleProblemBuilder arg2, ref MultipleProblemBuilder arg3)
        {
            builder.MergeWith(ref arg1);
            builder.MergeWith(ref arg2);
            builder.MergeWith(ref arg3);
        }

        /// <summary>
        /// Merges the contents of multiple <see cref="MultipleProblemBuilder"/> instances into this one.
        /// </summary>
        /// <param name="others">The other <see cref="MultipleProblemBuilder"/> instances to merge into this one.</param>
        /// <remarks>
        /// This <strong>may</strong> clear the <paramref name="others"/> after merging.
        /// </remarks>
        public void MergeWith(params Span<MultipleProblemBuilder> others)
        {
            foreach (ref var other in others)
            {
                builder.MergeWith(ref other);
            }
        }
    }
}
