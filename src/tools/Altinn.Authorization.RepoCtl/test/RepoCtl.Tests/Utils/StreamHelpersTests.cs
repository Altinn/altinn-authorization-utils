using System.Text;
using Altinn.Authorization.RepoCtl.Utils;

namespace Altinn.Authorization.RepoCtl.Tests.Utils;

public class StreamHelpersTests
{
    [Theory]
    [InlineData("abc\ndef", false, "abc\ndef")]
    [InlineData("abc\rdef", false, "abc\ndef")]
    [InlineData("abc\r\ndef", false, "abc\ndef")]
    [InlineData("a\r\nb\rc\nd", false, "a\nb\nc\nd")]
    [InlineData("\n", true, "")]
    [InlineData("\nabc", true, "abc")]
    [InlineData("\nabc\rdef", true, "abc\ndef")]
    [InlineData("\rabc", true, "\nabc")]
    public void NormalizeLineEndings_NormalizesLineEndings(
        string input,
        bool lastByteWasCr,
        string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        Span<byte> data = bytes;
        var firstLineBreakIndex = data.IndexOfAny((byte)'\r', (byte)'\n');

        StreamHelpers.NormalizeLineEndings(ref data, firstLineBreakIndex, lastByteWasCr);

        Encoding.UTF8.GetString(data).ShouldBe(expected);
    }
}
