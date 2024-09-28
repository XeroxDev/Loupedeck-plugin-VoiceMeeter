# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

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
