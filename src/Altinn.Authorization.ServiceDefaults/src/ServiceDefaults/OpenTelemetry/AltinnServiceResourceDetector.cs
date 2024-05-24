using OpenTelemetry.Resources;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

internal class AltinnServiceResourceDetector
    : IResourceDetector
{
    private readonly Resource _resource;

    public AltinnServiceResourceDetector(AltinnServiceDescriptor serviceDescription)
    {
        var attributes = new List<KeyValuePair<string, object>>(2)
        {
            KeyValuePair.Create("service.name", (object)serviceDescription.Name),
        };

        if (serviceDescription.IsLocalDev)
        {
            attributes.Add(KeyValuePair.Create("altinn.local_dev", (object)true));
        }

        _resource = new Resource(attributes);
    }

    public Resource Detect()
        => _resource;
}
