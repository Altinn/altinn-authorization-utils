# Changelog

## [2.0.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Cli-v1.3.0...Altinn.Cli-v2.0.0) (2025-07-31)


### ⚠ BREAKING CHANGES

* Modifies arguments to the CLI. Uses a new file structure for storing the JWKS. Old JWKS will not be recognized without manual intervention (rename the file from `.json` to `.jwks.json` or the key from `--priv` to `--jwks`).

### Features

* update jwks cli ([#321](https://github.com/Altinn/altinn-authorization-utils/issues/321)) ([b9d4a45](https://github.com/Altinn/altinn-authorization-utils/commit/b9d4a455822d7cc1c4d671525188a11ad71341bd))

## [1.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Cli-v1.2.0...Altinn.Cli-v1.3.0) (2025-03-06)


### Features

* npgsql export/import ([#187](https://github.com/Altinn/altinn-authorization-utils/issues/187)) ([e73450e](https://github.com/Altinn/altinn-authorization-utils/commit/e73450e291326ee38cc3bdb7463a434ddc236869))


### Bug Fixes

* cli --help text bug ([#206](https://github.com/Altinn/altinn-authorization-utils/issues/206)) ([86318e8](https://github.com/Altinn/altinn-authorization-utils/commit/86318e82007e7dc21b60ba2e4fd75457389449a5))

## [1.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Cli-v1.1.0...Altinn.Cli-v1.2.0) (2024-12-06)


### Features

* upgrade to .NET 9 ([#166](https://github.com/Altinn/altinn-authorization-utils/issues/166)) ([867c940](https://github.com/Altinn/altinn-authorization-utils/commit/867c9400ac8fd9a37c71d0af6386fbb414523267))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/Altinn.Cli-v1.0.0...Altinn.Cli-v1.1.0) (2024-07-25)


### Features

* add keyvault as valid jwks store ([58346b7](https://github.com/Altinn/altinn-authorization-utils/commit/58346b739fc1a7ffaea72bfeb825e9b794827f9e))

## 1.0.0 (2024-06-28)


### Features

* Create altinn-jwks Console App ([#72](https://github.com/Altinn/altinn-authorization-utils/issues/72)) ([b5d1dc0](https://github.com/Altinn/altinn-authorization-utils/commit/b5d1dc0cc55eedc1c6ff3fe97f6cd76ec9704b56))
