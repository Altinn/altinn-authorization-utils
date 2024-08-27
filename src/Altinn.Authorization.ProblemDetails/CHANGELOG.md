# Changelog

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
