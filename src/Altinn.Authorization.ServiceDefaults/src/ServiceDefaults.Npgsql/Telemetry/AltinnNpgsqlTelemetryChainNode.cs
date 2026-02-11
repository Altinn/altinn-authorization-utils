namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

internal sealed class AltinnNpgsqlTelemetryChainNode
{
    private readonly string? _spanName;
    private readonly bool? _shouldTrace;
    private readonly Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? _parameterFilter;

    public AltinnNpgsqlTelemetryChainNode(
        AltinnNpgsqlTelemetryChainNode? parent,
        string? spanName,
        bool? shouldTrace,
        Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? parameterFilter)
    {
        _spanName = spanName;
        _shouldTrace = shouldTrace;
        _parameterFilter = CombineFilters(parameterFilter, parent?._parameterFilter);
    }

    public string? SpanName => _spanName;

    public bool? ShouldTrace => _shouldTrace;

    public NpgsqlTelemetryParameterFilterResult FilterParameter(NpgsqlParameter parameter, string name, Type type)
        => _parameterFilter is null
        ? NpgsqlTelemetryParameterFilterResult.Default
        : _parameterFilter(parameter, name, type);

    private static Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? CombineFilters(
        Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? next,
        Func<NpgsqlParameter, string, Type, NpgsqlTelemetryParameterFilterResult>? parent)
    {
        if (next is null)
        {
            return parent;
        }

        if (parent is null)
        {
            return next;
        }

        return (parameter, name, type) => next(parameter, name, type) switch
        {
            NpgsqlTelemetryParameterFilterResult.Default => parent(parameter, name, type),
            var result => result,
        };
    }
}
