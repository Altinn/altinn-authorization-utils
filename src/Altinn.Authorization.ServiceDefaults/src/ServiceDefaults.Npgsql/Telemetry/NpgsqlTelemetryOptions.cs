namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

internal sealed class NpgsqlTelemetryOptions
    : INpgsqlScopeTelemetryOptions
{
    private readonly AltinnNpgsqlTelemetryChainNode? _parent;
    private string? _summary;
    private string? _spanName;
    private bool? _shouldTrace;
    private Dictionary<string, NpgsqlTelemetryParameterFilterResult>? _parameterByName;
    private Dictionary<Type, NpgsqlTelemetryParameterFilterResult>? _parameterByType;

    internal NpgsqlTelemetryOptions(AltinnNpgsqlTelemetryChainNode? parent)
    {
        _parent = parent;
    }

    public string? Summary
    {
        get => _summary ?? _parent?.Summary;
        set => _summary = value;
    }

    public string? SpanName
    {
        get => _spanName ?? _parent?.SpanName;
        set => _spanName = value;
    }

    public bool? ShouldTrace
    {
        get => _shouldTrace ?? _parent?.ShouldTrace;
        set => _shouldTrace = value;
    }

    public void SetParameterFilter(string parameterName, NpgsqlTelemetryParameterFilterResult filterResult)
    {
        _parameterByName ??= [];
        _parameterByName[parameterName] = filterResult;
    }

    public void SetParameterFilter(Type parameterType, NpgsqlTelemetryParameterFilterResult filterResult)
    {
        _parameterByType ??= [];
        _parameterByType[parameterType] = filterResult;
    }

    internal AltinnNpgsqlTelemetryChainNode? ToNode()
    {
        if (_summary is null && _spanName is null && _shouldTrace is null && _parameterByName is null && _parameterByType is null)
        {
            return null;
        }

        Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? _parameterFilter = null;
        if (_parameterByName is not null || _parameterByType is not null)
        {
            _parameterFilter = (parameter, name, type) =>
            {
                if (_parameterByName is not null
                    && _parameterByName.TryGetValue(name, out var result)
                    && result is not NpgsqlTelemetryParameterFilterResult.Default)
                {
                    return result;
                }

                if (_parameterByType is not null
                    && _parameterByType.TryGetValue(type, out result)
                    && result is not NpgsqlTelemetryParameterFilterResult.Default)
                {
                    return result;
                }

                return NpgsqlTelemetryParameterFilterResult.Default;
            };
        }

        return new AltinnNpgsqlTelemetryChainNode(
            parent: _parent,
            summary: _summary,
            spanName: _spanName,
            shouldTrace: _shouldTrace,
            parameterFilter: _parameterFilter);
    }
}
