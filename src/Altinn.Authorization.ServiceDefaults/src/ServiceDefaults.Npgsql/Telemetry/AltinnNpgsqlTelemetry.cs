using CommunityToolkit.Diagnostics;
using NpgsqlTypes;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

internal sealed class AltinnNpgsqlTelemetry
{
    private static readonly string RedactedPlaceholder = "REDACTED";
    private static readonly AsyncLocal<AltinnNpgsqlTelemetryChainNode?> _chain = new();
    private static readonly ConcurrentDictionary<Type, Type> _typeMap = new();

    internal static Builder CreateBuilder() => new();

    private static AltinnNpgsqlTelemetryChainNode? Chain => _chain.Value;

    internal static IDisposable? StartScope(Action<INpgsqlScopeTelemetryOptions> configure)
    {
        var prev = Chain;
        var options = new NpgsqlTelemetryOptions(prev);

        configure(options);

        var node = options.ToNode();
        if (node is null)
        {
            return null;
        }

        _chain.Value = node;
        return new Scope(prev);
    }

    private readonly FrozenDictionary<string, NpgsqlTelemetryParameterFilterResult> _parameterByName;
    private readonly FrozenDictionary<Type, NpgsqlTelemetryParameterFilterResult> _parameterByType;

    public AltinnNpgsqlTelemetry(
        IReadOnlyDictionary<string, NpgsqlTelemetryParameterFilterResult> parameterByNameFilters,
        IReadOnlyDictionary<Type, NpgsqlTelemetryParameterFilterResult> parameterByTypeFilters)
    {
        _parameterByName = parameterByNameFilters.ToFrozenDictionary();
        _parameterByType = parameterByTypeFilters.ToFrozenDictionary();

        if (_parameterByName.Values.Any(static result => result == NpgsqlTelemetryParameterFilterResult.Default))
        {
            ThrowHelper.ThrowArgumentException(nameof(parameterByNameFilters), "Parameter name filters cannot have undefined as a value.");
        }

        if (_parameterByType.Values.Any(static result => result == NpgsqlTelemetryParameterFilterResult.Default))
        {
            ThrowHelper.ThrowArgumentException(nameof(parameterByTypeFilters), "Parameter type filters cannot have undefined as a value.");
        }
    }

    public string? GetCommandSpanName(NpgsqlCommand command)
        => Chain?.SpanName;

    public string? GetBatchSpanName(NpgsqlBatch batch)
        => Chain?.SpanName;

    public bool ShouldTraceCommand(NpgsqlCommand command)
    {
        if (string.Equals(command.CommandText, NpgsqlConsts.HealthCheckCommandText, StringComparison.Ordinal))
        {
            return false;
        }

        return Chain?.ShouldTrace ?? true;
    }

    public bool ShouldTraceBatch(NpgsqlBatch batch)
        => Chain?.ShouldTrace ?? true;

