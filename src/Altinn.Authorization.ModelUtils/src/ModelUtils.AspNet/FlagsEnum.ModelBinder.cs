using Altinn.Authorization.ModelUtils.EnumUtils;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Altinn.Authorization.ModelUtils.AspNet;

public sealed partial class FlagsEnum
{
    internal sealed class ModelBinderProvider
        : IModelBinderProvider
    {
        private static ConcurrentDictionary<Type, ObjectFactory> _factories = new();

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            Guard.IsNotNull(context);

            if (IsFlagsEnumModelType(context.Metadata.ModelType, out var enumType))
            {
                var factory = _factories.GetOrAdd(enumType, static t => CreateFactory(t));
                return (IModelBinder)factory(context.Services, []);
            }

            return null;

            static ObjectFactory CreateFactory(Type enumType)
            {
                var type = typeof(ModelBinder<>).MakeGenericType(enumType);
                return ActivatorUtilities.CreateFactory(type, []);
            }
        }
    }

    internal sealed class ModelBinder<TEnum>
        : IModelBinder
        where TEnum : struct, Enum
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            Guard.IsNotNull(bindingContext);

            var values = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (values.Length == 0)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var hasErrors = false;
            var result = default(TEnum);
            foreach (var value in values)
            {
                if (!FlagsEnum<TEnum>.Model.TryParse(value, out var partialResult))
                {
                    hasErrors = true;
                }
                else
                {
                    result = result.BitwiseOr(partialResult);
                }
            }

            if (hasErrors)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            FlagsEnum<TEnum> final = result;
            bindingContext.Result = ModelBindingResult.Success(final);
            return Task.CompletedTask;
        }
    }
}
