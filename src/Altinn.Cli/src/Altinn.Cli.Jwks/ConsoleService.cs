using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;

namespace Altinn.Cli.Jwks;

internal sealed class ConsoleService(IEnumerable<string> args)
{
    private int? _result;

    public ImmutableArray<string> Args { get; } = [.. args];

    public void SetResult(int result)
    {
        if (_result is not null)
        {
            ThrowHelper.ThrowInvalidOperationException("Result has already been set.");
        }

        _result = result;
    }

    public int GetResult()
    {
        if (_result is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Result has not been set.");
        }

        return _result.Value;
    }
}
