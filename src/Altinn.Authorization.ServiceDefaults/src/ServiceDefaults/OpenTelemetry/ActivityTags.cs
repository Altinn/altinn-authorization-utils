using Altinn.Authorization.ServiceDefaults.Telemetry;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

[ExcludeFromCodeCoverage]
internal struct ActivityTags
    : TelemetryHelpers.ITags
{
    private Activity _activity;

    public ActivityTags(Activity activity)
    {
        _activity = activity;
    }

    public string this[string key] { set => _activity.AddTag(key, value); }
}
