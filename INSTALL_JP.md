# インストール方法

* [BepInEx 6.0.0-pre1 (UnityIl2CPP, x86)](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1) をダウンロード
* ダウンロードした BepInEx の zip ファイルを展開
* AUModS の[最新版 dll](https://github.com/AUModS/AUModS/releases) をダウンロード
* 展開した BepInEx に `plugins` フォルダを作成する
* `plugins` フォルダの下にダウンロードした dll を配置する
* 展開して出てきたファイルを AmongUs の実行ファイル `Among Us.exe` があるフォルダにコピーする

BepInEx フォルダの下は以下のようになっていれば OK です (一例です)
一度実行すると `cache` などのフォルダが生成されます
```
BepInEx
 |
 +- config
 |
 +- core
 |
 +- patchers
 |
 +- plugins
    |
    +- SupplementalAUMod-<sha1 hash>.dll
```

`Among Us.exe` のあるフォルダは以下のようになっていれば OK です (一例です)

```
Among Us
 |
 +- Among Us_Data
 |
 +- Among Us.exe
 |
 +- baselib.dll
 |
 +- BepInEx
 |
 +- doorstop_config.ini
 |
 +- GameAssembly.dll
 |
 +- mono
 |
 +- msvcp140.dll
 |
 +- UnityCrashHandler32.exe
 |
 +- UnityPlayer.dll
 |
 +- vcruntime140.dll
 |
 +- winhttp.dll
```
