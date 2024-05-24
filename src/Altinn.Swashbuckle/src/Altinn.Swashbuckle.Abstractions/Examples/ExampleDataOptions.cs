using Altinn.Swashbuckle.Configuration;
using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Options for getting example data.
/// </summary>
public sealed class ExampleDataOptions
{
    /// <summary>
    /// Gets the default options for getting example data.
    /// </summary>
    public static ExampleDataOptions DefaultOptions { get; } = CreateDefaultOptions();

    private IExampleDataProviderResolver? _providerResolver;
    private ProviderList? _providers;

    private ImmutableDictionary<Type, ExampleDataProvider?> _cachingContext
        = ImmutableDictionary<Type, ExampleDataProvider?>.Empty;

    /// <summary>
    /// Specifies whether the current instance has been locked for user modification.
    /// </summary>
    /// <remarks>
    /// A <see cref="ExampleDataOptions"/> instance can be locked either if
    /// it has been passed to one of the <see cref="ExampleData"/> methods,
    /// or a user explicitly called the <see cref="MakeReadOnly()"/> methods on the instance.
    /// </remarks>
    public bool IsReadOnly => _isReadOnly;
    private volatile bool _isReadOnly;

    /// <summary>
    /// Constructs a new <see cref="ExampleDataOptions"/> instance.
    /// </summary>
    public ExampleDataOptions()
    {
        _resolveProvider = ResolveProvider;
    }

    /// <summary>
    /// Copies the options from a <see cref="ExampleDataOptions"/> instance to a new instance.
    /// </summary>
    /// <param name="options">The <see cref="ExampleDataOptions"/> instance to copy options from.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public ExampleDataOptions(ExampleDataOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // The following fields are not copied intentionally:
        // 1. _cachingContext can only be set in immutable options instances.
        // 2. _providerResolverChain can be created lazily as it relies on
        //    _providerResolver as its source of truth.

        _providers = options._providers is { } providers ? new(this, providers) : null;
        _providerResolver = options._providerResolver;

