using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed.FileBased;

internal class SubPathFileProvider
    : IFileProvider
{
    private readonly IFileProvider _inner;
    private readonly string _subPath;

    public static IFileProvider Create(IFileProvider inner, string subPath)
    {
        Guard.IsNotNull(inner);

        if (string.IsNullOrEmpty(subPath) || subPath.Equals("/", StringComparison.Ordinal))
        {
            return inner;
        }

        if (!subPath.EndsWith('/'))
        {
            subPath += '/';
        }

        if (inner is SubPathFileProvider innerSubPath)
        {
            return new SubPathFileProvider(innerSubPath._inner, innerSubPath._subPath + subPath);
        }

        return new SubPathFileProvider(inner, subPath);
    }

    /// <inheritdoc/>
    public IDirectoryContents GetDirectoryContents(string subpath)
        => _inner.GetDirectoryContents(_subPath + subpath);

    /// <inheritdoc/>
    public IFileInfo GetFileInfo(string subpath)
        => _inner.GetFileInfo(_subPath + subpath);

    /// <inheritdoc/>
    public IChangeToken Watch(string filter)
        => _inner.Watch(_subPath + filter);

    private SubPathFileProvider(IFileProvider inner, PathString subPath)
    {
        _inner = inner;
        _subPath = subPath;
    }
}
