# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [5.0.0](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/compare/v4.0.1...v5.0.0) (2024-10-03)


### ⚠ BREAKING CHANGES

* migration to v6 ([#24](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/24))
* Due to the structure change, all actions have to be added again. We're sorry for the inconvenience!
* This code will only run in Loupedeck 5 due to api changes

### Features

* add bus sel command ([a3c2c73](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/a3c2c734798c843581428c3c563e838c2f160f8f))
* add command name to the bottom of adjustment bars ([0f31573](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/0f3157300728b5e0d9fca39b67dc55dca1b30a6c))
* add multi-state possibility ([2d93a52](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/2d93a525750f74beec86984330adaaea91d1795b)), closes [#19](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/19)
* add raw commands and adjustments ([#27](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/27)) ([676b152](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/676b1525b5835a775441cab64e3a161dc6a07087))
* Load command support ([#13](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/13)) ([3387b47](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/3387b47eb37ca1ea36a6cc796feadf946f79a700))
* Loupedeck 5 Beta support ([72811bd](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/72811bd594e6d49bcf7c80f73013ee5b24bc7031))
* migration to v6 ([#24](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/24)) ([56b05fd](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/56b05fd36c396e538b7b496fc84528b8e04dd60a))
* min / max value limiting for encoders ([#8](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/8)) ([c8df8d8](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/c8df8d8d56dc11053ad03e8170c53b947ce7ade0))
* named adjustment commands with mute indicator ([f6ecb3c](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/f6ecb3ca0541a882b0843a8607e722f6631e7233)), closes [#12](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/12)
* only trigger updates for the specific values that change ([#6](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/6)) ([36d2476](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/36d2476dfcfa316b7afcd5ecb1d9c65b6311da40))
* Pan_x, Pan_y adjustment support ([#11](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/11)) ([64fea6e](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/64fea6e15790dc2ccbf83484325c092582649e8d))
* remoddeled volume bar ([f9a121e](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/f9a121e1b109839f9922c79f3f10f340c8c4bb84))
* the icons are reflecting now the color scheme from VoiceMeeter ([8246d01](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/8246d01050efcc10699bf25b75a56f7340462631))


### Bug Fixes

* better detection of voicemeeter version if setup exe was differently named ([#9](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/9)) ([cec3232](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/cec3232754a93103b258ff83a652d2c0373e5522))
* Fix a bug that could cause InvalidOperationException in foreach statements. ([#14](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/14)) ([2df0b8d](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/2df0b8dd1d6588e1fc4785daabd44d46d51f45ac))
* give every icon the right color ([b7b5719](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/b7b57195e3d731b78ce7938ed4abc4a55e1b9d85))
* give every icon the right number ([#10](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/10)) ([595544e](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/595544e9d04558737fc1c3168a7b6c9670df63d0))
* icons update again ([3d26e3a](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/3d26e3a2a6b75037d2bc7f564d357f5cc7352adf))
* raw command doesn't update from outside sources ([72cb55f](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/72cb55fa810c037d6c1ad2af794e6a4e15665c9a))
* strips/buses recognition breaks due to faulty characters ([3dec5f6](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/3dec5f6eb423cfb80c8b298c748613c82f36fbfb))
* volume bar just shows 'Gain' ([34b51cb](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/34b51cb58ed350a9a83552b9bdb4a89b367710bd))
* volume sliders should look normal again ([b9ae829](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/b9ae829d4cfeb33070dba785f8838226a1ee1b70))


### Code Refactoring

* update plugin to latest structure ([7ad5870](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/7ad58707d190da1296d3b46a4c38ae0a74fc3f98))

## [4.0.1](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/compare/v4.0.0...v4.0.1) (2024-10-03)


### Bug Fixes

* raw command doesn't update from outside sources ([72cb55f](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/72cb55fa810c037d6c1ad2af794e6a4e15665c9a))

## [4.0.0](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/compare/v3.0.0...v4.0.0) (2024-09-28)


### ⚠ BREAKING CHANGES

* migration to v6 ([#24](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/24))

### Features

* add raw commands and adjustments ([#27](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/27)) ([676b152](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/676b1525b5835a775441cab64e3a161dc6a07087))
* migration to v6 ([#24](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/24)) ([56b05fd](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/56b05fd36c396e538b7b496fc84528b8e04dd60a))

## v3.0.0 (2023-02-26)

### BREAKING CHANGE

- Due to the structure change, all actions have to be added again. We're sorry for
the inconvenience!

### Feat

- add multi-state possibility
- remoddeled volume bar
- add command name to the bottom of adjustment bars
- add bus sel command
- named adjustment commands with mute indicator
- Load command support (#13)
- Pan_x, Pan_y adjustment support (#11)

### Fix

- volume bar just shows 'Gain'
- strips/buses recognition breaks due to faulty characters
- Fix a bug that could cause InvalidOperationException in foreach statements. (#14)
- give every icon the right number (#10)

### Refactor

- replace fontSize with cmdSize
- update plugin to latest structure
- change volume bar text to be at the top
- replace .Drawings with SkiaSharp

## v2.0.1 (2022-05-16)

### Fix

- give every icon the right color
- icons update again

## [2.0.0](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/compare/v1.0.0...v2.0.0) (2022-04-04)


### ⚠ BREAKING CHANGES

* This code will only run in Loupedeck 5 due to api changes

### Features

* Loupedeck 5 Beta support ([72811bd](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/72811bd594e6d49bcf7c80f73013ee5b24bc7031))
* min / max value limiting for encoders ([#8](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/8)) ([c8df8d8](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/c8df8d8d56dc11053ad03e8170c53b947ce7ade0))
* only trigger updates for the specific values that change ([#6](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/6)) ([36d2476](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/36d2476dfcfa316b7afcd5ecb1d9c65b6311da40))
* the icons are reflecting now the color scheme from VoiceMeeter ([8246d01](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/8246d01050efcc10699bf25b75a56f7340462631))


### Bug Fixes

* better detection of voicemeeter version if setup exe was differently named ([#9](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/issues/9)) ([cec3232](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/cec3232754a93103b258ff83a652d2c0373e5522))
* volume sliders should look normal again ([b9ae829](https://github.com/XeroxDev/Loupedeck-plugin-VoiceMeeter/commit/b9ae829d4cfeb33070dba785f8838226a1ee1b70))

## 1.0.0 (2021-12-08)
