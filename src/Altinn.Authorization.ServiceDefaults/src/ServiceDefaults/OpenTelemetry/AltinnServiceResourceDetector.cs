using Altinn.Authorization.ServiceDefaults.Telemetry.OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using System.Runtime.InteropServices;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

internal sealed partial class AltinnServiceResourceDetector
    : IResourceDetector
{
    private readonly Resource _resource;

    public AltinnServiceResourceDetector(
        AltinnServiceDescriptor serviceDescription,
        ILogger<AltinnServiceResourceDetector> logger)
    {
        var attributes = new List<KeyValuePair<string, object>>(2)
        {
            new(ServiceAttributes.AttributeServiceName, serviceDescription.Name),
            new(ServiceAttributes.AttributeServiceNamespace, "altinn"),
            new(ServiceAttributes.AttributeServiceInstanceId, Environment.MachineName),
        };

        DetectHostAttributes(attributes);

        if (serviceDescription.IsLocalDev)
        {
            attributes.Add(KeyValuePair.Create("altinn.local_dev", (object)true));
        }
        else
        {
            DetectKubernetesAttributes(attributes, logger);
        }

        _resource = new Resource(attributes);
    }

    public Resource Detect()
        => _resource;

    private static void DetectHostAttributes(List<KeyValuePair<string, object>> attributes)
    {
        MaybeAdd(attributes, HostAttributes.AttributeHostName, Environment.MachineName);
        MaybeAdd(attributes, HostAttributes.AttributeHostArch, MapArchitectureToOtel(RuntimeInformation.OSArchitecture));

        static string? MapArchitectureToOtel(Architecture arch)
            => arch switch
            {
                Architecture.X86 => HostAttributes.HostArchValues.X86,
                Architecture.X64 => HostAttributes.HostArchValues.Amd64,
                Architecture.Arm or Architecture.Armv6 => HostAttributes.HostArchValues.Arm32,
                Architecture.Arm64 => HostAttributes.HostArchValues.Arm64,
                _ => null,
            };

        static void MaybeAdd(List<KeyValuePair<string, object>> attributes, string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                attributes.Add(new(key, value));
            }
        }
    }

    private static void DetectKubernetesAttributes(List<KeyValuePair<string, object>> attributes, ILogger logger)
    {
        AddEnvOrWarn(attributes, logger, K8sAttributes.AttributeK8sContainerName, "K8S_CONTAINER_NAME");
        AddEnvOrWarn(attributes, logger, K8sAttributes.AttributeK8sPodName, "K8S_POD_NAME");
        AddEnvOrWarn(attributes, logger, K8sAttributes.AttributeK8sNamespaceName, "K8S_POD_NAMESPACE");
        AddEnvOrWarn(attributes, logger, K8sAttributes.AttributeK8sNodeName, "K8S_NODE_NAME");

        static void AddEnvOrWarn(List<KeyValuePair<string, object>> attributes, ILogger logger, string key, string envName)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(value))
            {
                Log.MissingKubernetesEnvVar(logger, envName);
            }
            else
            {
                attributes.Add(new(key, value));
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "Missing kubernetes environment variable: '{EnvironmentVariableName}'")]
        public static partial void MissingKubernetesEnvVar(ILogger logger, string environmentVariableName);
    }
}
