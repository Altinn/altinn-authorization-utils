using Altinn.Swashbuckle.Examples;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace Altinn.Urn.Swashbuckle;

internal sealed class UrnExampleDataProviderResolver
    : IExampleDataProviderResolver
{
    public ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options)
    {
        if (type.IsAssignableTo(typeof(IKeyValueUrn)))
        {
            var iface = type.GetInterfaces().FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IKeyValueUrn<,>));
            if (iface is null)
            {
                return null;
            }

            var urnType = iface.GetGenericArguments()[0];
            var variantEnumType = iface.GetGenericArguments()[1];
            if (urnType == type)
            {
                var method = _getBaseUrnProvider.MakeGenericMethod(urnType, variantEnumType);
                return (ExampleDataProvider?)method.Invoke(null, [options]);
            } 
            else
            {
                var variantIface = type.GetInterfaces().FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IKeyValueUrnVariant<,,,>));
                if (variantIface is null)
                {
                    return null;
                }

                var variantType = variantIface.GetGenericArguments()[0];
                urnType = variantIface.GetGenericArguments()[1];
                variantEnumType = variantIface.GetGenericArguments()[2];
                var valueType = variantIface.GetGenericArguments()[3];
                
                var method = _getVariantUrnProvider.MakeGenericMethod(variantType, urnType, variantEnumType, valueType);
                return (ExampleDataProvider?)method.Invoke(null, [options]);
            }
        }

        return null;
    }

    private static MethodInfo _getBaseUrnProvider = typeof(UrnExampleDataProviderResolver)
        .GetMethod(nameof(GetBaseUrnProvider), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static ExampleDataProvider? GetBaseUrnProvider<TUrn, TVariants>(ExampleDataOptions options)
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
    {
        var variantProviders = ImmutableArray.CreateBuilder<ExampleDataProvider<TUrn>>(TUrn.Variants.Length);
        foreach (var variant in TUrn.Variants)
        {
            var variantType = TUrn.VariantTypeFor(variant);
            var provider = options.GetProvider(variantType);
            if (provider is null)
            {
                continue;
            }

            var method = _convertToBaseProvider.MakeGenericMethod(variantType, typeof(TUrn), typeof(TVariants));
            variantProviders.Add((ExampleDataProvider<TUrn>)method.Invoke(null, [provider])!);
        }

        return new BaseUrnExampleDataProvider<TUrn, TVariants>(variantProviders.DrainToImmutable());
    }

    private static MethodInfo _convertToBaseProvider = typeof(UrnExampleDataProviderResolver)
        .GetMethod(nameof(ConvertToBaseProvider), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static ExampleDataProvider<TUrn> ConvertToBaseProvider<TVariant, TUrn, TVariants>(ExampleDataProvider variantProvider)
        where TVariant : IKeyValueUrnVariant<TUrn, TVariants>
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
    {
        Debug.Assert(typeof(TVariant).IsAssignableTo(typeof(TUrn)));
        var asVariant = variantProvider.AsTypedProvider<TVariant>();

        return asVariant.Select(static v => (TUrn)(object)v);
    }

    private static MethodInfo _getVariantUrnProvider = typeof(UrnExampleDataProviderResolver)
        .GetMethod(nameof(GetVariantUrnProvider), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static ExampleDataProvider? GetVariantUrnProvider<TVariant, TUrn, TVariants, TValue>(ExampleDataOptions options)
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
        where TVariant : TUrn, IKeyValueUrnVariant<TVariant, TUrn, TVariants, TValue>
    {
        var valueProvider = options.GetProvider<TValue>();

        if (valueProvider is null)
        {
            return null;
        }

        return new VariantUrnExampleDataProvider<TVariant, TUrn, TVariants, TValue>(valueProvider);
    }

    private class VariantUrnExampleDataProvider<TVariant, TUrn, TVariants, TValue>
        : ExampleDataProvider<TVariant>
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
        where TVariant : TUrn, IKeyValueUrnVariant<TVariant, TUrn, TVariants, TValue>
    {
        private readonly ExampleDataProvider<TValue> _valueProvider;

        public VariantUrnExampleDataProvider(ExampleDataProvider<TValue> valueProvider)
        {
            _valueProvider = valueProvider;
        }

        public override IEnumerable<TVariant>? GetExamples(ExampleDataOptions options)
            => _valueProvider.GetExamples(options)?.Select(TVariant.Create);
    }

    private class BaseUrnExampleDataProvider<TUrn, TVariants>
        : ExampleDataProvider<TUrn>
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
    {
        private readonly ImmutableArray<ExampleDataProvider<TUrn>> _variantProviders;

        public BaseUrnExampleDataProvider(ImmutableArray<ExampleDataProvider<TUrn>> variantProviders)
        {
            _variantProviders = variantProviders;
        }

        public override IEnumerable<TUrn>? GetExamples(ExampleDataOptions options)
        {
            var data = new List<IEnumerator<TUrn>?>(_variantProviders.Length);
            foreach (var provider in _variantProviders)
            {
                var examples = provider.GetExamples(options);
                if (examples is not null)
                {
                    data.Add(examples.GetEnumerator());
                }
            }
            
            if (data.Count == 0)
            {
                return null;
            }

            return GetExamples(data);
        }

        private static IEnumerable<TUrn> GetExamples(List<IEnumerator<TUrn>?> data)
        {
            var remaining = data.Count;
            while (remaining > 0)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var enumerator = data[i];
                    if (enumerator is null)
                    {
                        continue;
                    }

                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    else
                    {
                        enumerator.Dispose();
                        data[i] = null;
                        remaining--;
                    }
                }
            }
        }
    }
}
