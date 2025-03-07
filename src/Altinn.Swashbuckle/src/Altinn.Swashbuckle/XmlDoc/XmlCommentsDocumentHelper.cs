using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

internal static class XmlCommentsDocumentHelper
{
    internal static Dictionary<string, XPathNavigator> CreateMemberDictionary(XPathDocument xmlDoc)
    {
        var members = xmlDoc.CreateNavigator()
            .SelectFirstChild("doc")
            ?.SelectFirstChild("members")
            ?.SelectChildren("member")
            ?.OfType<XPathNavigator>();

        if (members == null)
        {
            return new();
        }

        return members.ToDictionary(memberNode => memberNode.GetAttribute("name"));
    }
}
