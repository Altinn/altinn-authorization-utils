using Nerdbank.Streams;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Altinn.Authorization.TestUtils.Http;

/// <summary>
/// Extensions for <see cref="HttpResponseMessage"/> to provide Shouldly-like assertions.
/// </summary>
[ShouldlyMethods]
[DebuggerStepThrough]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpResponseShouldExtensions
{
    /// <summary>
    /// Asserts that the HTTP response has a successful status code (2xx).
    /// </summary>
    /// <remarks>This method checks the <see cref="HttpResponseMessage.IsSuccessStatusCode"/> property.  If
    /// the status code is not successful, it reads the response content and throws a  <see
    /// cref="ShouldAssertException"/> with detailed information about the failure.</remarks>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to validate.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the response does not have a successful status code (2xx).</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task ShouldHaveSuccessStatusCode(this HttpResponseMessage response, string? customMessage = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            var responseData = await ResponseData.Read(response, TestContext.Current.CancellationToken);
            throw new ShouldAssertException(new HttpResponseActualShouldlyMessage(responseData, response.StatusCode, customMessage).ToString());
        }
    }

    /// <summary>
    /// Asserts that the HTTP response has the specified status code.
    /// </summary>
    /// <remarks>This method reads the response content to provide detailed information in the exception
    /// message  if the assertion fails. Ensure the response content is accessible and not already disposed.</remarks>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to validate.</param>
    /// <param name="expected">The expected <see cref="HttpStatusCode"/> to compare against the response's status code.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.  If not provided, a default
    /// message will be generated.</param>
    /// <returns></returns>
    /// <exception cref="ShouldAssertException">Thrown if the response's status code does not match the expected status code.  The exception message includes
    /// details about the actual response and the expected status code.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task ShouldHaveStatusCode(this HttpResponseMessage response, HttpStatusCode expected, string? customMessage = null)
    {
        if (response.StatusCode != expected)
        {
            var responseData = await ResponseData.Read(response, TestContext.Current.CancellationToken);
            throw new ShouldAssertException(new HttpResponseActualShouldlyMessage(responseData, response.StatusCode, expected, customMessage).ToString());
        }
    }

    /// <summary>
    /// Asserts that the HTTP response contains valid JSON content of the specified type.
    /// </summary>
    /// <remarks>This method reads and buffers the response content, then attempts to deserialize it into the
    /// specified type <typeparamref name="T"/>.  If the deserialization fails or the content is invalid, a detailed
    /// exception is thrown, including the response data and an optional custom message.</remarks>
    /// <typeparam name="T">The type to which the JSON content should be deserialized.</typeparam>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to validate and deserialize.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.  If not provided, a default
    /// message will be used.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to customize the JSON deserialization process.  If not provided,
    /// the default options will be used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object of type
    /// <typeparamref name="T"/>.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the response content is not valid JSON, cannot be deserialized to the specified type,  or if the
    /// deserialized content is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<T> ShouldHaveJsonContent<T>(this HttpResponseMessage response, string? customMessage = null, JsonSerializerOptions? options = null)
    {
        var content = await BufferContent(response, TestContext.Current.CancellationToken);

        try
        {
            var parsed = await content.ReadFromJsonAsync<T>(options, TestContext.Current.CancellationToken);
            if (parsed is null)
            {
                var responseData = await ResponseData.Read(response, TestContext.Current.CancellationToken);
                throw new ShouldAssertException(new HttpResponseActualShouldlyMessage(responseData, "null", customMessage).ToString());
            }

            return parsed;
        }
        catch (JsonException ex)
        {
            var responseData = await ResponseData.Read(response, TestContext.Current.CancellationToken);
            throw new ShouldAssertException(new HttpResponseActualShouldlyMessage(responseData, ex, customMessage).ToString());
        }
    }

    private static async Task<SequenceHttpContent> BufferContent(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is SequenceHttpContent preBuffered)
        {
            return preBuffered;
        }

        Sequence<byte>? buffer = null;
        SequenceHttpContent? sequenceContent = null;

        try
        {
            buffer = new(ArrayPool<byte>.Shared);
            await response.Content.CopyToAsync(buffer.AsStream(), cancellationToken);
            sequenceContent = new SequenceHttpContent(buffer);
            buffer = null;

            foreach (var h in response.Content.Headers)
            {
                sequenceContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            var result = sequenceContent;
            response.Content = result;
            sequenceContent = null;
            return result;
        }
        finally
        {
            sequenceContent?.Dispose();
            buffer?.Dispose();
        }
    }

    private sealed class ResponseData
    {
        public HttpMethod? RequestMethod { get; }

        public Uri? RequestUri { get; }

        public Version Version { get; }

        public HttpStatusCode StatusCode { get; }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; }

        public string Content { get; }

        public ResponseData(
            HttpMethod? httpMethod,
            Uri? requestUri,
            Version version,
            HttpStatusCode statusCode,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            string content)
        {
            RequestMethod = httpMethod;
            RequestUri = requestUri;
            Version = version;
            StatusCode = statusCode;
            Headers = headers;
            Content = content;
        }

        public static async Task<ResponseData> Read(HttpResponseMessage message, CancellationToken cancellationToken)
        {
            var buffered = await BufferContent(message, cancellationToken);
            var method = message.RequestMessage?.Method;
            var uri = message.RequestMessage?.RequestUri;

            return new(
                method,
                uri,
                message.Version,
                message.StatusCode,
                message.Headers.Concat(buffered.Headers),
                await buffered.ReadAsStringAsync(cancellationToken));
        }
    }

    private sealed class HttpResponseActualShouldlyMessage
        : ShouldlyMessage
    {
        private readonly ResponseData _response;

        public HttpResponseActualShouldlyMessage(ResponseData response, object? actual, string? customMessage = null, [CallerMemberName] string shouldlyMethod = null!)
        {
            _response = response;
            ShouldlyAssertionContext = new ShouldlyAssertionContext(shouldlyMethod)
            {
                Actual = actual,
            };

            if (customMessage != null)
            {
                ShouldlyAssertionContext.CustomMessage = customMessage;
            }
        }

        public HttpResponseActualShouldlyMessage(ResponseData response, object? actual, object? expected, string? customMessage = null, [CallerMemberName] string shouldlyMethod = null!)
            : this(response, actual, customMessage, shouldlyMethod)
        {
            ShouldlyAssertionContext.Expected = expected;
        }

        public override string ToString()
        {
            var context = ShouldlyAssertionContext;
            var codePart = context.CodePart;
            var actual = context.Actual;
            var expected = context.Expected;

            var actualString =
                $"""

                {actual}
                """;

            var expectedString = expected is null
                ? string.Empty
                : $"""

                   {StringHelpers.ToStringAwesomely(expected)}
                """;

            var message =
                $"""
                 {codePart}
                     {StringHelpers.PascalToSpaced(context.ShouldMethod)}{expectedString}
                     but was{actualString}
                 """;

            if (ShouldlyAssertionContext.CustomMessage != null)
            {
                message += $"""


                        Additional Info:
                            {ShouldlyAssertionContext.CustomMessage}
                        """;
            }

            message +=
                $"""

                ===== RESPONSE =====
                {PrintResponse(_response)}
                """;

            return message;
        }

        private static string PrintResponse(ResponseData message)
        {
            var sb = new StringBuilder();
            if (message.RequestMethod is not null && message.RequestUri is not null)
            {
                sb.Append(message.RequestMethod).Append(' ').Append(message.RequestUri).AppendLine();
            }

            sb.AppendLine($"HTTP/{message.Version} {message.StatusCode}");
            foreach (var header in message.Headers)
            {
                sb.Append($"{header.Key}: ").AppendJoin(", ", header.Value).AppendLine();
            }

            sb.AppendLine().AppendLine(message.Content);
            return sb.ToString();
        }
    }
}

