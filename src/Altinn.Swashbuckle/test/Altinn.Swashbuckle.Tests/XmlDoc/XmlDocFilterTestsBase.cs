using Altinn.Swashbuckle.XmlDoc;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocFilterTestsBase
{
    private readonly IXmlDocProvider _provider;

    public XmlDocFilterTestsBase()
    {
        _provider = new DefaultXmlDocProvider();
    }

    protected IXmlDocProvider Provider => _provider;
}
