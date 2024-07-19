using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.TypeMapping;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Used to configure a PostgreSQL database.
/// </summary>
public interface INpgsqlDatabaseBuilder
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> for the database being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// The <see cref="IConfiguration"/> for the database being configured.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the <see cref="NpgsqlDataSourceBuilder"/>.
    /// </summary>
    /// <param name="configure">A configuration delegate.</param>
    /// <returns><see langword="this"/>.</returns>
    public INpgsqlDatabaseBuilder Configure(Action<NpgsqlDataSourceBuilder> configure);

    /// <inheritdoc cref="INpgsqlTypeMapper.MapEnum{TEnum}(string?, INpgsqlNameTranslator?)"/>
    public INpgsqlDatabaseBuilder MapEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum;

    /// <inheritdoc cref="INpgsqlTypeMapper.MapEnum(Type, string?, INpgsqlNameTranslator?)"/>
    [RequiresDynamicCode("Calling MapEnum with a Type can require creating new generic types or methods. This may not work when AOT compiling.")]
    public INpgsqlDatabaseBuilder MapEnum(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type clrType,
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null);

    /// <inheritdoc cref="INpgsqlTypeMapper.MapComposite{T}(string?, INpgsqlNameTranslator?)"/>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public INpgsqlDatabaseBuilder MapComposite<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null);

    /// <inheritdoc cref="INpgsqlTypeMapper.MapComposite(Type, string?, INpgsqlNameTranslator?)"/>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public INpgsqlDatabaseBuilder MapComposite(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] Type clrType,
        string? pgName = null,
        INpgsqlNameTranslator? nameTranslator = null);

    /// <inheritdoc cref="INpgsqlTypeMapper.ConfigureJsonOptions(JsonSerializerOptions)"/>
    public INpgsqlDatabaseBuilder ConfigureJsonOptions(JsonSerializerOptions serializerOptions);

    /// <inheritdoc cref="INpgsqlTypeMapper.EnableDynamicJson(Type[], Type[])"/>
    [RequiresUnreferencedCode("Json serializer may perform reflection on trimmed types.")]
    [RequiresDynamicCode(
        "Serializing arbitrary types to json can require creating new generic types or methods, which requires creating code at runtime. This may not work when AOT compiling.")]
    public INpgsqlDatabaseBuilder EnableDynamicJson(Type[]? jsonbClrTypes = null, Type[]? jsonClrTypes = null);

    /// <inheritdoc cref="INpgsqlTypeMapper.EnableRecordsAsTuples()"/>
    [RequiresUnreferencedCode(
        "The mapping of PostgreSQL records as .NET tuples requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The mapping of PostgreSQL records as .NET tuples requires dynamic code usage which is incompatible with NativeAOT.")]
    public INpgsqlDatabaseBuilder EnableRecordsAsTuples();

    /// <inheritdoc cref="INpgsqlTypeMapper.EnableUnmappedTypes()"/>
    [RequiresUnreferencedCode(
        "The use of unmapped enums, ranges or multiranges requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The use of unmapped enums, ranges or multiranges requires dynamic code usage which is incompatible with NativeAOT.")]
    public INpgsqlDatabaseBuilder EnableUnmappedTypes();
}
