# Changelog

## [4.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.8.0...Altinn.Authorization.TestUtils-v4.0.0) (2026-07-23)


### ⚠ BREAKING CHANGES

* removes net9 support
* update TestHost to use modern builders ([#612](https://github.com/Altinn/altinn-authorization-utils/issues/612))
* move HttpResponseShouldExtensions to Altinn.Authorization.TestUtils.Shouldly namespace ([#599](https://github.com/Altinn/altinn-authorization-utils/issues/599))
* Removes support for .NET 8 for all packages that still supported it

### Features

* update TestHost to use modern builders ([#612](https://github.com/Altinn/altinn-authorization-utils/issues/612)) ([9174ae8](https://github.com/Altinn/altinn-authorization-utils/commit/9174ae8ccf9babd98141f44caf90c19f0038c8ca))


### Bug Fixes

* move HttpResponseShouldExtensions to Altinn.Authorization.TestUtils.Shouldly namespace ([#599](https://github.com/Altinn/altinn-authorization-utils/issues/599)) ([899542b](https://github.com/Altinn/altinn-authorization-utils/commit/899542b1aa9fe279c8989498e326c4bc96c6046f))


### Miscellaneous Chores

* drop .NET 8 support ([#589](https://github.com/Altinn/altinn-authorization-utils/issues/589)) ([82dc3c7](https://github.com/Altinn/altinn-authorization-utils/commit/82dc3c70b3a878a90ef14576f348b7909b132d22))
* drop net9 support ([#628](https://github.com/Altinn/altinn-authorization-utils/issues/628)) ([843ad55](https://github.com/Altinn/altinn-authorization-utils/commit/843ad55d842eaeea662f4b7b370949a3b8ca69ab))

## [3.8.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.7.0...Altinn.Authorization.TestUtils-v3.8.0) (2026-05-31)


### Features

* add test options-monitor ([#571](https://github.com/Altinn/altinn-authorization-utils/issues/571)) ([d43a709](https://github.com/Altinn/altinn-authorization-utils/commit/d43a709c3c32d38f52934efd20e403aa8f45880b))

## [3.7.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.6.1...Altinn.Authorization.TestUtils-v3.7.0) (2026-04-08)


### Features

* add validation library ([#539](https://github.com/Altinn/altinn-authorization-utils/issues/539)) ([e223f30](https://github.com/Altinn/altinn-authorization-utils/commit/e223f30e36a03d0702eb421f7f7549a4da922277))

## [3.6.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.6.0...Altinn.Authorization.TestUtils-v3.6.1) (2026-02-17)


### Bug Fixes

* allow activity-collector to not collect everything ([#509](https://github.com/Altinn/altinn-authorization-utils/issues/509)) ([8732888](https://github.com/Altinn/altinn-authorization-utils/commit/8732888dcfffaea21f180ca2b52ec8e6c9e7613b))

## [3.6.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.5.0...Altinn.Authorization.TestUtils-v3.6.0) (2026-02-16)


### Features

* add activity collector ([#505](https://github.com/Altinn/altinn-authorization-utils/issues/505)) ([d590d6f](https://github.com/Altinn/altinn-authorization-utils/commit/d590d6f829002e8bbb72f539f9ffa4163eb00ca3))

## [3.5.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.4.0...Altinn.Authorization.TestUtils-v3.5.0) (2026-01-15)


### Features

* add respond overload that takes a status-code ([#470](https://github.com/Altinn/altinn-authorization-utils/issues/470)) ([1a01564](https://github.com/Altinn/altinn-authorization-utils/commit/1a01564c88f82c4e79ceb46fb5972fbed95f3c5e))


### Bug Fixes

* disable exception wrapping for response-delegates ([#469](https://github.com/Altinn/altinn-authorization-utils/issues/469)) ([75766f4](https://github.com/Altinn/altinn-authorization-utils/commit/75766f4f33470c50ff548cffad1cd7f952d8a831))

## [3.4.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.3.0...Altinn.Authorization.TestUtils-v3.4.0) (2025-11-24)


### Features

* add `TestMeterFactory` ([#429](https://github.com/Altinn/altinn-authorization-utils/issues/429)) ([33f2c7b](https://github.com/Altinn/altinn-authorization-utils/commit/33f2c7b6f23b119c6b23b25d70a83ba4e29eb833))

## [3.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.2.0...Altinn.Authorization.TestUtils-v3.3.0) (2025-11-19)


### Features

* add net10 package ([#423](https://github.com/Altinn/altinn-authorization-utils/issues/423)) ([2c43aff](https://github.com/Altinn/altinn-authorization-utils/commit/2c43aff3f244fc38c863a880022dee0263d50ab3))

## [3.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.1.0...Altinn.Authorization.TestUtils-v3.2.0) (2025-11-05)


### Features

* enable .NET 8 support ([#405](https://github.com/Altinn/altinn-authorization-utils/issues/405)) ([3974e28](https://github.com/Altinn/altinn-authorization-utils/commit/3974e286f0bb75369fb63d6bca0aed3530a0529d))
* enable authorization support for TestClient ([#406](https://github.com/Altinn/altinn-authorization-utils/issues/406)) ([a6d1165](https://github.com/Altinn/altinn-authorization-utils/commit/a6d11653436a28f6e04a53447afad41f04e744c7))

## [3.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.0.1...Altinn.Authorization.TestUtils-v3.1.0) (2025-10-18)


### Features

* aspnet testing utils ([#389](https://github.com/Altinn/altinn-authorization-utils/issues/389)) ([fa86525](https://github.com/Altinn/altinn-authorization-utils/commit/fa86525f1648afa684a4778c49b421435dc3df8b))

## [3.0.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v3.0.0...Altinn.Authorization.TestUtils-v3.0.1) (2025-09-30)


### Bug Fixes

* release sha handling ([#376](https://github.com/Altinn/altinn-authorization-utils/issues/376)) ([9899481](https://github.com/Altinn/altinn-authorization-utils/commit/9899481a22fe932f383c2b0b6a664218d2402ed8))

## [3.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v2.2.1...Altinn.Authorization.TestUtils-v3.0.0) (2025-09-24)


### ⚠ BREAKING CHANGES

* Modified the IFakeRequestHandler interface

### Features

* unmet expectations ([#370](https://github.com/Altinn/altinn-authorization-utils/issues/370)) ([e6ac66a](https://github.com/Altinn/altinn-authorization-utils/commit/e6ac66ad3e0a647fc73ca94cb5b694425320494d))

## [2.2.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v2.2.0...Altinn.Authorization.TestUtils-v2.2.1) (2025-09-24)


### Bug Fixes

* disable service-discovery ([#368](https://github.com/Altinn/altinn-authorization-utils/issues/368)) ([282ec5b](https://github.com/Altinn/altinn-authorization-utils/commit/282ec5be7b789c4df14c182201ae56586366001f))

## [2.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v2.1.0...Altinn.Authorization.TestUtils-v2.2.0) (2025-09-24)


### Features

* handle service-discovery ([#365](https://github.com/Altinn/altinn-authorization-utils/issues/365)) ([59ba0c8](https://github.com/Altinn/altinn-authorization-utils/commit/59ba0c844e2e09dda5d6489e6b455cf1d9e84b97))

## [2.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v2.0.0...Altinn.Authorization.TestUtils-v2.1.0) (2025-09-23)


### Features

* add authentication filter ([#360](https://github.com/Altinn/altinn-authorization-utils/issues/360)) ([714658f](https://github.com/Altinn/altinn-authorization-utils/commit/714658f33219678f5d45050a3e7629469881084b))

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Authorization.TestUtils-v1.0.0...Altinn.Authorization.TestUtils-v2.0.0) (2025-09-03)


### ⚠ BREAKING CHANGES

* Drops .NET 8 support

### Miscellaneous Chores

* drop .NET 8 support ([#338](https://github.com/Altinn/altinn-authorization-utils/issues/338)) ([9bf6ba9](https://github.com/Altinn/altinn-authorization-utils/commit/9bf6ba91a57f9520cedd9611cb4a15b130903df3))

## 1.0.0 (2025-06-24)


### Features

* add testutils packages ([#281](https://github.com/Altinn/altinn-authorization-utils/issues/281)) ([e6f4250](https://github.com/Altinn/altinn-authorization-utils/commit/e6f42507888f63a8549a6489dc589c1ab2de0463))
