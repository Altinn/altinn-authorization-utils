﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding health checks to the <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class AltinnHealthChecksExtensions
{
    /// <summary>
    /// Adds a HealthCheckRegistration if one hasn't already been added to the builder.
    /// </summary>
    public static void TryAddHealthCheck(this IHostApplicationBuilder builder, HealthCheckRegistration healthCheckRegistration)
    {
        builder.TryAddHealthCheck(healthCheckRegistration.Name, hcBuilder => hcBuilder.Add(healthCheckRegistration));
    }

    /// <summary>
    /// Invokes the <paramref name="addHealthCheck"/> action if the given <paramref name="name"/> hasn't already been added to the builder.
    /// </summary>
    public static void TryAddHealthCheck(this IHostApplicationBuilder builder, string name, Action<IHealthChecksBuilder> addHealthCheck)
    {
        var healthCheckKey = $"Altinn.HealthChecks.{name}";
        if (!builder.Properties.ContainsKey(healthCheckKey))
        {
            builder.Properties[healthCheckKey] = true;
            addHealthCheck(builder.Services.AddHealthChecks());
        }
    }
}
