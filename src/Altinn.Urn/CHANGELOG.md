# Changelog

## [2.6.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.6.0...Altinn.Urn-v2.6.1) (2025-01-09)


### Bug Fixes

* trigger rebuild ([#184](https://github.com/Altinn/altinn-authorization-utils/issues/184)) ([8d86886](https://github.com/Altinn/altinn-authorization-utils/commit/8d8688610f25408f44dd129bb40f31076c3fefa7))

## [2.6.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.5.1...Altinn.Urn-v2.6.0) (2024-12-06)


### Features

* upgrade to .NET 9 ([#166](https://github.com/Altinn/altinn-authorization-utils/issues/166)) ([867c940](https://github.com/Altinn/altinn-authorization-utils/commit/867c9400ac8fd9a37c71d0af6386fbb414523267))

## [2.5.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.5.0...Altinn.Urn-v2.5.1) (2024-10-18)


### Bug Fixes

* better json support for KeyValueUrn ([#141](https://github.com/Altinn/altinn-authorization-utils/issues/141)) ([b780a1b](https://github.com/Altinn/altinn-authorization-utils/commit/b780a1b24413b713d7ea09c9cbf43e7359621573))
* broken by dependency updates ([#142](https://github.com/Altinn/altinn-authorization-utils/issues/142)) ([e64d510](https://github.com/Altinn/altinn-authorization-utils/commit/e64d510c28c9989bd538a8f090b5563e60635b0b))

## [2.5.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.4.0...Altinn.Urn-v2.5.0) (2024-08-22)


### Features

* better type-value swagger objects ([#120](https://github.com/Altinn/altinn-authorization-utils/issues/120)) ([7071c9e](https://github.com/Altinn/altinn-authorization-utils/commit/7071c9e75350881ae0c43dbec56598c5a2dd2b58))

## [2.4.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.3.2...Altinn.Urn-v2.4.0) (2024-08-21)


### Features

* add base urn-json-type-value ([#114](https://github.com/Altinn/altinn-authorization-utils/issues/114)) ([a5b2a0e](https://github.com/Altinn/altinn-authorization-utils/commit/a5b2a0e4534bcac66a5ce88dcf6dd3222f37c356))

## [2.3.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.3.1...Altinn.Urn-v2.3.2) (2024-06-11)


### Bug Fixes

* create documentation files ([171dd71](https://github.com/Altinn/altinn-authorization-utils/commit/171dd7120ab70c8c5629224e6e7a2380ad827306))

## [2.3.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.3.0...Altinn.Urn-v2.3.1) (2024-05-14)


### Bug Fixes

* downgrade Microsoft.CodeAnalysis ([354f2b2](https://github.com/Altinn/altinn-authorization-utils/commit/354f2b268430a1c35dde5834d2de3968c53dfb8c))

## [2.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.2.2...Altinn.Urn-v2.3.0) (2024-04-26)


### Features

* make KeyValueUrnDictionary write all prefixes in the output ([bdfd099](https://github.com/Altinn/altinn-authorization-utils/commit/bdfd0999e4890619dc97a8e49c33f5896565eefc))

## [2.2.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.2.1...Altinn.Urn-v2.2.2) (2024-04-25)


### Bug Fixes

* correctly detect manual tryparse methods for urn variants ([b88096e](https://github.com/Altinn/altinn-authorization-utils/commit/b88096e95544ca49eb0cce11a187d18975e429e8))

## [2.2.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.2.0...Altinn.Urn-v2.2.1) (2024-04-23)


### Bug Fixes

* update sln files ([bd51b9e](https://github.com/Altinn/altinn-authorization-utils/commit/bd51b9e40d3644e0970fa6837809b1c21ad55b28))

## [2.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.1.1...Altinn.Urn-v2.2.0) (2024-04-23)


### Features

* add Altinn.Urn.Swashbuckle ([edeb6ee](https://github.com/Altinn/altinn-authorization-utils/commit/edeb6ee6f853203d3cfe0786ba44b80f29c2cc98))

## [2.1.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.1.0...Altinn.Urn-v2.1.1) (2024-04-22)


### Bug Fixes

* release issues ([ea3fbbf](https://github.com/Altinn/altinn-authorization-utils/commit/ea3fbbffc1fd6f0109a24397ec345f6c0c82b705))

## [2.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v2.0.0...Altinn.Urn-v2.1.0) (2024-04-22)


### Features

* more urn reflection capabilities ([6f27361](https://github.com/Altinn/altinn-authorization-utils/commit/6f273617c34f862d4992dd7836c01161ede3f0ad))

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.5...Altinn.Urn-v2.0.0) (2024-04-10)


### ⚠ BREAKING CHANGES

* New attribute names are `KeyValueUrn` on the record and `UrnKey` on the variants.
* `IUrn`, `RawUrn` and multiple other related types has been renamed.
* Urn keys that ends with colon are now disallowed, similar to them starting with "urn:". The colon suffix is added by the source generator.
* The previously public JsonConverters has been removed/made private, and instead to customize the serialization of an urn one should use the new wrapper types. Currently supported wrapper types are `UrnJsonString` and `UrnJsonTypeValue`.

### Features

* add new reflection capabilities on urns with new interfaces ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))
* add support for canonical keys ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))
* add urn wrappers for customizing serialization ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))
* redesign API ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))


### Bug Fixes

* reject urn keys that ends with colon ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))


### Code Refactoring

* rename attributes used by source generator ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))
* rename Urn to KeyValueUrn ([be02bf5](https://github.com/Altinn/altinn-authorization-utils/commit/be02bf5255c8b1a477d541e6c60c870067c7b835))

## [1.6.5](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.4...Altinn.Urn-v1.6.5) (2024-04-05)


### Bug Fixes

* add more doc comments ([a977734](https://github.com/Altinn/altinn-authorization-utils/commit/a97773479b810112faa5a2e7ad2a12e1359c4544))

## [1.6.4](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.3...Altinn.Urn-v1.6.4) (2024-04-05)


### Bug Fixes

* nuget licence and readme ([20ea20d](https://github.com/Altinn/altinn-authorization-utils/commit/20ea20db8d1de5db1e97372cd1b894da3640d35e))

## [1.6.3](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.2...Altinn.Urn-v1.6.3) (2024-04-05)


### Bug Fixes

* nuget deploy ([f715669](https://github.com/Altinn/altinn-authorization-utils/commit/f7156691475443e9b15cc5fe91d118839b1518a6))

## [1.6.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.1...Altinn.Urn-v1.6.2) (2024-04-05)


### Bug Fixes

* deploy ([d90cb99](https://github.com/Altinn/altinn-authorization-utils/commit/d90cb99f2fae90fd68bfa397790c847607760fe7))

## [1.6.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.6.0...Altinn.Urn-v1.6.1) (2024-04-05)


### Bug Fixes

* add more tests ([ecb1645](https://github.com/Altinn/altinn-authorization-utils/commit/ecb16453af5e6f93a86d7fcdee758ab780d164ad))
* json converter on type hierarchies ([ef49644](https://github.com/Altinn/altinn-authorization-utils/commit/ef49644bbbf8afe9c0bf61cdb3c6d9541e59f2cc))

## [1.6.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.5.3...Altinn.Urn-v1.6.0) (2024-04-05)


### Features

* add more json options and tests ([e0eefd1](https://github.com/Altinn/altinn-authorization-utils/commit/e0eefd14cbfa199ce861b48b8e55260a65fa50b8))

## [1.5.3](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.5.2...Altinn.Urn-v1.5.3) (2024-03-21)


### Bug Fixes

* release script ([4cea7fe](https://github.com/Altinn/altinn-authorization-utils/commit/4cea7fe256dbdd09417d55b13a4f06f2f2af9ba6))

## [1.5.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.5.1...Altinn.Urn-v1.5.2) (2024-03-21)


### Bug Fixes

* release permissions ([0e93e88](https://github.com/Altinn/altinn-authorization-utils/commit/0e93e8819d4fcb9492477fe6cd93af02ac041d16))

## [1.5.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.5.0...Altinn.Urn-v1.5.1) (2024-03-21)


### Bug Fixes

* source generator included in nuget package even when using --no-build ([f0e831e](https://github.com/Altinn/altinn-authorization-utils/commit/f0e831ec4bc6eba55f82c01d99dbf03e2f1897af))

## [1.5.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.9...Altinn.Urn-v1.5.0) (2024-03-21)


### Features

* add RawUrn ([14e4c90](https://github.com/Altinn/altinn-authorization-utils/commit/14e4c906099c507285b365afda6e872689f2fb6c))

## [1.4.9](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.8...Altinn.Urn-v1.4.9) (2024-03-20)


### Bug Fixes

* read file to memory ([5f4a2ec](https://github.com/Altinn/altinn-authorization-utils/commit/5f4a2ece447abbf584660ccfc13ca85c9432d02e))

## [1.4.8](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.7...Altinn.Urn-v1.4.8) (2024-03-20)


### Bug Fixes

* stat file ([b11d5fd](https://github.com/Altinn/altinn-authorization-utils/commit/b11d5fd6295a18a503f6084156e880970ae54268))

## [1.4.7](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.6...Altinn.Urn-v1.4.7) (2024-03-20)


### Bug Fixes

* ignore close error ([039ff2f](https://github.com/Altinn/altinn-authorization-utils/commit/039ff2fb5bb423c42b50b7d6d5a643c2e02eb3a8))

## [1.4.6](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.5...Altinn.Urn-v1.4.6) (2024-03-20)


### Bug Fixes

* try readable ([2bfc209](https://github.com/Altinn/altinn-authorization-utils/commit/2bfc20934c90547cfb950eb02d7e0d4c8bc67db5))

## [1.4.5](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.4...Altinn.Urn-v1.4.5) (2024-03-20)


### Bug Fixes

* install deno on ci ([016bf1c](https://github.com/Altinn/altinn-authorization-utils/commit/016bf1c60e7ce500d469726ccfb215630eb9639c))

## [1.4.4](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.3...Altinn.Urn-v1.4.4) (2024-03-20)


### Bug Fixes

* upload better ([153cd28](https://github.com/Altinn/altinn-authorization-utils/commit/153cd289e48dee6c53bb53863cca18b5c26ddaf9))

## [1.4.3](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.2...Altinn.Urn-v1.4.3) (2024-03-20)


### Bug Fixes

* github expressions ([7448a96](https://github.com/Altinn/altinn-authorization-utils/commit/7448a968109dbed03ac1345c6bde29eee41b4498))

## [1.4.2](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.1...Altinn.Urn-v1.4.2) (2024-03-20)


### Bug Fixes

* github artifact name again ([d4da93b](https://github.com/Altinn/altinn-authorization-utils/commit/d4da93b077de7ddb3d9f8aef55d08298ff28df2e))

## [1.4.1](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.4.0...Altinn.Urn-v1.4.1) (2024-03-20)


### Bug Fixes

* github artifact name ([7831f80](https://github.com/Altinn/altinn-authorization-utils/commit/7831f8060185a8c977a6a3433ad050f1c3821d67))

## [1.4.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.3.0...Altinn.Urn-v1.4.0) (2024-03-20)


### Features

* fix build 2? ([7fb7a28](https://github.com/Altinn/altinn-authorization-utils/commit/7fb7a28b95038c7faa10170f9e1925f4db84ebfd))
* simplify build ([58fb86a](https://github.com/Altinn/altinn-authorization-utils/commit/58fb86a278bc50123a960c71cf2aa2d74d307c84))

## [1.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.2.0...Altinn.Urn-v1.3.0) (2024-03-20)


### Features

* fix build? ([064d9ac](https://github.com/Altinn/altinn-authorization-utils/commit/064d9ac278ae1d3440f04060baba77df0fc3e76f))

## [1.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.1.0...Altinn.Urn-v1.2.0) (2024-03-20)


### Features

* improve release pipeline ([56cda9a](https://github.com/Altinn/altinn-authorization-utils/commit/56cda9a84aebe8533502472253aa5eac396fed0b))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Urn-v1.0.0...Altinn.Urn-v1.1.0) (2024-03-20)


### Features

* upload nuget packages to github release ([64b1997](https://github.com/Altinn/altinn-authorization-utils/commit/64b19979ef72603ac8e74f2fd55e31ab66f45676))

## 1.0.0 (2024-03-20)


### Features

* create Altinn.Urn ([1c7b2f3](https://github.com/Altinn/altinn-authorization-utils/commit/1c7b2f38b8795cbb325e8da1872dd6b00596ffa1))
