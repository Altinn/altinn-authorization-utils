using System.Collections.Concurrent;
using System.CommandLine;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a factory for creating command line options.
/// </summary>
public abstract class OptionFactory
{
    /// <summary>
    /// Creates an option of the specified type with the given parameters.
    /// </summary>
    /// <typeparam name="T">The option type.</typeparam>
    /// <param name="name">The option name.</param>
    /// <param name="aliases">The option aliases.</param>
    /// <param name="description">The option description.</param>
    /// <param name="isRequired">Indicates whether the option is required.</param>
    /// <param name="defaultValueBox">The box containing the default value for the option, if any.</param>
    /// <returns>The created option.</returns>
    public abstract Option<T> Create<T>(string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, StrongBox<T>? defaultValueBox);

    /// <summary>
    /// Creates an option of the specified type with the given parameters.
    /// </summary>
    /// <param name="type">The option type.</param>
    /// <param name="name">The option name.</param>
    /// <param name="aliases">The option aliases.</param>
    /// <param name="description">The option description.</param>
    /// <param name="isRequired">Indicates whether the option is required.</param>
    /// <param name="defaultValueBox">The box containing the default value for the option, if any.</param>
    /// <returns>The created option.</returns>
    public virtual Option Create(Type type, string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, StrongBox<object?>? defaultValueBox)
        => TypedFactory.ForType(type).Create(this, name, aliases, description, isRequired, defaultValueBox);

    private abstract class TypedFactory
    {
        private static readonly ConcurrentDictionary<Type, TypedFactory> _factories = new();

        /// <summary>
        /// Creates a typed factory for the specified type.
        /// </summary>
        /// <param name="type">The option type.</param>
        /// <returns>A typed factory for the specified type.</returns>
        public static TypedFactory ForType(Type type)
            => _factories.GetOrAdd(type, t => (TypedFactory)Activator.CreateInstance(typeof(TypedFactory<>).MakeGenericType(t))!);

        public abstract Option Create(OptionFactory factory, string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, StrongBox<object?>? defaultValueBox);
    }

    private sealed class TypedFactory<T>
        : TypedFactory
    {
        public override Option Create(OptionFactory factory, string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, StrongBox<object?>? defaultValueBox)
        {
            StrongBox<T>? typedDefaultValueBox = null;
            if (defaultValueBox is not null)
            {
                typedDefaultValueBox = new();

                if (defaultValueBox.Value is null)
                {
                    typedDefaultValueBox.Value = default(T)!;
                }
                else if (defaultValueBox.Value is T tValue)
                {
                    typedDefaultValueBox.Value = tValue;
                }
                else
                {
                    ThrowHelper.ThrowInvalidOperationException($"The default value of type '{TypeNameHelper.GetTypeDisplayName(defaultValueBox.Value.GetType(), fullName: false)}' cannot be assigned to an option of type '{TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: false)}'.");
                }
            }

            return factory.Create<T>(name, aliases, description, isRequired, typedDefaultValueBox);
        }
    }
}
