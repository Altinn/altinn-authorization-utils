# Altinn.Authorization.ServiceDefaults

Service defaults for altinn authorization services.

## Caching

`AddAltinnCache` registers Microsoft `HybridCache` for the service. The local in-memory tier is always on. Valkey (Redis protocol, provided by the platform's `dis-cache-operator`) is used as the distributed tier when the app enables it and the connection values are configured. Otherwise the cache is local only. The pre-start logger reports the active mode and why during registration, unless `Altinn:Logging:PreStart:Disable` is set to `true`.

```csharp
// Host built with AltinnHost or AddAltinnServiceDefaults:
builder.AddAltinnCache(enabled: distributedCacheFeatureFlag);

// Plain IServiceCollection:
services.AddAltinnCache(configuration, serviceName: "authorization", enabled: distributedCacheFeatureFlag);
```

`Altinn.Authorization.ServiceDefaults.Authorization` registers the same cache (local tier only) for the platform access token signing keys, so the app's `AddAltinnCache` call is what turns the distributed tier on.

Calls without entry options use the configured cache lifetimes. When supplying `HybridCacheEntryOptions` with a custom `Expiration`, also set `LocalCacheExpiration` to bound how long the local copy can remain stale after another instance removes the entry. To keep a value in this instance's memory only, set `Flags = HybridCacheEntryFlags.DisableDistributedCache`.

### Configuration

| Key | Value |
| --- | --- |
| `Altinn:Cache:Valkey:Host` | Valkey host and port: `valkey-<name>.<namespace>.svc.cluster.local:6379` for the `Cache` resource `<name>` |
| `Altinn:Cache:Valkey:Username` | The ACL user, `app`, from the `username` key of the Secret `<name>-cache-auth` |
| `Altinn:Cache:Valkey:Password` | The password, supplied through configuration (for example, a Kubernetes Secret or local user secrets) |
| `Altinn:Cache:Valkey:KeyVersion` | Optional. The key prefix version, from 1 to 65535 when enabled. Default `1` |
| `Altinn:Cache:DefaultLocalCacheExpiration` | Optional. Default lifetime in memory without a distributed tier. Default `00:05:00`, minimum `00:00:10` |
| `Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache` | Optional. Default lifetime in memory with a distributed tier. Default `00:00:30`, minimum `00:00:10` |

Password changes are not refreshed automatically; restart the pod after rotating credentials. The client does not use TLS; the service mesh encrypts the traffic.

Example Kubernetes configuration:

```yaml
env:
  - name: Altinn__Cache__Valkey__Host
    value: valkey-<name>.<namespace>.svc.cluster.local:6379
  - name: Altinn__Cache__Valkey__Username
    valueFrom:
      secretKeyRef:
        name: <name>-cache-auth
        key: username
        optional: true
  - name: Altinn__Cache__Valkey__Password
    valueFrom:
      secretKeyRef:
        name: <name>-cache-auth
        key: password
        optional: true
```

With `optional: true` a pod starts before the `Cache` resource exists; the credentials are then missing and the cache runs local only until the pod restarts.

### Keys, limits and staleness

- Keys in Valkey are prefixed `{serviceName}:v{keyVersion}:`. Bump `Altinn:Cache:Valkey:KeyVersion` when the shape of a cached type changes, so the new version never reads entries written by the old one.
- Staleness bound with the distributed tier on: `HybridCache` has no backplane, so a removed key or tag is gone from Valkey at once and from every instance's memory within the local lifetime, `Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache` (30 seconds). This bound applies to calls without entry options (5 minutes in Valkey, 30 seconds in memory). Custom entry options determine their own bound: setting only `Expiration` also sets the local lifetime. Entries with the distributed tier disabled are unaffected by removals on other instances. With the local tier only, values use `DefaultLocalCacheExpiration` (5 minutes by default), unless overridden by per-entry options.
- `MaximumPayloadBytes` is 1 MB. A larger value is kept in memory only and `HybridCache` logs an error on every write of it, so keep cached values small.
- Unreachable Valkey: requests succeed from the local tier and the factory. The client connects with 200 ms connect, sync and async timeouts, keeps retrying in the background, and fails fast while disconnected. Measured with the test container stopped: about 7 ms added per miss, dominated by the error logging. `HybridCache` logs an error for every failed Valkey operation while it is down.
- Values must be serializable by `HybridCache`: `string`, `byte[]`, or a type `System.Text.Json` can round-trip. Mutable types are deserialized on every hit; `sealed` types marked `[ImmutableObject(true)]` are reused from memory.

## Contributing

Contributions to Altinn.Authorization.ServiceDefaults are welcome! Feel free to open issues for bug reports, feature requests, or submit pull requests with improvements.

## License

Altinn.Authorization.ServiceDefaults is licensed under the [MIT License](LICENSE).
