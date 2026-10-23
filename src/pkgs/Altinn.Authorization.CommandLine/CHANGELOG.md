# Changelog

## [1.3.0](https://github.com/Altinn/altinn-authorization-utils/compare/pkg/CommandLine-v1.2.0...pkg/CommandLine-v1.3.0) (2026-10-23)


### Features

* add configurable value parsing pipeline ([#784](https://github.com/Altinn/altinn-authorization-utils/issues/784)) ([832cca1](https://github.com/Altinn/altinn-authorization-utils/commit/832cca1c7c8114cdd70887c3c31c75eefa9bfcd5))
* add typed default value providers ([#787](https://github.com/Altinn/altinn-authorization-utils/issues/787)) ([7539abc](https://github.com/Altinn/altinn-authorization-utils/commit/7539abcd22d4ea2c6a004c5d3cca95d23a394cc1))
* customize argument arity and contextual colors in help ([#771](https://github.com/Altinn/altinn-authorization-utils/issues/771)) ([0b61457](https://github.com/Altinn/altinn-authorization-utils/commit/0b61457a04076cfe84942f22a9de3ce914635796))
* parse whitespace separated values as StringValues ([#765](https://github.com/Altinn/altinn-authorization-utils/issues/765)) ([65e7180](https://github.com/Altinn/altinn-authorization-utils/commit/65e71805bba963869d5694846b9084732d363ac7))
* pass display context to default value help callbacks ([#780](https://github.com/Altinn/altinn-authorization-utils/issues/780)) ([7f76b07](https://github.com/Altinn/altinn-authorization-utils/commit/7f76b07509a0f7c0862080b4310cbb17bf30f6a3))
* support environment variable option defaults ([#788](https://github.com/Altinn/altinn-authorization-utils/issues/788)) ([372f161](https://github.com/Altinn/altinn-authorization-utils/commit/372f1615f4f82774130e909b3d15cf706159290a))


### Bug Fixes

* deserialize pull_request_target as a pull request event ([#793](https://github.com/Altinn/altinn-authorization-utils/issues/793)) ([1d07e19](https://github.com/Altinn/altinn-authorization-utils/commit/1d07e195fb8f243b5431b238216232adb328958c))
* reject explicit empty StringValues input ([#783](https://github.com/Altinn/altinn-authorization-utils/issues/783)) ([b5fcae2](https://github.com/Altinn/altinn-authorization-utils/commit/b5fcae2bb69e09c54456e483cc3c3a6fd8c07e34))

## [1.2.0](https://github.com/Altinn/altinn-authorization-utils/compare/pkg/CommandLine-v1.1.0...pkg/CommandLine-v1.2.0) (2026-10-13)


### Features

* print exported env vars ([#756](https://github.com/Altinn/altinn-authorization-utils/issues/756)) ([0908ef4](https://github.com/Altinn/altinn-authorization-utils/commit/0908ef4b2e4dc94146ea054677ed3eff5bf57e90))

## [1.1.0](https://github.com/Altinn/altinn-authorization-utils/compare/pkg/CommandLine-v1.0.0...pkg/CommandLine-v1.1.0) (2026-10-08)


### Features

* capture process line output ([#727](https://github.com/Altinn/altinn-authorization-utils/issues/727)) ([8dd068c](https://github.com/Altinn/altinn-authorization-utils/commit/8dd068c3b7540fe605f4ae4f6446a685b64aca68))
* expose github context ([#735](https://github.com/Altinn/altinn-authorization-utils/issues/735)) ([ca97935](https://github.com/Altinn/altinn-authorization-utils/commit/ca97935375eacd89e1ae5bf9d9e37d7bd5a4533a))


### Bug Fixes

* render plain json verbatim ([#728](https://github.com/Altinn/altinn-authorization-utils/issues/728)) ([9ca139a](https://github.com/Altinn/altinn-authorization-utils/commit/9ca139a765da7395279e33c08c24429f5eb5c75b))

## 1.0.0 (2026-08-21)


### Features

* add argument preprocessor support ([#659](https://github.com/Altinn/altinn-authorization-utils/issues/659)) ([61e0466](https://github.com/Altinn/altinn-authorization-utils/commit/61e0466dff0f745adf1d5349e29df5548dd42b8c))
* add command execution helper ([#654](https://github.com/Altinn/altinn-authorization-utils/issues/654)) ([ccee390](https://github.com/Altinn/altinn-authorization-utils/commit/ccee39086a94f152b062a11de9c942c4ff6b78b0))
* add formatting handling to the cli ([#642](https://github.com/Altinn/altinn-authorization-utils/issues/642)) ([6b26269](https://github.com/Altinn/altinn-authorization-utils/commit/6b26269de06aa2c4b7447d3ac6ca743bd156563a))
* add invocation extensions ([#647](https://github.com/Altinn/altinn-authorization-utils/issues/647)) ([8126203](https://github.com/Altinn/altinn-authorization-utils/commit/81262036272b3e029299fde10782af6eac9d3a08))
* add json pretty printing ([#641](https://github.com/Altinn/altinn-authorization-utils/issues/641)) ([487ec3a](https://github.com/Altinn/altinn-authorization-utils/commit/487ec3ac3c585495c10683b19046d31fe8783af8))
* add simple console pretty-printer ([#640](https://github.com/Altinn/altinn-authorization-utils/issues/640)) ([30c9728](https://github.com/Altinn/altinn-authorization-utils/commit/30c97280112d1783c7251d2a4de4a971726a33a0))
* allow command-factory more binding patters ([#643](https://github.com/Altinn/altinn-authorization-utils/issues/643)) ([0153c3f](https://github.com/Altinn/altinn-authorization-utils/commit/0153c3fdcd192b89f73b89ed3b8a29d07d40c777))
* async state ([#661](https://github.com/Altinn/altinn-authorization-utils/issues/661)) ([b78928d](https://github.com/Altinn/altinn-authorization-utils/commit/b78928d0654d798cb2857628756cf6529e2b8a5e))
* command metadata ([#635](https://github.com/Altinn/altinn-authorization-utils/issues/635)) ([d88702e](https://github.com/Altinn/altinn-authorization-utils/commit/d88702e315cbac055fc7552287f117705c96c886))
* create CommandLine library ([#627](https://github.com/Altinn/altinn-authorization-utils/issues/627)) ([dff0d56](https://github.com/Altinn/altinn-authorization-utils/commit/dff0d56a7469f3f8c8225973a394e406956f4031))
* customize enum argument handling ([#664](https://github.com/Altinn/altinn-authorization-utils/issues/664)) ([467c90f](https://github.com/Altinn/altinn-authorization-utils/commit/467c90f2bd0ca749fee309c58ebe8f423d82e35e))
* execute-command result ([#662](https://github.com/Altinn/altinn-authorization-utils/issues/662)) ([06b62f0](https://github.com/Altinn/altinn-authorization-utils/commit/06b62f0249e6fc8abb5ce9a7508a02f480c16ce8))
* github actions utils ([#663](https://github.com/Altinn/altinn-authorization-utils/issues/663)) ([a732a70](https://github.com/Altinn/altinn-authorization-utils/commit/a732a706394cde3b8aca04495c0256b2eb9951e8))


### Bug Fixes

* ensure timeprovider is registered ([#670](https://github.com/Altinn/altinn-authorization-utils/issues/670)) ([8616fd9](https://github.com/Altinn/altinn-authorization-utils/commit/8616fd9d2413c33a5439e04ede1d5e5c0eb2299c))
