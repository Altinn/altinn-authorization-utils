using Altinn.Swashbuckle.XmlDoc;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocFilterTestsBase
{
    private readonly IXmlDocProvider _provider;

    public XmlDocFilterTestsBase()
    {
        _provider = new XmlDocProvider();
    }

    protected IXmlDocProvider Provider => _provider;
}
