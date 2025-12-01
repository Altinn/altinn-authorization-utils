using System.Collections.Concurrent;
using System.Reflection;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// Represents a descriptor that provides identifying information about an assembly's meter, including its name and
/// optional version.
/// </summary>
public sealed record AssemblyMeterDescriptor
{
    private readonly static ConcurrentDictionary<Assembly, AssemblyMeterDescriptor> _cache = new();

    /// <summary>
    /// Gets the <see cref="AssemblyMeterDescriptor"/> for the specified <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>A <see cref="AssemblyMeterDescriptor"/>.</returns>
    /// <exception cref="InvalidOperationException">If the assembly's name cannot be determined.</exception>
    public static AssemblyMeterDescriptor For(Assembly assembly)
    {
        return _cache.GetOrAdd(assembly, static assembly =>
        {
            var name = GetName(assembly);
            var version = GetVersion(assembly);

            return new(name, version);
        });

        static string GetName(Assembly assembly)
        {
            if (assembly.GetCustomAttribute<AssemblyMeterNameAttribute>() is { } attr)
            {
                return attr.MeterName;
            }

            var assemblyName = assembly.GetName().Name;

            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new InvalidOperationException($"Cannot determine assembly name for assembly {assembly}");
            }

            return assemblyName;
        }

        static string? GetVersion(Assembly assembly)
        {
            if (assembly.GetCustomAttribute<AssemblyMeterVersionAttribute>() is { } attr)
            {
                return attr.MeterVersion;
            }

            if (assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is { } versionAttr)
            {
                return versionAttr.InformationalVersion;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the <see cref="AssemblyMeterDescriptor"/> for the assembly containing the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>A <see cref="AssemblyMeterDescriptor"/>.</returns>
    /// <exception cref="InvalidOperationException">If the assembly's name cannot be determined.</exception>
    public static AssemblyMeterDescriptor For<T>()
        => For(typeof(T));

    /// <summary>
    /// Gets the <see cref="AssemblyMeterDescriptor"/> for the assembly containing the specified type <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="AssemblyMeterDescriptor"/>.</returns>
    /// <exception cref="InvalidOperationException">If the assembly's name cannot be determined.</exception>
    public static AssemblyMeterDescriptor For(Type type)
        => For(type.Assembly);

    private AssemblyMeterDescriptor(string name, string? version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>
    /// Gets the name associated with the meters defined in this assembly.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the version identifier associated with the meters defined in this assembly.
    /// </summary>
    public string? Version { get; init; }
}
