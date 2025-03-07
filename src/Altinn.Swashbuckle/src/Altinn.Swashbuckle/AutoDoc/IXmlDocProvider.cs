using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.AutoDoc;

public interface IXmlDocProvider
{
    public bool TryGetXmlDoc(Type type, [NotNullWhen(true)] out XPathNavigator? typeNode);

    public bool TryGetXmlDoc(MemberInfo member, [NotNullWhen(true)] out XPathNavigator? memberNode);

    public bool TryGetXmlDoc(MethodInfo method, [NotNullWhen(true)] out XPathNavigator? memberNode);

    public bool TryGetXmlDoc(FieldInfo method, [NotNullWhen(true)] out XPathNavigator? memberNode);

    public bool TryGetXmlDoc(PropertyInfo method, [NotNullWhen(true)] out XPathNavigator? memberNode);
}

internal class XmlDocProvider
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

    public bool TryGetXmlDoc(Type type, [NotNullWhen(true)] out XPathNavigator? typeNode)
    {
        if (!TryGetAssemblyDoc(type.Assembly, out var assemblyDoc))
        {
            typeNode = null;
            return false;
        }

        var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
        return assemblyDoc.TryGetValue(typeMemberName, out typeNode);
    }

    public bool TryGetXmlDoc(MemberInfo member, [NotNullWhen(true)] out XPathNavigator? memberNode)
    {
        memberNode = null;

        return member switch
        {
            MethodInfo method => TryGetXmlDoc(method, out memberNode),
            FieldInfo field => TryGetXmlDoc(field, out memberNode),
            PropertyInfo property => TryGetXmlDoc(property, out memberNode),
            Type type => TryGetXmlDoc(type, out memberNode),
            _ => false,
        };
    }

    public bool TryGetXmlDoc(MethodInfo method, [NotNullWhen(true)] out XPathNavigator? memberNode)
    {
        if (method.DeclaringType is null || !TryGetAssemblyDoc(method.DeclaringType.Assembly, out var assemblyDoc))
        {
            memberNode = null;
            return false;
        }

        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(method);

        return assemblyDoc.TryGetValue(methodMemberName, out memberNode);
    }

    public bool TryGetXmlDoc(FieldInfo field, [NotNullWhen(true)] out XPathNavigator? memberNode)
    {
        if (field.DeclaringType is null || !TryGetAssemblyDoc(field.DeclaringType.Assembly, out var assemblyDoc))
        {
            memberNode = null;
            return false;
        }

        var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(field);

        return assemblyDoc.TryGetValue(fieldOrPropertyMemberName, out memberNode);
    }

    public bool TryGetXmlDoc(PropertyInfo property, [NotNullWhen(true)] out XPathNavigator? memberNode)
    {
        if (property.DeclaringType is null || !TryGetAssemblyDoc(property.DeclaringType.Assembly, out var assemblyDoc))
        {
            memberNode = null;
            return false;
        }

        var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(property);

        return assemblyDoc.TryGetValue(fieldOrPropertyMemberName, out memberNode);
    }
}
