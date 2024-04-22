using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace Altinn.Swashbuckle.Testing;

public static class ModelMetadataFactory
{
    private static readonly ModelMetadataProvider _provider = new EmptyModelMetadataProvider();

    public static ModelMetadata CreateForType(Type type)
        => _provider.GetMetadataForType(type);

    public static ModelMetadata CreateForProperty(Type containingType, string propertyName)
        => _provider.GetMetadataForProperty(containingType, propertyName);

    public static ModelMetadata CreateForParameter(ParameterInfo parameterInfo)
        => _provider.GetMetadataForParameter(parameterInfo);
}
