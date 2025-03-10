# Changelog

## [2.11.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.10.0...Altinn.Authorization.ServiceDefaults-v2.11.0) (2025-03-10)


### Features

* add azure appconfiguration support ([#217](https://github.com/Altinn/altinn-authorization-utils/issues/217)) ([0daf8b2](https://github.com/Altinn/altinn-authorization-utils/commit/0daf8b22f15806782aa4c6b514862d318aa4e34b))

## [2.10.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.9.0...Altinn.Authorization.ServiceDefaults-v2.10.0) (2025-03-03)


### Features

* add support for configuring yuniql transaction mode ([#202](https://github.com/Altinn/altinn-authorization-utils/issues/202)) ([7db2baf](https://github.com/Altinn/altinn-authorization-utils/commit/7db2baf0719bc489822921b64aff7ffc25f2d903))
* npgsql export/import ([#187](https://github.com/Altinn/altinn-authorization-utils/issues/187)) ([e73450e](https://github.com/Altinn/altinn-authorization-utils/commit/e73450e291326ee38cc3bdb7463a434ddc236869))


### Bug Fixes

* make yuniql cancellable ([#199](https://github.com/Altinn/altinn-authorization-utils/issues/199)) ([ed7f3b1](https://github.com/Altinn/altinn-authorization-utils/commit/ed7f3b1b654642a5ef4173be92bdcfd8e4c2ffe4))

## [2.9.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.8.0...Altinn.Authorization.ServiceDefaults-v2.9.0) (2025-01-08)


### Features

* enable disabling pre-startup logging ([#181](https://github.com/Altinn/altinn-authorization-utils/issues/181)) ([64e776a](https://github.com/Altinn/altinn-authorization-utils/commit/64e776abd7a382b7fd1f9b123b6b923ee903f7b5))

## [2.8.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.7.0...Altinn.Authorization.ServiceDefaults-v2.8.0) (2024-12-06)


### Features

* upgrade to .NET 9 ([#166](https://github.com/Altinn/altinn-authorization-utils/issues/166)) ([867c940](https://github.com/Altinn/altinn-authorization-utils/commit/867c9400ac8fd9a37c71d0af6386fbb414523267))

## [2.7.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.6.1...Altinn.Authorization.ServiceDefaults-v2.7.0) (2024-11-21)


### Features

* support more Activity props ([#161](https://github.com/Altinn/altinn-authorization-utils/issues/161)) ([c7718d7](https://github.com/Altinn/altinn-authorization-utils/commit/c7718d7a7ac39c0f0552a11a53dff511d55040cd))

## [2.6.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.6.0...Altinn.Authorization.ServiceDefaults-v2.6.1) (2024-11-04)


### Bug Fixes

* obsolete yuniql migrations overload without serviceKey ([#148](https://github.com/Altinn/altinn-authorization-utils/issues/148)) ([647b9a2](https://github.com/Altinn/altinn-authorization-utils/commit/647b9a28a897b293c70db224e5eac7b0f6c959ec))

## [2.6.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.5.0...Altinn.Authorization.ServiceDefaults-v2.6.0) (2024-11-01)


### Features

* add support for multiple yuniql migrators ([#146](https://github.com/Altinn/altinn-authorization-utils/issues/146)) ([bfa873b](https://github.com/Altinn/altinn-authorization-utils/commit/bfa873b6ec8dcb3adedd09c5ca8f4b3ea1911a8c))

## [2.5.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.4.0...Altinn.Authorization.ServiceDefaults-v2.5.0) (2024-08-27)


### Features

* move AltinnActivityExtensions to new package ([#126](https://github.com/Altinn/altinn-authorization-utils/issues/126)) ([189c9ae](https://github.com/Altinn/altinn-authorization-utils/commit/189c9aeb0c9e1a22e6bb033c95d72e0ef5f53c00))

## [2.4.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.3.0...Altinn.Authorization.ServiceDefaults-v2.4.0) (2024-08-23)


### Features

* add activity extensions ([#124](https://github.com/Altinn/altinn-authorization-utils/issues/124)) ([df081df](https://github.com/Altinn/altinn-authorization-utils/commit/df081df9d118452dcb83fa3d38d8f47aefb5bb91))

## [2.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.2.2...Altinn.Authorization.ServiceDefaults-v2.3.0) (2024-08-21)


### Features

* add support for managed/workload identity ([#112](https://github.com/Altinn/altinn-authorization-utils/issues/112)) ([2d8ad2a](https://github.com/Altinn/altinn-authorization-utils/commit/2d8ad2a17bc3b92b08506fea14eacfd217879007))

## [2.2.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.2.1...Altinn.Authorization.ServiceDefaults-v2.2.2) (2024-07-26)


### Bug Fixes

* emit opentelemetry traces for yuniql package ([#102](https://github.com/Altinn/altinn-authorization-utils/issues/102)) ([b16157a](https://github.com/Altinn/altinn-authorization-utils/commit/b16157a60371e389df95b131f15eccf14cc0d462))

## [2.2.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.2.0...Altinn.Authorization.ServiceDefaults-v2.2.1) (2024-07-25)


### Bug Fixes

* wait for database server to be ready ([#100](https://github.com/Altinn/altinn-authorization-utils/issues/100)) ([81878be](https://github.com/Altinn/altinn-authorization-utils/commit/81878be0297a3e8b10a2a8d0900e77fe548a0b02))

## [2.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.1.1...Altinn.Authorization.ServiceDefaults-v2.2.0) (2024-07-19)


### Features

* add content-negotiation support for health report writer ([#92](https://github.com/Altinn/altinn-authorization-utils/issues/92)) ([8e3700e](https://github.com/Altinn/altinn-authorization-utils/commit/8e3700ed7baf730442c196d1a51b93c8e9ca9e3c))

## [2.1.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.1.0...Altinn.Authorization.ServiceDefaults-v2.1.1) (2024-07-16)


### Bug Fixes

* telemetry tags ([#89](https://github.com/Altinn/altinn-authorization-utils/issues/89)) ([9ca0a47](https://github.com/Altinn/altinn-authorization-utils/commit/9ca0a47a4069a8905c5d1aa025689963ba9abb91))

## [2.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v2.0.0...Altinn.Authorization.ServiceDefaults-v2.1.0) (2024-07-15)


### Features

* add health report information ([#86](https://github.com/Altinn/altinn-authorization-utils/issues/86)) ([8b21096](https://github.com/Altinn/altinn-authorization-utils/commit/8b2109645a583074c7dd956d464cbed629670cb4))
* add plain format support for health writer ([#88](https://github.com/Altinn/altinn-authorization-utils/issues/88)) ([0022dc6](https://github.com/Altinn/altinn-authorization-utils/commit/0022dc6b44efaec97d8d627f2634bf3593e602d2))

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v1.2.0...Altinn.Authorization.ServiceDefaults-v2.0.0) (2024-07-10)


### ⚠ BREAKING CHANGES

* The connection-string priorities have been modified in cases where multiple connection-strings are provided.

### Features

* add ApplicationInsights and KeyVault support ([#83](https://github.com/Altinn/altinn-authorization-utils/issues/83)) ([712b93e](https://github.com/Altinn/altinn-authorization-utils/commit/712b93e69d701af06f94bd4e202b18e9f4d9e843))
* add TestSeed library ([#77](https://github.com/Altinn/altinn-authorization-utils/issues/77)) ([0c6d662](https://github.com/Altinn/altinn-authorization-utils/commit/0c6d662b99ca31e137f1a9065881d541024eed07))
* modify connection-string priorities ([712b93e](https://github.com/Altinn/altinn-authorization-utils/commit/712b93e69d701af06f94bd4e202b18e9f4d9e843))

## [1.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v1.1.0...Altinn.Authorization.ServiceDefaults-v1.2.0) (2024-06-20)


### Features

* add AltinnClusterInfo support ([323ec92](https://github.com/Altinn/altinn-authorization-utils/commit/323ec926753290790e094f34103312e43b5635bb))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ServiceDefaults-v1.0.0...Altinn.Authorization.ServiceDefaults-v1.1.0) (2024-06-14)


### Features

* add ServiceDiscovery ([e1473bb](https://github.com/Altinn/altinn-authorization-utils/commit/e1473bb3a8648b26ae8731214d78ab626c81249b))

## 1.0.0 (2024-06-14)


### Features

* create Altinn.Authorization.ServiceDefaults ([beafffa](https://github.com/Altinn/altinn-authorization-utils/commit/beafffa60b08b9f212084f50ec4a3f5ec311b2f0))
* create Altinn.Authorization.ServiceDefaults.Npgsql ([f796761](https://github.com/Altinn/altinn-authorization-utils/commit/f796761b0de35730abdbf05da0025cc2d9834606))
* create ServiceDefaults.Npgsql.Yuniql ([#67](https://github.com/Altinn/altinn-authorization-utils/issues/67)) ([fb960dd](https://github.com/Altinn/altinn-authorization-utils/commit/fb960dd8143a2f07c9185d531492e977f36439e0))
* make Yuniql work with IFileProvider ([2a6f0a1](https://github.com/Altinn/altinn-authorization-utils/commit/2a6f0a1ab25963943ea164006cffe1122176187d))


### Bug Fixes

* create documentation files ([171dd71](https://github.com/Altinn/altinn-authorization-utils/commit/171dd7120ab70c8c5629224e6e7a2380ad827306))
* remove use of internal npgsql types ([fe7d0ab](https://github.com/Altinn/altinn-authorization-utils/commit/fe7d0ab8cd6ecb0b1a5e1c415922dd2dee0d6a18))
