using Altinn.Authorization.ServiceDefaults.Telemetry;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.ApplicationInsights;

[ExcludeFromCodeCoverage]
internal class ApplicationInsightsRequestTelemetryEnricherProcessor
    : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationInsightsRequestTelemetryEnricherProcessor(ITelemetryProcessor next, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Process(ITelemetry item)
    {
        if (item is ISupportProperties itemWithProps
            && _httpContextAccessor.HttpContext is { } ctx)
        {
            TelemetryHelpers.EnrichFromRequest(new DictionaryTags(itemWithProps.Properties), ctx);
        }
    }

    private struct DictionaryTags
        : TelemetryHelpers.ITags
    {
        private IDictionary<string, string> _tags;

        public DictionaryTags(IDictionary<string, string> tags)
        {
            _tags = tags;
        }

        public string this[string key] { set => _tags[key] = value; }
    }
}