    public void EnrichCommand(Activity activity, NpgsqlCommand command)
    {
        var chain = Chain;

        if (command.Parameters.Count > 0)
        {
            foreach (NpgsqlParameter param in command.Parameters)
            {
                var result = Filter(this, chain, param);
                Debug.Assert(result != NpgsqlTelemetryParameterFilterResult.Default);

                if (result == NpgsqlTelemetryParameterFilterResult.Ignore)
                {
                    continue;
                }

                object? value = param.Value;
                if (value is DBNull)
                {
                    value = null;
                }

                if (result == NpgsqlTelemetryParameterFilterResult.RedactValue)
                {
                    value = RedactedPlaceholder;
                }

                activity.SetTag($"db.query.parameters.{param.ParameterName}", value);
            }
        }

        static NpgsqlTelemetryParameterFilterResult Filter(AltinnNpgsqlTelemetry telemetry, AltinnNpgsqlTelemetryChainNode? chain, NpgsqlParameter parameter)
        {
            var name = parameter.ParameterName;
            var type = GetType(parameter);

            type = _typeMap.GetOrAdd(type, UnwrapType);

            var result = RunFilter(telemetry, chain, parameter, name, type);
            if (result != NpgsqlTelemetryParameterFilterResult.Default)
            {
                return result;
            }

            return NpgsqlTelemetryParameterFilterResult.Ignore;
        }

        static Type GetType(NpgsqlParameter parameter)
        {
            var paramType = parameter.GetType();
            if (paramType.IsConstructedGenericType && paramType.GetGenericTypeDefinition() == typeof(NpgsqlParameter<>)) 
            {
                var type = paramType.GetGenericArguments()[0];
                if (type != typeof(object))
                {
                    return type;
                }
            }

            return parameter.Value?.GetType() ?? typeof(object);
        }

        static Type UnwrapType(Type type)
        {
            Type original;

            do
            {
                original = type;
                if (Nullable.GetUnderlyingType(type) is { } underlyingType)
                {
                    type = underlyingType;
                }

                if (type.IsEnum)
                {
                    type = typeof(Enum);
                }

                if (type.IsSZArray)
                {
                    type = type.GetElementType()!;
                }

                if (type.IsConstructedGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>)
                        || genericDef == typeof(IList<>)
                        || genericDef == typeof(IReadOnlyList<>)
                        || genericDef == typeof(IEnumerable<>)
                        || genericDef == typeof(ICollection<>)
                        || genericDef == typeof(IReadOnlyCollection<>)
                        || genericDef == typeof(Memory<>)
                        || genericDef == typeof(ReadOnlyMemory<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }
                }
            }
            while (type != original);

            return type;
        }

        static NpgsqlTelemetryParameterFilterResult RunFilter(AltinnNpgsqlTelemetry telemetry, AltinnNpgsqlTelemetryChainNode? chain, NpgsqlParameter parameter, string name, Type type)
        {
            return chain?.FilterParameter(parameter, name, type) switch
            {
                null or NpgsqlTelemetryParameterFilterResult.Default => telemetry.DefaultFilterParameter(parameter, name, type),
                { } result => result,
            };
        }
    }

    public void EnrichBatch(Activity activity, NpgsqlBatch batch)
    {
        // No enrichment for batches at the moment, but this is where it would go if we wanted to add any.
    }

    internal NpgsqlTelemetryParameterFilterResult DefaultFilterParameter(NpgsqlParameter parameter, string name, Type type)
    {
        if (_parameterByName.TryGetValue(name, out var result))
        {
            return result;
        }

        if (_parameterByType.TryGetValue(type, out result))
        {
            return result;
        }

        return NpgsqlTelemetryParameterFilterResult.Default;
    }

    internal sealed class Builder
        : INpgsqlTelemetryOptions
    {
        private readonly Dictionary<string, NpgsqlTelemetryParameterFilterResult> _parameterByNameFilters = new();
        private readonly Dictionary<Type, NpgsqlTelemetryParameterFilterResult> _parameterByTypeFilters = new()
        {
            { typeof(Guid), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(int), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(long), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(short), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(byte), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(sbyte), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(bool), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(decimal), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(double), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(float), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(DateTime), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(DateTimeOffset), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(DateOnly), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(TimeOnly), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(TimeSpan), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(Enum), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(string), NpgsqlTelemetryParameterFilterResult.RedactValue },
            { typeof(NpgsqlCidr), NpgsqlTelemetryParameterFilterResult.RedactValue },
            { typeof(IPAddress), NpgsqlTelemetryParameterFilterResult.RedactValue },
            { typeof(PhysicalAddress), NpgsqlTelemetryParameterFilterResult.RedactValue },
        };

        public void SetParameterFilter(string parameterName, NpgsqlTelemetryParameterFilterResult filterResult)
        {
            _parameterByNameFilters[parameterName] = filterResult;
        }

        public void SetParameterFilter(Type parameterType, NpgsqlTelemetryParameterFilterResult filterResult)
        {
            _parameterByTypeFilters[parameterType] = filterResult;
        }

        public AltinnNpgsqlTelemetry Build()
            => new(_parameterByNameFilters, _parameterByTypeFilters);
    }

    private sealed class Scope(AltinnNpgsqlTelemetryChainNode? previous)
        : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _chain.Value = previous;
            }
        }
    }
}
