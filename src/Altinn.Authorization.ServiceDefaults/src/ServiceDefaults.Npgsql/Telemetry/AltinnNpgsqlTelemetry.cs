using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

internal sealed partial class AltinnNpgsqlTelemetry
{
    public static readonly string RedactedPlaceholder = "REDACTED";
    private static readonly AsyncLocal<AltinnNpgsqlTelemetryChainNode?> _chain = new();
    private static readonly ConcurrentDictionary<Type, Type> _typeMap = new();

    internal static Builder CreateBuilder(ILogger<AltinnNpgsqlTelemetry> logger) => new(logger);

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

    private readonly QueryHasher _queryHasher;
    private readonly FrozenDictionary<string, NpgsqlTelemetryParameterFilterResult> _parameterByName;
    private readonly FrozenDictionary<Type, NpgsqlTelemetryParameterFilterResult> _parameterByType;
    private readonly Func<string, bool> _excludeQuery;

    public AltinnNpgsqlTelemetry(
        IReadOnlyDictionary<string, NpgsqlTelemetryParameterFilterResult> parameterByNameFilters,
        IReadOnlyDictionary<Type, NpgsqlTelemetryParameterFilterResult> parameterByTypeFilters,
        Func<string, bool> excludeQuery,
        ILogger<AltinnNpgsqlTelemetry> logger)
    {
        _queryHasher = new QueryHasher(logger);

        _parameterByName = parameterByNameFilters.ToFrozenDictionary();
        _parameterByType = parameterByTypeFilters.ToFrozenDictionary();
        _excludeQuery = excludeQuery;

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
        if (_excludeQuery(command.CommandText))
        {
            return false;
        }

        return Chain?.ShouldTrace ?? true;
    }

    public bool ShouldTraceBatch(NpgsqlBatch batch)
    {
        foreach (var command in batch.BatchCommands)
        {
            // if any of the commands are not excluded, we trace the batch as a whole
            if (!_excludeQuery(command.CommandText))
            {
                return Chain?.ShouldTrace ?? true;
            }
        }

        return false;
    }

    public void EnrichCommand(Activity activity, NpgsqlCommand command)
    {
        EnrichCommand(activity, command.CommandText, command.Parameters);
    }

    public void EnrichCommand(Activity activity, NpgsqlBatchCommand command)
    {
        EnrichCommand(activity, command.CommandText, command.Parameters);
    }

