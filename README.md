# AUModS

**Caution**:
Only the alpha version of AUModS is available so far.
You are **NOT** customers. You are participants.

AUModS is a plugin to play Madmate and EvilHacker on AmongUs.
A large part of its code base is shared with [TheOtherRoles](https://github.com/TheOtherRolesAU/TheOtherRoles) and its forks[[1]](https://github.com/yukinogatari/TheOtherRoles-GM)[[2]](https://github.com/haoming37/TheOtherRoles-GM-Haoming)[[3]](https://github.com/tomarai/TheOtherRoles).

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

## Features

* Roles
  - Sheriff
  - Madmate
  - EvilHacker
* Ability restriction
  - Admin
  - Cameras
  - Vitals
* Remove the limit on the number of tasks
* Random fix wiring locations
* Random locations to spawn on Airship

## Installation

The installation steps are essentially the same as [TheOtherRoles](https://github.com/TheOtherRolesAU/TheOtherRoles).

* Download [BepInEx 6.0.0-pre1 (UnityIl2CPP, x86)](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1)
* Extract BepInEx
* Download the latest release
* Place the downloaded dll in the plugin folder.

日本語版のインストール方法は[こちら](INSTALL_JP.md)

## How to contribute

It would be appreciated if you were interested in contributing to this project.

Please send your patches as pull requests.
All kinds of changes are always welcome.

* fix
  - bug
  - typo
  - grammar error
* new feature
* installation manual

## How to build

### Prerequisite

* [Docker](https://www.docker.com/)
* Your AmongUs binaries analyzed by BepInEx

### Example

```
# docker build -t aumodbuild .
# docker run --name aumodbuild aumodbuild
# docker cp aumodbuild:/source/src/AmongUs/BepInEx/plugins/SupplementalAUMOD.dll .
```

## Credits and Resources

* [BepInEx](https://github.com/BepInEx)
  - Used to hook to game functions
* [TheOtherRoles](https://github.com/TheOtherRolesAU/TheOtherRoles) and its forks[[1]](https://github.com/yukinogatari/TheOtherRoles-GM)[[2]](https://github.com/haoming37/TheOtherRoles-GM-Haoming)[[3]](https://github.com/tomarai/TheOtherRoles)
  - The basic design of this plugin came from TheOtherRoles
* [Among-Us-Sheriff-Mod](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod)
  - Idea for the Sheriff role came from Woodi-dev
* [Amongusの非公式MODで遊ぼう](https://au.libhalt.net/)
  - Idea for the Madmate role came from Shobosuke and libhalt
* [TheOtherRoles/tomarai](https://github.com/tomarai/TheOtherRoles/tree/dev-v3.4.x)
  - Idea for the EvilHacker role came from Kenshi Takada
