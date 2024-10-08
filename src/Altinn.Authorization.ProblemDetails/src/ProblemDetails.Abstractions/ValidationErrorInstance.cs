﻿using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An instance of a <see cref="ValidationErrorDescriptor"/>, with optional paths and extensions.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Detail,nq}")]
public sealed record class ValidationErrorInstance
{
    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor) 
        => new ValidationErrorInstance(descriptor, paths: [], extensions: []);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, string path)
        => new ValidationErrorInstance(descriptor, paths: [path], extensions: []);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
        => new ValidationErrorInstance(descriptor, paths: paths, extensions: []);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => new ValidationErrorInstance(descriptor, paths: [.. paths], extensions: []);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, ProblemExtensionData extensions)
        => new ValidationErrorInstance(descriptor, paths: [], extensions: extensions);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => new ValidationErrorInstance(descriptor, paths: [], extensions: [.. extensions]);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions)
        => new ValidationErrorInstance(descriptor, paths: [path], extensions: extensions);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => new ValidationErrorInstance(descriptor, paths: [path], extensions: [.. extensions]);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions)
        => new ValidationErrorInstance(descriptor, paths: paths, extensions: extensions);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => new ValidationErrorInstance(descriptor, paths: paths, extensions: [.. extensions]);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions)
        => new ValidationErrorInstance(descriptor, paths: [.. paths], extensions: extensions);

    /// <summary>
    /// Creates a new <see cref="ValidationErrorInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static ValidationErrorInstance Create(ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => new ValidationErrorInstance(descriptor, paths: [.. paths], extensions: [.. extensions]);

    private readonly ValidationErrorDescriptor _descriptor;
    private readonly ImmutableArray<string> _paths;
    private readonly ProblemExtensionData _extensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorInstance"/> class.
    /// </summary>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    internal ValidationErrorInstance(
        ValidationErrorDescriptor descriptor, 
        ImmutableArray<string> paths, 
        ProblemExtensionData extensions)
    {
        _descriptor = descriptor;
        _paths = paths;
        _extensions = extensions;
    }

    /// <inheritdoc cref="ValidationErrorDescriptor.ErrorCode"/>
    public ErrorCode ErrorCode => _descriptor.ErrorCode;

    /// <inheritdoc cref="ValidationErrorDescriptor.Detail"/>
    public string Detail => _descriptor.Detail;

    /// <summary>
    /// Gets the error paths.
    /// </summary>
    /// <remarks>
    /// The paths SHOULD be a set of JSON Pointer values that identify the path(s) to the erroneous field(s) within the request document or parameters.
    /// 
    /// A few examples of JSON Pointers:
    /// <list type="table">
    ///   <listheader>
    ///     <term>Path</term>
    ///     <description>Description</description>
    ///   </listheader>
    ///   <item>
    ///     <term><c>/foo/bar</c></term>
    ///     <description>The json field <c>foo.bar</c> in the body.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>/$QUERY/baz</c></term>
    ///     <description>The query parameter <c>baz</c>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>/$HEADER/x-value</c></term>
    ///     <description>The header <c>X-Value</c> (headers are case insensitive, so it's lowercased).</description>
    ///   </item>
    ///   <item>
    ///     <term><c>/$HEADER/authorization/bearer</c></term>
    ///     <description>The bearer token value in a authorization header.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>/$PATH/id</c></term>
    ///     <description>The path parameter <c>id</c>.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public ImmutableArray<string> Paths => _paths;

    /// <summary>
    /// Gets the extensions.
    /// </summary>
    public ProblemExtensionData Extensions => _extensions;

    /// <summary>
    /// Implicitly converts a <see cref="ProblemDescriptor"/> to a <see cref="ProblemInstance"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    public static implicit operator ValidationErrorInstance(ValidationErrorDescriptor descriptor) =>
        new ValidationErrorInstance(descriptor, paths: [], extensions: []);
}
