using Altinn.Swashbuckle.XmlDoc;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocProviderTests
{
    private readonly IXmlDocProvider _provider
        = new DefaultXmlDocProvider();

    [Fact]
    public void GetXmlDocForType()
    {
        _provider.TryGetXmlDoc(typeof(DocClass), out var node).ShouldBeTrue();
        node.ShouldNotBeNull();

        var summary = node.SelectFirstChild("summary").ShouldNotBeNull().Value.Trim();
        summary.ShouldBe("Class summary.");

        var remarks = node.SelectFirstChild("remarks").ShouldNotBeNull().Value.Trim();
        remarks.ShouldBe("And a remark");
    }

    [Fact]
    public void GetXmlDocForField()
    {
        _provider.TryGetXmlDoc(typeof(DocClass).GetField(nameof(DocClass.MyField))!, out var node).ShouldBeTrue();
        node.ShouldNotBeNull();

        var summary = node.SelectFirstChild("summary").ShouldNotBeNull().Value.Trim();
        summary.ShouldBe("Field summary.");
    }

    [Fact]
    public void GetXmlDocForProperty()
    {
        _provider.TryGetXmlDoc(typeof(DocClass).GetProperty(nameof(DocClass.MyProperty))!, out var node).ShouldBeTrue();
        node.ShouldNotBeNull();

        var summary = node.SelectFirstChild("summary").ShouldNotBeNull().Value.Trim();
        summary.ShouldBe("Property summary.");
    }

    [Fact]
    public void GetXmlDocForMethod()
    {
        _provider.TryGetXmlDoc(typeof(DocClass).GetMethod(nameof(DocClass.MyMethod))!, out var node).ShouldBeTrue();
        node.ShouldNotBeNull();

        var summary = node.SelectFirstChild("summary").ShouldNotBeNull().Value.Trim();
        summary.ShouldBe("Method summary.");

        var param1 = node.SelectFirstChildWithAttribute("param", "name", "arg1").ShouldNotBeNull().Value.Trim();
        param1.ShouldBe("Arg 1");

        var param2 = node.SelectFirstChildWithAttribute("param", "name", "arg2").ShouldNotBeNull().Value.Trim();
        param2.ShouldBe("Arg 2");

        var returns = node.SelectFirstChild("returns").ShouldNotBeNull().Value.Trim();
        returns.ShouldBe("Return value.");
    }

    [Fact]
    public void GetXmlDocForNestedClass()
    {
        _provider.TryGetXmlDoc(typeof(DocClass.Nested), out var node).ShouldBeTrue();
        node.ShouldNotBeNull();

        var summary = node.SelectFirstChild("summary").ShouldNotBeNull().Value.Trim();
        summary.ShouldBe("Nested class.");
    }

    /// <summary>
    /// Class summary.
    /// </summary>
    /// <remarks>And a remark</remarks>
    public class DocClass
    {
        /// <summary>
        /// Field summary.
        /// </summary>
        public readonly string MyField = "MyField";

        /// <summary>
        /// Property summary.
        /// </summary>
        public string? MyProperty { get; set; }

        /// <summary>
        /// Method summary.
        /// </summary>
        /// <param name="arg1">Arg 1</param>
        /// <param name="arg2">Arg 2</param>
        /// <returns>Return value.</returns>
        public string MyMethod(string arg1, int arg2)
        {
            return arg1 + arg2;
        }

        /// <summary>
        /// Nested class.
        /// </summary>
        public class Nested { }
    }
}
