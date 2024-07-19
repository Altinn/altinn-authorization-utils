using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

internal class NpgsqlDatabaseBuilder
    : INpgsqlDatabaseBuilder
{
    public NpgsqlDatabaseBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }

    public INpgsqlDatabaseBuilder Configure(Action<NpgsqlDataSourceBuilder> configure)
    {
        Services.AddSingleton<IConfigureOptions<NpgsqlDataSourceBuilder>>(new ConfigureOptions<NpgsqlDataSourceBuilder>(configure));

        return this;
    }

    /// <inheritdoc/>
    public INpgsqlDatabaseBuilder MapEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
        => Configure(builder => builder.MapEnum<TEnum>(pgName, nameTranslator));

    /// <inheritdoc/>
    [RequiresDynamicCode("Calling MapEnum with a Type can require creating new generic types or methods. This may not work when AOT compiling.")]
    public INpgsqlDatabaseBuilder MapEnum(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type clrType,
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null)
        => Configure(builder => builder.MapEnum(clrType, pgName, nameTranslator));

    /// <inheritdoc/>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public INpgsqlDatabaseBuilder MapComposite<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null)
        => Configure(builder => builder.MapComposite<T>(pgName, nameTranslator));

    /// <inheritdoc/>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public INpgsqlDatabaseBuilder MapComposite(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] Type clrType,
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null)
        => Configure(builder => builder.MapComposite(clrType, pgName, nameTranslator));

    /// <inheritdoc/>
    public INpgsqlDatabaseBuilder ConfigureJsonOptions(JsonSerializerOptions serializerOptions)
        => Configure(builder => builder.ConfigureJsonOptions(serializerOptions));

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Json serializer may perform reflection on trimmed types.")]
    [RequiresDynamicCode(
        "Serializing arbitrary types to json can require creating new generic types or methods, which requires creating code at runtime. This may not work when AOT compiling.")]
    public INpgsqlDatabaseBuilder EnableDynamicJson(Type[]? jsonbClrTypes = null, Type[]? jsonClrTypes = null)
        => Configure(builder => builder.EnableDynamicJson(jsonbClrTypes, jsonClrTypes));

    /// <inheritdoc/>
    [RequiresUnreferencedCode(
        "The mapping of PostgreSQL records as .NET tuples requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The mapping of PostgreSQL records as .NET tuples requires dynamic code usage which is incompatible with NativeAOT.")]
    public INpgsqlDatabaseBuilder EnableRecordsAsTuples()
        => Configure(builder => builder.EnableRecordsAsTuples());

    /// <inheritdoc/>
    [RequiresUnreferencedCode(
        "The use of unmapped enums, ranges or multiranges requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The use of unmapped enums, ranges or multiranges requires dynamic code usage which is incompatible with NativeAOT.")]
    public INpgsqlDatabaseBuilder EnableUnmappedTypes()
        => Configure(builder => builder.EnableUnmappedTypes());
}
