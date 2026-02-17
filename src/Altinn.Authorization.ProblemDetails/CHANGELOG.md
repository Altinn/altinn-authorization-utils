# Changelog

## [5.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v4.0.1...Altinn.Authorization.ProblemDetails-v5.0.0) (2026-02-17)


### ⚠ BREAKING CHANGES

* `type` is now set to `urn:altinn:error:<CODE>`, instead of the default URI used by ASP.NET.
* `title` is set instead of `detail` from the problem- and validation-error- descriptors.
* `detail` is now optional and set on instances instead of descriptors.
* A new `statusDescription` field is added to `ProblemDetails` and auto-populated based on the (HTTP) status code.
* When running under `DEBUG`, `ProblemDescriptorFactory` and `ValidationErrorDescriptorFactory` will throw if a code is reused.
* `ValidationErrorBuilder` has been renamed to `ValidationProblemBuilder` to align with other names.

### Features

* add `MultipleProblemInstance.CreateBuilder`, `ValidationProblemInstance.CreateBuilder`, and `ProblemExtensionData.CreateBuilder` ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* add `statusDescription` field ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* add overloads to most converters/builders accepting an optional `detail` value. ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* add std validation-error "catch-all" ([#446](https://github.com/Altinn/altinn-authorization-utils/issues/446)) ([03f0421](https://github.com/Altinn/altinn-authorization-utils/commit/03f04211dfd416363998bc81516ab74ef5ddd889))
* add trace-id to problem-instance and details ([#445](https://github.com/Altinn/altinn-authorization-utils/issues/445)) ([8cf446b](https://github.com/Altinn/altinn-authorization-utils/commit/8cf446b012b82a2f39ddedb17e8ff93c12f1a293))
* allow setting `detail` on problem/error instances ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* detect duplicate code usage when running in `DEBUG` ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* renamed `ValidationErrorBuilder` to `ValidationProblemBuilder` ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* set `title` instead of `detail` from descriptors ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* set `type` property on `ProblemDetails` ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))
* set problem-details type based on error-code ([#482](https://github.com/Altinn/altinn-authorization-utils/issues/482)) ([4d3287c](https://github.com/Altinn/altinn-authorization-utils/commit/4d3287c2524c0143c47d1843142f00875d7a062a))


### Bug Fixes

* re-add .NET 8 support ([#496](https://github.com/Altinn/altinn-authorization-utils/issues/496)) ([e6e23e0](https://github.com/Altinn/altinn-authorization-utils/commit/e6e23e07797b6b0486bcd84a325f71b2439cfe6a))

## [4.0.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v4.0.0...Altinn.Authorization.ProblemDetails-v4.0.1) (2025-12-01)


### Bug Fixes

* log problem-details action-result ([#438](https://github.com/Altinn/altinn-authorization-utils/issues/438)) ([5631d04](https://github.com/Altinn/altinn-authorization-utils/commit/5631d0472bf5f3000ca706a78dbb4f4eb5aa54cc))

## [4.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.3.0...Altinn.Authorization.ProblemDetails-v4.0.0) (2025-09-03)


### ⚠ BREAKING CHANGES

* Drops .NET 8 support

### Features

* unit result type ([#345](https://github.com/Altinn/altinn-authorization-utils/issues/345)) ([4b8dc2e](https://github.com/Altinn/altinn-authorization-utils/commit/4b8dc2e537297bed084daaeef878bbc9f89bd8bd))


### Miscellaneous Chores

* drop .NET 8 support ([#338](https://github.com/Altinn/altinn-authorization-utils/issues/338)) ([9bf6ba9](https://github.com/Altinn/altinn-authorization-utils/commit/9bf6ba91a57f9520cedd9611cb4a15b130903df3))

## [3.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.2.2...Altinn.Authorization.ProblemDetails-v3.3.0) (2025-05-12)


### Features

* add `MultipleProblemDetails` ([#263](https://github.com/Altinn/altinn-authorization-utils/issues/263)) ([397ae08](https://github.com/Altinn/altinn-authorization-utils/commit/397ae083732c2732e933310847b65b2fd828c4a9))

## [3.2.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.2.1...Altinn.Authorization.ProblemDetails-v3.2.2) (2025-03-26)


### Bug Fixes

* include more details in `ProblemInstanceException` ([#228](https://github.com/Altinn/altinn-authorization-utils/issues/228)) ([5fe0f72](https://github.com/Altinn/altinn-authorization-utils/commit/5fe0f72b1f4c9505ec5facef39350dc7c63d24e3))

## [3.2.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.2.0...Altinn.Authorization.ProblemDetails-v3.2.1) (2024-12-19)


### Bug Fixes

* add ProblemInstanceException constructor overloads ([#175](https://github.com/Altinn/altinn-authorization-utils/issues/175)) ([95b8a0b](https://github.com/Altinn/altinn-authorization-utils/commit/95b8a0bbfd8a01d3ca72b4239fce63d040f30eb5))

## [3.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.1.0...Altinn.Authorization.ProblemDetails-v3.2.0) (2024-12-17)


### Features

* add `ProblemInstanceException` ([#172](https://github.com/Altinn/altinn-authorization-utils/issues/172)) ([7bed62f](https://github.com/Altinn/altinn-authorization-utils/commit/7bed62fdf844d534883286ba73c3095714d079b9))

## [3.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.0.1...Altinn.Authorization.ProblemDetails-v3.1.0) (2024-12-06)


### Features

* upgrade to .NET 9 ([#166](https://github.com/Altinn/altinn-authorization-utils/issues/166)) ([867c940](https://github.com/Altinn/altinn-authorization-utils/commit/867c9400ac8fd9a37c71d0af6386fbb414523267))

## [3.0.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v3.0.0...Altinn.Authorization.ProblemDetails-v3.0.1) (2024-08-27)


### Bug Fixes

* add documentation for problem-details ([#128](https://github.com/Altinn/altinn-authorization-utils/issues/128)) ([9eca254](https://github.com/Altinn/altinn-authorization-utils/commit/9eca2540b7d234327d2806fbadb15928000ebf2e))

## [3.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v2.0.0...Altinn.Authorization.ProblemDetails-v3.0.0) (2024-07-10)


### ⚠ BREAKING CHANGES

* The ValidationErrorBuilder class has been moved to the ProblemDetails.Abstractions assembly.
* The various Extensions properties in ProblemDetails.Abstractions have been changed from being a immutable array of key-value pairs, to a new ProblemExtensionData type.

### Features

* add ProblemExtensionData to ProblemDetails.Abstractions ([91c428a](https://github.com/Altinn/altinn-authorization-utils/commit/91c428adcd8341c9096b6b015fe65d118dcc55bf))
* add Result to ProblemDetails.Abstractions ([#81](https://github.com/Altinn/altinn-authorization-utils/issues/81)) ([91c428a](https://github.com/Altinn/altinn-authorization-utils/commit/91c428adcd8341c9096b6b015fe65d118dcc55bf))
* create ValidationProblemInstance in ProblemDetails.Abstractions. ([91c428a](https://github.com/Altinn/altinn-authorization-utils/commit/91c428adcd8341c9096b6b015fe65d118dcc55bf))


### Code Refactoring

* move ValidationErrorBuilder to ProblemDetails.Abstractions ([91c428a](https://github.com/Altinn/altinn-authorization-utils/commit/91c428adcd8341c9096b6b015fe65d118dcc55bf))

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v1.1.1...Altinn.Authorization.ProblemDetails-v2.0.0) (2024-06-11)


### ⚠ BREAKING CHANGES

* Multiple overloads for ProblemDetails and related types have been removed to avoid ambiguity. ValidationErrors has also been renamed to ValidationErrorBuilder and moved from abstractions to the ProblemDetails package.

### Bug Fixes

* Resolve ambigous method overloads ([4620f64](https://github.com/Altinn/altinn-authorization-utils/commit/4620f64555252fddca3c165269de33166eb35c9b))

## [1.1.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v1.1.0...Altinn.Authorization.ProblemDetails-v1.1.1) (2024-06-11)


### Bug Fixes

* doc-comment issue ([433fa54](https://github.com/Altinn/altinn-authorization-utils/commit/433fa548c4da6d356ed128a5c3216a3766ddf686))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.ProblemDetails-v1.0.0...Altinn.Authorization.ProblemDetails-v1.1.0) (2024-06-11)


### Features

* improve ProblemDetails ([9115fc2](https://github.com/Altinn/altinn-authorization-utils/commit/9115fc2994f61bc6d2ded09d874fb48cfdbe1b6a))


### Bug Fixes

* create documentation files ([171dd71](https://github.com/Altinn/altinn-authorization-utils/commit/171dd7120ab70c8c5629224e6e7a2380ad827306))

## 1.0.0 (2024-05-24)


### Features

* create Altinn.Authorization.ProblemDetails ([b544e57](https://github.com/Altinn/altinn-authorization-utils/commit/b544e57b6bec5d81c36bd693e73082c3ea11eec2))
* create Altinn.Authorization.ProblemDetails.Abstractions ([7ff8f2e](https://github.com/Altinn/altinn-authorization-utils/commit/7ff8f2e20dd563bf01c0e11456ee36122f9de539))
