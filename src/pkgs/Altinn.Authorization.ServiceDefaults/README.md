# Altinn.Authorization.ServiceDefaults

Service defaults for altinn authorization services.

## Caching

`AddAltinnCache` registers Microsoft `HybridCache` for the service. The local in-memory tier is always on. Valkey (Redis protocol, provided by the platform's `dis-cache-operator`) is used as the distributed tier when the app enables it and the connection values are configured. Otherwise the cache is local only. One log line at startup says which mode is active and why.

```csharp
// Host built with AltinnHost or AddAltinnServiceDefaults:
builder.AddAltinnCache(enabled: distributedCacheFeatureFlag, keyVersion: 1);

// Plain IServiceCollection:
services.AddAltinnCache(configuration, serviceName: "authorization", enabled: distributedCacheFeatureFlag, keyVersion: 1);
```

`Altinn.Authorization.ServiceDefaults.Authorization` registers the same cache (local tier only) for the platform access token signing keys, so the app's `AddAltinnCache` call is what turns the distributed tier on.

### Configuration

| Key | Value |
| --- | --- |
| `Altinn:Cache:Host` | Valkey host and port: `valkey-<name>.<namespace>.svc.cluster.local:6379` for the `Cache` resource `<name>` |
| `Altinn:Cache:Username` | The ACL user, `app`, from the `username` key of the Secret `<name>-cache-auth` |
| `Altinn:Cache:PasswordFile` | Path of the mounted password file, `/var/run/secrets/cache/password` |
| `Altinn:Cache:PasswordRefreshInterval` | Optional. How often the password file is re-read. Default `00:01:00` |

The password is read from a file, never from an environment variable, because the operator rotates it (every 90 days, with the previous password valid for 7 more days) and an environment variable would hold the old one until the pod restarts. The file is read on connect and re-read every `PasswordRefreshInterval`, and the client uses the current value on every reconnect, so a rotated password takes effect without a restart. The client does not use TLS; the service mesh encrypts the traffic.

Manifest snippet, following the platform's reference consumer (`dis-console` in altinn-platform):

```yaml
env:
  - name: Altinn__Cache__Host
    value: valkey-<name>.<namespace>.svc.cluster.local:6379
  - name: Altinn__Cache__Username
    valueFrom:
      secretKeyRef:
        name: <name>-cache-auth
        key: username
        optional: true
  - name: Altinn__Cache__PasswordFile
    value: /var/run/secrets/cache/password
volumeMounts:
  - name: cache-auth
    mountPath: /var/run/secrets/cache
    readOnly: true
volumes:
  - name: cache-auth
    secret:
      secretName: <name>-cache-auth
      optional: true
      items:
        - key: password
          path: password
```

With `optional: true` a pod starts before the `Cache` resource exists; the password file is then missing and the cache runs local only until the pod restarts.

### Keys, limits and staleness

- Keys in Valkey are prefixed `{serviceName}:v{keyVersion}:`. Bump `keyVersion` when the shape of a cached type changes, so the new version never reads entries written by the old one.
- Default entry options with the distributed tier on: `Expiration` 5 minutes in Valkey, `LocalCacheExpiration` 30 seconds in memory. `HybridCache` has no backplane, so a `RemoveAsync` or `RemoveByTagAsync` clears Valkey at once while every other instance may keep serving the old value from memory for up to 30 seconds. That is the staleness bound; per-call `HybridCacheEntryOptions` override it. With the local tier only, both expirations are the `HybridCache` default of 5 minutes.
- `MaximumPayloadBytes` is 1 MB. A larger value is kept in memory only and `HybridCache` logs an error on every write of it, so keep cached values small.
- Unreachable Valkey: requests succeed from the local tier and the factory. The client connects with 200 ms connect, sync and async timeouts, keeps retrying in the background, and fails fast while disconnected. Measured with the test container stopped: about 7 ms added per miss, dominated by the error logging. `HybridCache` logs an error for every failed Valkey operation while it is down.
- Values must be serializable by `HybridCache`: `string`, `byte[]`, or a type `System.Text.Json` can round-trip. Mutable types are deserialized on every hit; `sealed` types marked `[ImmutableObject(true)]` are reused from memory.

## Contributing

Contributions to Altinn.Authorization.ServiceDefaults are welcome! Feel free to open issues for bug reports, feature requests, or submit pull requests with improvements.

## License

Altinn.Authorization.ServiceDefaults is licensed under the [MIT License](LICENSE).
