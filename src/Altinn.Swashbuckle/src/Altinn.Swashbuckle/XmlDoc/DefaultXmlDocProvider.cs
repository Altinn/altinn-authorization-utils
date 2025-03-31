using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Default implementation of <see cref="IXmlDocProvider"/>.
/// </summary>
public class DefaultXmlDocProvider
    : IXmlDocProvider
{
    private readonly ConcurrentDictionary<Assembly, IReadOnlyDictionary<string, XPathNavigator>?> _cache = new();

    private bool TryGetAssemblyDoc(Assembly assembly, [NotNullWhen(true)] out IReadOnlyDictionary<string, XPathNavigator>? doc)
    {
        doc = _cache.GetOrAdd(assembly, LoadXmlDoc);
        return doc != null;

        static IReadOnlyDictionary<string, XPathNavigator>? LoadXmlDoc(Assembly assembly)
        {
            var assemblyPath = assembly.Location;
            var xmlFile = Path.ChangeExtension(assemblyPath, ".xml");
            if (!File.Exists(xmlFile))
            {
                return null;
            }

            var doc = new XPathDocument(xmlFile);
            return XmlCommentsDocumentHelper.CreateMemberDictionary(doc);
        }
    }

    /// <inheritdoc/>
    public bool TryGetXmlDoc(Type type, [NotNullWhen(true)] out XPathNavigator? node)
    {
        if (!TryGetAssemblyDoc(type.Assembly, out var assemblyDoc))
        {
            node = null;
            return false;
        }

        var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
        return assemblyDoc.TryGetValue(typeMemberName, out node);
    }

    /// <inheritdoc/>
    public bool TryGetXmlDoc(MemberInfo member, [NotNullWhen(true)] out XPathNavigator? node)
    {
        node = null;

        return member switch
        {
            MethodInfo method => TryGetXmlDoc(method, out node),
            FieldInfo field => TryGetXmlDoc(field, out node),
            PropertyInfo property => TryGetXmlDoc(property, out node),
            Type type => TryGetXmlDoc(type, out node),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public bool TryGetXmlDoc(MethodInfo method, [NotNullWhen(true)] out XPathNavigator? node)
    {
        if (method.DeclaringType is null || !TryGetAssemblyDoc(method.DeclaringType.Assembly, out var assemblyDoc))
        {
            node = null;
            return false;
        }

        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(method);

        return assemblyDoc.TryGetValue(methodMemberName, out node);
    }

    /// <inheritdoc/>
    public bool TryGetXmlDoc(FieldInfo field, [NotNullWhen(true)] out XPathNavigator? node)
    {
        if (field.DeclaringType is null || !TryGetAssemblyDoc(field.DeclaringType.Assembly, out var assemblyDoc))
        {
            node = null;
            return false;
        }

        var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(field);

        return assemblyDoc.TryGetValue(fieldOrPropertyMemberName, out node);
    }

    /// <inheritdoc/>
    public bool TryGetXmlDoc(PropertyInfo property, [NotNullWhen(true)] out XPathNavigator? node)
    {
        if (property.DeclaringType is null || !TryGetAssemblyDoc(property.DeclaringType.Assembly, out var assemblyDoc))
        {
            node = null;
            return false;
        }

        var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(property);

        return assemblyDoc.TryGetValue(fieldOrPropertyMemberName, out node);
    }
}
