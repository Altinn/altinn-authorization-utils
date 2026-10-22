using System.Collections.Concurrent;
using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a factory for creating command line arguments.
/// </summary>
public abstract class ArgumentFactory
{
    /// <summary>
    /// Creates an argument of the specified type with the given parameters.
    /// </summary>
    /// <typeparam name="T">The argument type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="defaultValueProvider">The default-value provider, or null when no default is configured.</param>
    /// <returns>The created argument.</returns>
    public abstract Argument<T> Create<T>(string name, string? description, DefaultValueProvider<T>? defaultValueProvider);

    /// <summary>
    /// Creates an argument of the specified type with the given parameters.
    /// </summary>
    /// <param name="type">The argument type.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="defaultValueProvider">The default-value provider, or null when no default is configured.</param>
    /// <returns>The created argument.</returns>
    public virtual Argument Create(Type type, string name, string? description, DefaultValueProvider? defaultValueProvider)
        => TypedFactory.ForType(type).Create(this, name, description, defaultValueProvider);

    private abstract class TypedFactory
    {
        private static readonly ConcurrentDictionary<Type, TypedFactory> _factories = new();

        /// <summary>
        /// Creates a typed factory for the specified type.
        /// </summary>
        /// <param name="type">The argument type.</param>
        /// <returns>A typed factory for the specified type.</returns>
        public static TypedFactory ForType(Type type)
            => _factories.GetOrAdd(type, t => (TypedFactory)Activator.CreateInstance(typeof(TypedFactory<>).MakeGenericType(t))!);

        public abstract Argument Create(ArgumentFactory factory, string name, string? description, DefaultValueProvider? defaultValueProvider);
    }

    private sealed class TypedFactory<T>
        : TypedFactory
    {
        public override Argument Create(ArgumentFactory factory, string name, string? description, DefaultValueProvider? defaultValueProvider)
        {
            return factory.Create<T>(name, description, defaultValueProvider?.Bind<T>());
        }
    }
}
