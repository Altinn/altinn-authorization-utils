using System.Collections.Concurrent;
using System.CommandLine;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

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
    /// <param name="defaultValueBox">The box containing the default value for the argument, if any.</param>
    /// <returns>The created argument.</returns>
    public abstract Argument<T> Create<T>(string name, string? description, StrongBox<T>? defaultValueBox);

    /// <summary>
    /// Creates an argument of the specified type with the given parameters.
    /// </summary>
    /// <param name="type">The argument type.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="defaultValueBox">The box containing the default value for the argument, if any.</param>
    /// <returns>The created argument.</returns>
    public virtual Argument Create(Type type, string name, string? description, StrongBox<object?>? defaultValueBox)
        => TypedFactory.ForType(type).Create(this, name, description, defaultValueBox);

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

        public abstract Argument Create(ArgumentFactory factory, string name, string? description, StrongBox<object?>? defaultValueBox);
    }

    private sealed class TypedFactory<T>
        : TypedFactory
    {
        public override Argument Create(ArgumentFactory factory, string name, string? description, StrongBox<object?>? defaultValueBox)
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
                    ThrowHelper.ThrowInvalidOperationException($"The default value of type '{TypeNameHelper.GetTypeDisplayName(defaultValueBox.Value.GetType(), fullName: false)}' cannot be assigned to an argument of type '{TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: false)}'.");
                }
            }

            return factory.Create<T>(name, description, typedDefaultValueBox);
        }
    }
}
