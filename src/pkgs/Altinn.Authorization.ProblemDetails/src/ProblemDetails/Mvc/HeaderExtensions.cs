using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Altinn.Authorization.ProblemDetails.Mvc;

internal static class HeaderExtensions
{
    extension(StringValues values)
    {
        public IList<MediaTypeHeaderValue> GetMediaTypeHeaderValueList()
        {
            if (StringValues.IsNullOrEmpty(values))
            {
                return [];
            }

            return MediaTypeHeaderValue.TryParseList(values, out var result)
                ? result
                : [];
        }
    }
}
