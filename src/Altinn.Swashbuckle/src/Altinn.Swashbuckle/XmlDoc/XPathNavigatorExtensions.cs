﻿using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

internal static class XPathNavigatorExtensions
{
    private const string EmptyNamespace = "";

    internal static XPathNodeIterator SelectChildren(this XPathNavigator navigator, string name)
    {
        return navigator.SelectChildren(name, EmptyNamespace);
    }

    internal static string GetAttribute(this XPathNavigator navigator, string name)
    {
        return navigator.GetAttribute(name, EmptyNamespace);
    }

    internal static XPathNavigator? SelectFirstChild(this XPathNavigator navigator, string name)
    {
        return navigator.SelectChildren(name, EmptyNamespace)
                ?.OfType<XPathNavigator>()
                .FirstOrDefault();
    }

    internal static XPathNavigator? SelectFirstChildWithAttribute(this XPathNavigator navigator, string name, string attributeName, string? attributeValue)
    {
        return navigator.SelectChildren(name, EmptyNamespace)
                ?.OfType<XPathNavigator>()
                .FirstOrDefault(n => n.GetAttribute(attributeName, EmptyNamespace) == attributeValue);
    }
}