    private void EnrichCommand(Activity activity, string commandText, NpgsqlParameterCollection parameters)
    {
        RemoveUselessTags(activity);
        UpdateStatement(activity, [commandText]);

        var chain = Chain;
        SetSummary(activity, chain);

        if (parameters.Count > 0)
        {
            foreach (NpgsqlParameter param in parameters)
            {
                var result = FilterParameter(this, chain, param);
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
    }

    public void EnrichBatch(Activity activity, NpgsqlBatch batch)
    {
        var count = batch.BatchCommands.Count;

        if (count == 1)
        {
            EnrichCommand(activity, batch.BatchCommands[0]);
            return;
        }

        RemoveUselessTags(activity);
        activity.SetTag("db.operation.batch.size", count);
        string[] queries = ArrayPool<string>.Shared.Rent(count);
        try 
        {
            for (int i = 0; i < count; i++)
            {
                queries[i] = batch.BatchCommands[i].CommandText;
            }

            UpdateStatement(activity, queries.AsSpan(0, count));
        }
        finally
        {
            ArrayPool<string>.Shared.Return(queries, clearArray: true);
        }

        var chain = Chain;
        SetSummary(activity, chain);
    }

    private static void RemoveUselessTags(Activity activity)
    {
        // Npgsql adds a lot of tags by default, but many of them are not useful for us and just add noise to our telemetry.
        // We remove them here to keep our telemetry clean and focused on what we care about.
        activity.SetTag("db.connection_id", null);
        activity.SetTag("db.connection_string", null);
        activity.SetTag("db.name", null);
        activity.SetTag("db.user", null);
        activity.SetTag("db.npgsql.data_source", null);
        activity.SetTag("db.npgsql.connection_id", null);
        activity.SetTag("net.peer.ip", null);
        activity.SetTag("net.peer.name", null);
        activity.SetTag("net.peer.port", null);
        activity.SetTag("net.transport", null);
        activity.SetTag("server.address", null);
        activity.SetTag("server.port", null);

        // we rename to newer spec tags
        activity.SetTag("db.system.name", "postgres");
        activity.SetTag("db.system", null);
    }

    private void UpdateStatement(Activity activity, ReadOnlySpan<string> queries)
    {
        Debug.Assert(queries.Length > 0);
        activity.SetTag("db.statement", null);
        activity.SetTag("db.query.text", null);

        if (queries.Length == 1)
        {
            var hash = _queryHasher.Hash(queries[0]);
            activity.SetTag("db.query.hash", hash);
            activity.DisplayName = hash;
        }
        else
        {
            var hash = _queryHasher.Hash(queries);
            activity.SetTag("db.query.hash", hash);
            activity.DisplayName = $"batch count={queries.Length}";
        }
    }

    private static void SetSummary(Activity activity, AltinnNpgsqlTelemetryChainNode? chain)
    {
        if (chain?.Summary is { } summary)
        {
            activity.SetTag("db.query.summary", summary);
            activity.DisplayName = summary;
        }

        if (chain?.SpanName is { } spanName)
        {
            activity.DisplayName = spanName;
        }
    }

    private static NpgsqlTelemetryParameterFilterResult FilterParameter(AltinnNpgsqlTelemetry telemetry, AltinnNpgsqlTelemetryChainNode? chain, NpgsqlParameter parameter)
    {
        var name = parameter.ParameterName;
        var type = GetParameterType(parameter);

        type = _typeMap.GetOrAdd(type, UnwrapType);

        var result = RunFilter(telemetry, chain, parameter, name, type);
        if (result != NpgsqlTelemetryParameterFilterResult.Default)
        {
            return result;
        }

        return NpgsqlTelemetryParameterFilterResult.Ignore;
    }

    private static Type GetParameterType(NpgsqlParameter parameter)
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

    private static Type UnwrapType(Type type)
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

    private static NpgsqlTelemetryParameterFilterResult RunFilter(AltinnNpgsqlTelemetry telemetry, AltinnNpgsqlTelemetryChainNode? chain, NpgsqlParameter parameter, string name, Type type)
    {
        return chain?.FilterParameter(parameter, name, type) switch
        {
            null or NpgsqlTelemetryParameterFilterResult.Default => telemetry.DefaultFilterParameter(parameter, name, type),
            { } result => result,
        };
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

    internal sealed class Builder(ILogger<AltinnNpgsqlTelemetry> logger)
        : INpgsqlDataSourceTelemetryOptions
    {
        private readonly Dictionary<string, NpgsqlTelemetryParameterFilterResult> _parameterByNameFilters = new();
        private readonly Dictionary<Type, NpgsqlTelemetryParameterFilterResult> _parameterByTypeFilters = new()
        {
            { typeof(Guid), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(int), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(long), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(short), NpgsqlTelemetryParameterFilterResult.Include },
            { typeof(byte), NpgsqlTelemetryParameterFilterResult.Ignore }, // this is typically a byte-array, which we don't want to include
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
            { typeof(string), NpgsqlTelemetryParameterFilterResult.Ignore },
            { typeof(NpgsqlCidr), NpgsqlTelemetryParameterFilterResult.Ignore },
            { typeof(IPAddress), NpgsqlTelemetryParameterFilterResult.Ignore },
            { typeof(PhysicalAddress), NpgsqlTelemetryParameterFilterResult.Ignore },
        };

        private readonly HashSet<string> _excludedQueries = [
            NpgsqlConsts.HealthCheckCommandText,

            // well known queries used internally by Npgsql
            "select pg_is_in_recovery()",
            "SHOW default_transaction_read_only",
        ];
        private readonly List<Func<string, bool>> _excludeQueryDelegates = new(8);

        public void SetParameterFilter(string parameterName, NpgsqlTelemetryParameterFilterResult filterResult)
        {
            _parameterByNameFilters[parameterName] = filterResult;
        }

        public void SetParameterFilter(Type parameterType, NpgsqlTelemetryParameterFilterResult filterResult)
        {
            _parameterByTypeFilters[parameterType] = filterResult;
        }

        public void ExcludeQuery(string query)
        {
            _excludedQueries.Add(query);
        }

        public void ExcludeQuery(Func<string, bool> excludePredicate)
        {
            _excludeQueryDelegates.Add(excludePredicate);
        }

        public AltinnNpgsqlTelemetry Build()
        {
            var searchValues = SearchValues.Create([.. _excludedQueries], StringComparison.Ordinal);
            _excludeQueryDelegates.Add(CreateExcludeBySearchValueDelegate(searchValues));

            return new(_parameterByNameFilters, _parameterByTypeFilters, CreateExcludeQueryDelegate([.. _excludeQueryDelegates]), logger);

            static Func<string, bool> CreateExcludeBySearchValueDelegate(SearchValues<string> searchValues)
                => searchValues.Contains;

            static Func<string, bool> CreateExcludeQueryDelegate(ImmutableArray<Func<string, bool>> exludeDelegates) 
            {
                Debug.Assert(!exludeDelegates.IsDefaultOrEmpty);

                if (exludeDelegates.Length == 1)
                {
                    return exludeDelegates[0];
                }

                return query =>
                {
                    foreach (var excludeDelegate in exludeDelegates)
                    {
                        if (excludeDelegate(query))
                        {
                            return true;
                        }
                    }

                    return false;
                };
            }
        }
    }

    internal sealed class OptionsFactory(
        ILogger<AltinnNpgsqlTelemetry> logger,
        IEnumerable<IConfigureOptions<INpgsqlDataSourceTelemetryOptions>> setups,
        IEnumerable<IPostConfigureOptions<INpgsqlDataSourceTelemetryOptions>> postConfigures,
        IEnumerable<IValidateOptions<INpgsqlDataSourceTelemetryOptions>> validations)
        : OptionsFactory<INpgsqlDataSourceTelemetryOptions>(setups, postConfigures, validations)
    {
        protected override INpgsqlDataSourceTelemetryOptions CreateInstance(string name)
            => CreateBuilder(logger);
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

    internal sealed class QueryHasher
    {
        private readonly HashSet<int> _seen = new();
        private readonly Lock _lock = new();
        private readonly ILogger _logger;

        public QueryHasher(ILogger logger)
        {
            _logger = logger;
        }

        public string Hash(string query)
        {
            var (hash, hashString) = ComputeHashAndString(query);

            bool isNew;
            lock (_lock)
            {
                isNew = _seen.Add(hash.GetHashCode());
            }

            if (isNew)
            {
                Log.NewQueryHash(_logger, hashString, query);
            }

            return hashString;
        }

        public string[] Hash(ReadOnlySpan<string> queries)
        {
            Debug.Assert(queries.Length > 1);

            var hashes = new string[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                hashes[i] = Hash(queries[i]);
            }

            return hashes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hash, string HexString) ComputeHashAndString(ReadOnlySpan<char> query)
        {
            var hash = ComputeHash(query);
            var hashString = HashToString(hash);
            return (hash, hashString);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ulong ComputeHash(ReadOnlySpan<char> query)
                => XxHash64.HashToUInt64(MemoryMarshal.AsBytes(query));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static string HashToString(ulong hash)
                => string.Create(16, hash, static (span, value) => Hex.Format(value, span));
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Encountered new query hash {Hash} for query: {Query}")]
        public static partial void NewQueryHash(ILogger logger, string hash, string query);
    }
}