        _resolveProvider = ResolveProvider;
    }

    /// <summary>
    /// Gets an example-data provider for the type.
    /// </summary>
    public ExampleDataProvider<T>? GetProvider<T>()
    {
        var provider = GetProvider(typeof(T));

        return provider?.AsTypedProvider<T>();
    }

    /// <summary>
    /// Gets an example-data provider for the type.
    /// </summary>
    public ExampleDataProvider? GetProvider(Type type)
    {
        MakeReadOnly(populateMissingResolver: true);

        return ImmutableInterlocked.GetOrAdd(ref _cachingContext, type, _resolveProvider);
    }

    private readonly Func<Type, ExampleDataProvider?> _resolveProvider;
    private ExampleDataProvider? ResolveProvider(Type type)
        => (_effectiveProviderResolver ?? _providerResolver)!.GetProvider(type, this);

    /// <summary>
    /// Gets the list of example-data providers.
    /// </summary>
    public IList<ExampleDataProvider> Providers => _providers ??= new(this);
    internal IReadOnlyList<ExampleDataProvider>? ProvidersInternal => _providers;

    /// <summary>
    /// Gets or sets the <see cref="ExampleDataProvider"/> resolver used by this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this property is set after serialization or deserialization has occurred.
    /// </exception>
    /// <remarks>
    /// A <see langword="null"/> setting is equivalent to using the <see cref="DefaultExampleDataProviderResolver" />.
    /// The property will be populated automatically once used with one of the <see cref="ExampleData"/> methods.
    ///
    /// This property is kept in sync with the <see cref="ProviderResolverChain"/> property.
    /// Any change made to this property will be reflected by <see cref="ProviderResolverChain"/> and vice versa.
    /// </remarks>
    public IExampleDataProviderResolver? ProviderResolver
    {
        get
        {
            return _providerResolver;
        }
        set
        {
            VerifyMutable();

            if (_providerResolverChain is { } resolverChain && !ReferenceEquals(resolverChain, value))
            {
                // User is setting a new resolver; invalidate the resolver chain if already created.
                resolverChain.Clear();
                resolverChain.AddFlattened(value);
            }

            _providerResolver = value;
        }
    }

    /// <summary>
    /// Gets the list of chained <see cref="ExampleDataProvider"/> resolvers used by this instance.
    /// </summary>
    /// <remarks>
    /// The ordering of the chain is significant: <see cref="ExampleDataOptions "/> will query each
    /// of the resolvers in their specified order, returning the first result that is non-null.
    /// If all resolvers in the chain return null, then <see cref="ExampleDataOptions"/> will also return null.
    ///
    /// This property is auxiliary to and is kept in sync with the <see cref="ProviderResolver"/> property.
    /// Any change made to this property will be reflected by <see cref="ProviderResolver"/> and vice versa.
    /// </remarks>
    public IList<IExampleDataProviderResolver> ProviderResolverChain => _providerResolverChain ??= new(this);
    private OptionsBoundExampleDataProviderResolverChain? _providerResolverChain;

    /// <summary>
    /// Marks the current instance as read-only preventing any further user modification.
    /// </summary>
    /// <exception cref="InvalidOperationException">The instance does not specify a <see cref="ProviderResolver"/> setting.</exception>
    /// <remarks>This method is idempotent.</remarks>
    public void MakeReadOnly()
    {
        if (_providerResolver is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Cannot make the options read-only without a ProviderResolver setting.");
        }

        _isReadOnly = true;
    }

    /// <summary>
    /// Marks the current instance as read-only preventing any further user modification.
    /// </summary>
    /// <param name="populateMissingResolver">Populates unconfigured <see cref="ProviderResolver"/> properties with the default.</param>
    /// <exception cref="InvalidOperationException">
    /// The instance does not specify a <see cref="ProviderResolver"/> setting. Thrown if <paramref name="populateMissingResolver"/> is <see langword="false"/>.
    /// </exception>
    /// <remarks>
    /// When <paramref name="populateMissingResolver"/> is set to <see langword="true" />, configures the instance using the default <see cref="DefaultExampleDataProviderResolver"/>.
    ///
    /// This method is idempotent.
    /// </remarks>
    public void MakeReadOnly(bool populateMissingResolver)
    {
        if (populateMissingResolver)
        {
            if (!_isConfiguredForExampleData)
            {
                ConfigureForExampleData();
            }
        }
        else
        {
            MakeReadOnly();
        }

        Debug.Assert(IsReadOnly);
    }

    private volatile bool _isConfiguredForExampleData;
    private IExampleDataProviderResolver? _effectiveProviderResolver;

    private void ConfigureForExampleData()
    {
        // Even if a resolver has already been specified, we need to root
        // the default resolver to gain access to the default converters.
        DefaultExampleDataProviderResolver defaultResolver = DefaultExampleDataProviderResolver.DefaultInstance;

        switch (_providerResolver)
        {
            case null:
                // Use the default reflection-based resolver if no resolver has been specified.
                _providerResolver = defaultResolver;
                break;

            default:
                var resolver = new ExampleDataProviderResolverChain();
                resolver.AddFlattened(_providerResolver);
                resolver.AddFlattened(defaultResolver);
                _effectiveProviderResolver = resolver;
                break;
        }

        Debug.Assert(_providerResolver != null);
        // NB preserve write order.
        _isReadOnly = true;
        _isConfiguredForExampleData = true;
    }

    internal void VerifyMutable()
    {
        if (_isReadOnly)
        {
            ThrowHelper.ThrowInvalidOperationException("The options are read-only.");
        }
    }

    private static ExampleDataOptions CreateDefaultOptions()
    {
        ExampleDataOptions options = new();
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    private sealed class ProviderList 
        : ConfigurationList<ExampleDataProvider>
    {
        private readonly ExampleDataOptions _options;

        public ProviderList(ExampleDataOptions options, IList<ExampleDataProvider>? source = null)
            : base(source)
        {
            _options = options;
        }

        public override bool IsReadOnly => _options.IsReadOnly;

        protected override void OnCollectionModifying() => _options.VerifyMutable();
    }

    private sealed class OptionsBoundExampleDataProviderResolverChain 
        : ExampleDataProviderResolverChain
    {
        private readonly ExampleDataOptions _options;

        public OptionsBoundExampleDataProviderResolverChain(ExampleDataOptions options)
        {
            _options = options;

            AddFlattened(options._providerResolver);
        }

        public override bool IsReadOnly => _options.IsReadOnly;

        protected override void ValidateAddedValue(IExampleDataProviderResolver item)
        {
            if (ReferenceEquals(item, this) || ReferenceEquals(item, _options._providerResolver))
            {
                // Cannot add the instances in TypeInfoResolver or TypeInfoResolverChain to the chain itself.
                ThrowHelper.ThrowInvalidOperationException("Invalid chained resolver");
            }
        }

        protected override void OnCollectionModifying()
        {
            _options.VerifyMutable();

            // Collection modified by the user: replace the main
            // resolver with the resolver chain as our source of truth.
            _options._providerResolver = this;
        }
    }
}
