using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Altinn.Authorization.CommandLine.Factory;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Extensions for <see cref="CliApplication"/> and <see cref="CliApplicationBuilder"/>.
/// </summary>
public static class CliApplicationExtensions
{
    extension<T>(Option<T> option)
    {
        /// <summary>
        /// Sets both <see cref="Option{T}.CustomParser"/> <strong>and</strong> <see cref="Option{T}.DefaultValueFactory"/> to the specified function.
        /// </summary>
        public Func<ArgumentResult, T> ParserAndValueFactory
        {
            set
            {
                Guard.IsNotNull(value, nameof(value));
                option.CustomParser = value;
                option.DefaultValueFactory = value;
            }
        }
    }

    extension(ICommandBuilder builder)
    {
        /// <summary>
        /// Sets the command handler for the command being built, using the specified delegate.
        /// </summary>
        /// <param name="handler">The command handler.</param>
        public void SetHandler(Delegate handler)
        {
            Guard.IsNotNull(handler);

            var result = CommandHandlerDelegateFactory.Create(handler, builder.ApplicationServices);
            builder.SetHandler(result);
        }

        /// <summary>
        /// Sets the command handler for the command being built using the specified type.
        /// </summary>
        /// <remarks>
        /// The specified type must have a public instance method named <c>Invoke</c> or <c>InvokeAsync</c> that is not abstract, generic, or static.
        /// </remarks>
        /// <typeparam name="T">The command handler type.</typeparam>
        public void SetHandler<T>()
            where T : class
        {
            var type = typeof(T);
            var candidates = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.IsPublic && !m.IsSpecialName && !m.IsGenericMethod && !m.IsAbstract && !m.IsStatic && IsInvokeMethod(m.Name))
                .ToArray();

            if (candidates.Length == 0)
            {
                ThrowHelper.ThrowInvalidOperationException($"Type '{TypeNameHelper.GetTypeDisplayName(type, fullName: false)}' does not have any public Invoke/InvokeAsync methods that is not abstract, generic, or static.");
            }

            if (candidates.Length > 1)
            {
                ThrowHelper.ThrowInvalidOperationException($"Type '{TypeNameHelper.GetTypeDisplayName(type, fullName: false)}' has multiple public Invoke/InvokeAsync methods that is not abstract, generic, or static.");
            }

            var result = CommandHandlerDelegateFactory.Create(type, candidates[0], builder.ApplicationServices);
            builder.SetHandler(result);

            static bool IsInvokeMethod(string methodName)
                => string.Equals(methodName, "Invoke", StringComparison.OrdinalIgnoreCase)
                || string.Equals(methodName, "InvokeAsync", StringComparison.OrdinalIgnoreCase);
        }

        private void SetHandler(CommandHandlerDelegateResult result)
        {
            foreach (var option in result.Options)
            {
                builder.Options.Add(option);
            }

            foreach (var argument in result.Arguments)
            {
                builder.Arguments.Add(argument);
            }

            builder.CommandHandler = result.Delegate;
        }

        /// <summary>
        /// Adds a subcommand to the command being built, with the specified name, description, and command handler.
        /// </summary>
        /// <param name="name">The subcommand name.</param>
        /// <param name="description">The subcommand description.</param>
        /// <param name="handler">The command handler for the subcommand.</param>
        /// <returns>The command convention builder for the subcommand.</returns>
        public ICommandConventionBuilder AddCommand(string name, string description, Delegate handler)
            => builder.AddCommand(name, description, b => b.SetHandler(handler));

        /// <summary>
        /// Adds a subcommand to the command being built, with the specified name, description, and command handler type.
        /// </summary>
        /// <typeparam name="T">The command handler type.</typeparam>
        /// <param name="name">The subcommand name.</param>
        /// <param name="description">The subcommand description.</param>
        /// <returns>The command convention builder for the subcommand.</returns>
        public ICommandConventionBuilder AddCommand<T>(string name, string description)
            where T : class
            => builder.AddCommand(name, description, b => b.SetHandler<T>());
    }
}
