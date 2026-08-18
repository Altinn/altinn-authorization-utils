using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.CommandLine.PreProcessing;
using Altinn.Authorization.CommandLine.Results;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            foreach (var metadata in result.Metadata)
            {
                builder.Metadata.Add(metadata);
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

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a <see cref="IConfigureOption"/> service to the service collection.
        /// </summary>
        /// <typeparam name="TConfigureOption">The configure option type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddConfigureOption<TConfigureOption>()
            where TConfigureOption : class, IConfigureOption
        {
            services.TryAddSingleton<TConfigureOption>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOption, TConfigureOption>(s => s.GetRequiredService<TConfigureOption>()));

            return services;
        }

        /// <summary>
        /// Adds a <see cref="IConfigureArgument"/> service to the service collection.
        /// </summary>
        /// <typeparam name="TConfigureArgument">The configure argument type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddConfigureArgument<TConfigureArgument>()
            where TConfigureArgument : class, IConfigureArgument
        {
            services.TryAddSingleton<TConfigureArgument>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureArgument, TConfigureArgument>(s => s.GetRequiredService<TConfigureArgument>()));

            return services;
        }

        /// <summary>
        /// Adds a command result handler resolver to the service collection.
        /// </summary>
        /// <typeparam name="TResolver">The resolver type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCommandResultHandlerResolver<TResolver>()
            where TResolver : class, ICommandResultHandlerResolver
        {
            services.TryAddSingleton<TResolver>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandResultHandlerResolver, TResolver>(s => s.GetRequiredService<TResolver>()));

            return services;
        }

        /// <summary>
        /// Adds an output format to the service collection.
        /// </summary>
        /// <typeparam name="T">The output format type.</typeparam>
        public IServiceCollection AddOutputFormat<T>()
            where T : class, IFormat
        {
            services.TryAddSingleton<T>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFormat, T>(s => s.GetRequiredService<T>()));

            return services;
        }

        /// <summary>
        /// Adds an output formatter to the service collection.
        /// </summary>
        /// <typeparam name="TFormat">The output format type.</typeparam>
        /// <typeparam name="TFormatter">The formatter type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddOutputFormatter<TFormat, TFormatter>()
            where TFormat : class, IFormat
            where TFormatter : class, IFormatter<TFormat>
        {
            services.AddOutputFormat<TFormat>();
            services.TryAddSingleton<TFormatter>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFormatter<TFormat>, TFormatter>(static s => s.GetRequiredService<TFormatter>()));

            return services;
        }

        /// <summary>
        /// Adds an argument preprocessor to the service collection.
        /// </summary>
        /// <typeparam name="TPreProcessor">The argument preprocessor type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddArgumentPreProcessor<TPreProcessor>()
            where TPreProcessor : class, IArgumentPreProcessor
        {
            services.TryAddSingleton<TPreProcessor>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IArgumentPreProcessor, TPreProcessor>(s => s.GetRequiredService<TPreProcessor>()));

            return services;
        }

        /// <summary>
        /// Adds an argument substitutor to the service collection.
        /// </summary>
        /// <typeparam name="TSubstitutor">The argument substitutor.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddArgumentSubstitutor<TSubstitutor>()
            where TSubstitutor : class, IArgumentSubstitutor
        {
            services.TryAddSingleton<TSubstitutor>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IArgumentSubstitutor, TSubstitutor>(s => s.GetRequiredService<TSubstitutor>()));

            return services;
        }

        /// <summary>
        /// Adds an environment variable substitutor to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddEnvironmentVariableSubstitutor()
        {
            services.AddArgumentSubstitutor<EnvironmentVariableSubstitutor>();

            return services;
        }

        /// <summary>
        /// Adds a URI variable substitutor to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddUriVariableSubstitutor()
        {
            services.AddArgumentSubstitutor<UriVariableSubstitutor>();

            return services;
        }

        /// <summary>
        /// Adds an argument URI resolver to the service collection.
        /// </summary>
        /// <typeparam name="TResolver">The argument URI resolver type.</typeparam>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddArgumentUriResolver<TResolver>()
            where TResolver : class, IArgumentUriResolver
        {
            services.TryAddSingleton<TResolver>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IArgumentUriResolver, TResolver>(s => s.GetRequiredService<TResolver>()));

            return services;
        }
    }
}
