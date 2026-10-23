using Octokit.Webhooks;

namespace Altinn.Authorization.CommandLine.GitHub.Actions;

internal sealed class EventDeserializer
    : WebhookEventProcessor
{
    public static WebhookEvent Deserialize(string eventName, string payload)
    {
        var headers = new WebhookHeaders
        {
            Event = eventName == "pull_request_target" ? "pull_request" : eventName,
        };

        return Instance.DeserializeWebhookEvent(headers, payload);
    }

    private static readonly EventDeserializer Instance
        = new EventDeserializer();

    private EventDeserializer() { }
}
