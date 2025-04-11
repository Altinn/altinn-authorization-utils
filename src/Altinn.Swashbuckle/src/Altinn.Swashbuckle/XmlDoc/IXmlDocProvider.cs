using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Provides XML documentation for types and members.
/// </summary>
public interface IXmlDocProvider
{
    /// <summary>
    /// Gets the XML documentation for a type.
    /// </summary>
    /// <param name="type">The type to get XML documentation for.</param>
    /// <param name="node">Output <see cref="XPathNavigator"/> with documentation for <paramref name="type"/>, if it exists.</param>
    /// <returns><see langword="true"/> if documentation for <paramref name="type"/> was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetXmlDoc(Type type, [NotNullWhen(true)] out XPathNavigator? node);

    /// <summary>
    /// Gets the XML documentation for a member.
    /// </summary>
    /// <param name="member">The member to get XML documentation for.</param>
    /// <param name="node">Output <see cref="XPathNavigator"/> with documentation for <paramref name="member"/>, if it exists.</param>
    /// <returns><see langword="true"/> if documentation for <paramref name="member"/> was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetXmlDoc(MemberInfo member, [NotNullWhen(true)] out XPathNavigator? node);

    /// <summary>
    /// Gets the XML documentation for a method.
    /// </summary>
    /// <param name="method">The method to get XML documentation for.</param>
    /// <param name="node">Output <see cref="XPathNavigator"/> with documentation for <paramref name="method"/>, if it exists.</param>
    /// <returns><see langword="true"/> if documentation for <paramref name="method"/> was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetXmlDoc(MethodInfo method, [NotNullWhen(true)] out XPathNavigator? node);

    /// <summary>
    /// Gets the XML documentation for a field.
    /// </summary>
    /// <param name="field">The field to get XML documentation for.</param>
    /// <param name="node">Output <see cref="XPathNavigator"/> with documentation for <paramref name="field"/>, if it exists.</param>
    /// <returns><see langword="true"/> if documentation for <paramref name="field"/> was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetXmlDoc(FieldInfo field, [NotNullWhen(true)] out XPathNavigator? node);

    /// <summary>
    /// Gets the XML documentation for a property.
    /// </summary>
    /// <param name="property">The property to get XML documentation for.</param>
    /// <param name="node">Output <see cref="XPathNavigator"/> with documentation for <paramref name="property"/>, if it exists.</param>
    /// <returns><see langword="true"/> if documentation for <paramref name="property"/> was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetXmlDoc(PropertyInfo property, [NotNullWhen(true)] out XPathNavigator? node);
}
