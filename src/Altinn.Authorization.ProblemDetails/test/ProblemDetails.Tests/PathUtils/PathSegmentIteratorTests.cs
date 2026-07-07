using System.Collections.Immutable;
using Altinn.Authorization.ProblemDetails.PathUtils;
using Xunit.Sdk;

namespace Altinn.Authorization.ProblemDetails.Tests.PathUtils;

public class PathSegmentIteratorTests
{
    [Theory]
    [MemberData(nameof(GetCases))]
    public void ProducesExpectedSegments(TestCase testCase)
    {
        var iterator = PathSegmentIterator.Create(testCase.Path);
        var segments = new List<(string Segment, PathSegmentType Type)>();

        foreach (var segment in iterator)
        {
            segments.Add((segment.Value.ToString(), segment.Type));
        }

        segments.ShouldBe(testCase.Expected);
    }

    public static TheoryData<TestCase> GetCases()
    {
        return new TheoryData<TestCase>
        {
            new TestCase(""),
            new TestCase("Foo", [P("Foo")]),
            new TestCase("Foo.Bar.Baz", [P("Foo"), P("Bar"), P("Baz")]),
            new TestCase("Foo.Bar[0].Baz", [P("Foo"), P("Bar"), I("0"), P("Baz")]),
            new TestCase("[0]", [I("0")]),
            new TestCase("[0].Foo", [I("0"), P("Foo")]),
            new TestCase("Foo[0]", [P("Foo"), I("0")]),
            new TestCase(".Foo", [P("Foo")]),
            new TestCase("Foo.", [P("Foo")]),
            new TestCase("Foo..Bar", [P("Foo"), P("Bar")]),
            new TestCase("Foo[0][1].Bar", [P("Foo"), I("0"), I("1"), P("Bar")]),
        };

        static (string Segment, PathSegmentType Type) P(string segment) => (segment, PathSegmentType.Property);
        static (string Segment, PathSegmentType Type) I(string segment) => (segment, PathSegmentType.Indexer);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Foo", "Foo")]
    [InlineData("Foo.Bar", "Foo")]
    [InlineData("Foo[0]", "Foo")]
    [InlineData("[0].Foo", "")]
    public void GetInitialSegment_ReturnsSegmentBeforeFirstSeparator(string path, string expected)
    {
        var result = PathSegmentIterator.GetInitialSegment(path);

        result.ToString().ShouldBe(expected);
    }

    public sealed class TestCase
        : IXunitSerializable
    {
        private string _path;
        private ImmutableArray<(string Segment, PathSegmentType Type)> _expected;

        public string Path
            => _path;

        public ImmutableArray<(string Segment, PathSegmentType Type)> Expected
            => _expected;

        public TestCase()
        {
            _path = null!; // will be set by deserialize
            _expected = default; // will be set by deserialize
        }

        public TestCase(string path, params ReadOnlySpan<(string Segment, PathSegmentType Type)> expected)
        {
            _path = path;
            _expected = [.. expected];
        }

        public override string ToString()
            => _path;

        void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
        {
            _path = info.GetValue<string>(nameof(_path))!;
            _expected = [.. info.GetValue<(string Segment, PathSegmentType Type)[]>(nameof(_expected))!];
        }

        void IXunitSerializable.Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(_path), _path);
            info.AddValue(nameof(_expected), _expected.ToArray());
        }
    }
}
