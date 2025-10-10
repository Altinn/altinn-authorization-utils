# Changelog

## [3.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v2.1.0...Altinn.Authorization.ModelUtils-v3.0.0) (2025-10-10)


### ⚠ BREAKING CHANGES

* Removes json options support from `FlagsEnumModel` and replaces it with `JsonNamingPolicy`.

### Bug Fixes

* `FlagsEnumModel` handles multiple enum cases with same value ([#385](https://github.com/Altinn/altinn-authorization-utils/issues/385)) ([dcef52e](https://github.com/Altinn/altinn-authorization-utils/commit/dcef52edad8ac13e9d1b8af8d90673f6419165f0))

## [2.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v2.0.0...Altinn.Authorization.ModelUtils-v2.1.0) (2025-09-10)


### Features

* non-exhaustive select ([#348](https://github.com/Altinn/altinn-authorization-utils/issues/348)) ([0c51c11](https://github.com/Altinn/altinn-authorization-utils/commit/0c51c11387c492c6ddf9e9c4c4e4fe7ffa50124f))

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.5.0...Altinn.Authorization.ModelUtils-v2.0.0) (2025-09-03)


### ⚠ BREAKING CHANGES

* Drops .NET 8 support

### Miscellaneous Chores

* drop .NET 8 support ([#338](https://github.com/Altinn/altinn-authorization-utils/issues/338)) ([9bf6ba9](https://github.com/Altinn/altinn-authorization-utils/commit/9bf6ba91a57f9520cedd9611cb4a15b130903df3))

## [1.5.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.4.3...Altinn.Authorization.ModelUtils-v1.5.0) (2025-07-03)


### Features

* add json-extension-data type ([#313](https://github.com/Altinn/altinn-authorization-utils/issues/313)) ([9496466](https://github.com/Altinn/altinn-authorization-utils/commit/9496466cf34c28f9f622e2abcf5024af829a7ad3))

## [1.4.3](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.4.2...Altinn.Authorization.ModelUtils-v1.4.3) (2025-07-03)


### Bug Fixes

* mark extension-types with additional properties in swagger ([#311](https://github.com/Altinn/altinn-authorization-utils/issues/311)) ([0ac0316](https://github.com/Altinn/altinn-authorization-utils/commit/0ac0316cd7bfb80c886e1d1923c3546e130acf3f))
* non-exhaustive was false for field-values ([#310](https://github.com/Altinn/altinn-authorization-utils/issues/310)) ([54de43e](https://github.com/Altinn/altinn-authorization-utils/commit/54de43e37653e68d8dc5d63269ac1200725896ca))

## [1.4.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.4.1...Altinn.Authorization.ModelUtils-v1.4.2) (2025-07-01)


### Bug Fixes

* partial JSON safety ([#306](https://github.com/Altinn/altinn-authorization-utils/issues/306)) ([d26906f](https://github.com/Altinn/altinn-authorization-utils/commit/d26906f3c835257e892b76e511e83542a0206f22))

## [1.4.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.4.0...Altinn.Authorization.ModelUtils-v1.4.1) (2025-06-30)


### Bug Fixes

* change schema suffix ([#303](https://github.com/Altinn/altinn-authorization-utils/issues/303)) ([b3b6301](https://github.com/Altinn/altinn-authorization-utils/commit/b3b6301fa8fdf34daa013342b9f184feed5b3e2f))

## [1.4.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.3.1...Altinn.Authorization.ModelUtils-v1.4.0) (2025-06-27)


### Features

* add extension data interface ([#300](https://github.com/Altinn/altinn-authorization-utils/issues/300)) ([110fa0e](https://github.com/Altinn/altinn-authorization-utils/commit/110fa0e5910a9184d169545ab0b98aa22b03e081))
* add support for custom json property names ([#301](https://github.com/Altinn/altinn-authorization-utils/issues/301)) ([e2c1f81](https://github.com/Altinn/altinn-authorization-utils/commit/e2c1f8165d99cd031eb004b848bec5d9bc7310f1))
* json extension data ([#299](https://github.com/Altinn/altinn-authorization-utils/issues/299)) ([d8aee6a](https://github.com/Altinn/altinn-authorization-utils/commit/d8aee6a8f1853d11c6041930708df8269f267f7c))

## [1.3.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.3.0...Altinn.Authorization.ModelUtils-v1.3.1) (2025-06-24)


### Bug Fixes

* exhaustive polymorphic swagger ([#295](https://github.com/Altinn/altinn-authorization-utils/issues/295)) ([d60c2b5](https://github.com/Altinn/altinn-authorization-utils/commit/d60c2b5c923435ad2d805ffb74ece8b17adae160))
* field-value-record null property bug ([#296](https://github.com/Altinn/altinn-authorization-utils/issues/296)) ([f9d74c4](https://github.com/Altinn/altinn-authorization-utils/commit/f9d74c49cf534718db4ba076e8b2554958cd8516))

## [1.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.2.0...Altinn.Authorization.ModelUtils-v1.3.0) (2025-06-24)


### Features

* add swagger support for polymorphic field-value-records ([#277](https://github.com/Altinn/altinn-authorization-utils/issues/277)) ([389bdfe](https://github.com/Altinn/altinn-authorization-utils/commit/389bdfef6f716861e9aefbf2e50b362a9708c30c))
* enum extensions ([#293](https://github.com/Altinn/altinn-authorization-utils/issues/293)) ([4bd4bfb](https://github.com/Altinn/altinn-authorization-utils/commit/4bd4bfbcda13184582aedcd723d985f5f3f8083f))
* flags enum model ([#294](https://github.com/Altinn/altinn-authorization-utils/issues/294)) ([eaad07f](https://github.com/Altinn/altinn-authorization-utils/commit/eaad07f0f47b5f3f79f1358e6c00383c4e83c072))
* polymorphic field-value-record ([#267](https://github.com/Altinn/altinn-authorization-utils/issues/267)) ([f0d8d60](https://github.com/Altinn/altinn-authorization-utils/commit/f0d8d600bae21f97e302e71252cc817a48e7bea7))

## [1.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.1.3...Altinn.Authorization.ModelUtils-v1.2.0) (2025-04-24)


### Features

* add `SelectFieldValue` for `FieldValue` ([#254](https://github.com/Altinn/altinn-authorization-utils/issues/254)) ([bf26efd](https://github.com/Altinn/altinn-authorization-utils/commit/bf26efd9c3e39f32265fe84b49cbce0aa07ace34))

## [1.1.3](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.1.2...Altinn.Authorization.ModelUtils-v1.1.3) (2025-04-22)


### Bug Fixes

* swashbuckle field-value-record handles unserializable types ([#250](https://github.com/Altinn/altinn-authorization-utils/issues/250)) ([901b36e](https://github.com/Altinn/altinn-authorization-utils/commit/901b36ec8d05eccbe1fe6d77c1652ac17b757726))

## [1.1.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.1.1...Altinn.Authorization.ModelUtils-v1.1.2) (2025-04-22)


### Bug Fixes

* register DI deps ([#248](https://github.com/Altinn/altinn-authorization-utils/issues/248)) ([cd92dbc](https://github.com/Altinn/altinn-authorization-utils/commit/cd92dbcbe89165cbdcbaaebc5e55c57031fa1aae))

## [1.1.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.1.0...Altinn.Authorization.ModelUtils-v1.1.1) (2025-04-22)


### Bug Fixes

* `ImmutableValueArray` json support ([#244](https://github.com/Altinn/altinn-authorization-utils/issues/244)) ([518b88b](https://github.com/Altinn/altinn-authorization-utils/commit/518b88bc78cfbdbef965a3741814502db87ae3c4))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ModelUtils-v1.0.0...Altinn.Authorization.ModelUtils-v1.1.0) (2025-04-21)


### Features

* add `ImmutableValueArray` ([#241](https://github.com/Altinn/altinn-authorization-utils/issues/241)) ([f38286d](https://github.com/Altinn/altinn-authorization-utils/commit/f38286daf817c36b68d0e6a6c04aa87e9654d9dd))

## 1.0.0 (2025-04-15)


### Features

* add `FieldValue<T>` ([#214](https://github.com/Altinn/altinn-authorization-utils/issues/214)) ([711f3b1](https://github.com/Altinn/altinn-authorization-utils/commit/711f3b11f80f088dd6b9ee003d3ff941ff2820ae))
* add `NonExhaustiveEnum<T>` ([#215](https://github.com/Altinn/altinn-authorization-utils/issues/215)) ([eca7941](https://github.com/Altinn/altinn-authorization-utils/commit/eca794191a057c9461e2907b8779459b70ce1e04))
* add model-utils project ([#205](https://github.com/Altinn/altinn-authorization-utils/issues/205)) ([d590c5c](https://github.com/Altinn/altinn-authorization-utils/commit/d590c5c7d47c08a2fd894577d5640609dc5e51d7))
* field-value-record models ([#239](https://github.com/Altinn/altinn-authorization-utils/issues/239)) ([e75e444](https://github.com/Altinn/altinn-authorization-utils/commit/e75e444ffeaaac922a84509624a306d456050f62))
* field-value-record swagger support ([#240](https://github.com/Altinn/altinn-authorization-utils/issues/240)) ([2984b5e](https://github.com/Altinn/altinn-authorization-utils/commit/2984b5e493dd10a915f21875e119d8afed22a7c7))
* non-exhaustive-enum-filter ([#225](https://github.com/Altinn/altinn-authorization-utils/issues/225)) ([0109545](https://github.com/Altinn/altinn-authorization-utils/commit/0109545a6a3352383c27156c12b5b2ecf6acd348))
