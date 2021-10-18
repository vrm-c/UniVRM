# v0.68～v0.78 (Unity-2018.4) 開発版

* VRM-1.0 作業中
* `.Net-3.5` が無くなって `coRoutine` を `Task` に置き換え
* glb/gltf に `ScriptedImporter` の導入
* `Standard` マテリアルの import/export や mipmapパたメーター などの改修

## ReleaseNote

```{toctree}
:glob:
:maxdepth: 1
   
v*
```

## Download


* [API](https://vrm-c.github.io/UniVRM/ja/)

| date  | version                                                                                                                           | 安定性・バグ                                    | 更新内容・備考                                                                                                                                                                |
|-------|-----------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|       | [0.68.0](http://github.com/vrm-c/UniVRM/releases/tag/v0.68.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/30?closed=1)  | [^material_import] [^import_bug]                | glb/gltf 座標軸オプション。ImporterContext API                                                                                                                                |
| 03/16 | [0.68.1](http://github.com/vrm-c/UniVRM/releases/tag/v0.68.1)                                                                     | [^import_bug]                                   |                                                                                                                                                                               |
| 03/17 | [0.68.2](http://github.com/vrm-c/UniVRM/releases/tag/v0.68.2)                                                                     |                                                 |                                                                                                                                                                               |
| 03/22 | [0.69.0](http://github.com/vrm-c/UniVRM/releases/tag/v0.69.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/31?closed=1)  | [^MetallicOcclusion][^EncodeToPng] [^NotUnique] | SmoothTexture 変換の修正[\#388](https://github.com/vrm-c/UniVRM/issues/388), Unity2020対応                                                                                    |
| 03/23 | [0.69.1](http://github.com/vrm-c/UniVRM/releases/tag/v0.69.1)                                                                     | [^MetallicOcclusion][^EncodeToPng]              |                                                                                                                                                                               |
| 03/31 | [0.70.0](http://github.com/vrm-c/UniVRM/releases/tag/v0.70.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/32?closed=1)  | [^MetallicOcclusion]                            | impl `WEIGHTS_0` not float4                                                                                                                                                   |
| 04/05 | [0.71.0](http://github.com/vrm-c/UniVRM/releases/tag/v0.71.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/33?closed=1)  | IOS build                                       |                                                                                                                                                                               |
| 04/13 | [0.72.0](http://github.com/vrm-c/UniVRM/releases/tag/v0.72.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/34?closed=1)  |                                                 | 頂点バッファを分割するオプション。T-Pose                                                                                                                                      |
| 04/22 | [0.73.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.73.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/35?closed=1) |                                                 | * [OtherPermissionUrl欄の修正](https://github.com/vrm-c/UniVRM/pull/897) * [正規化するときにBlendShapeを使うLookAtがExportされない](https://github.com/vrm-c/UniVRM/pull/894) |
| 05/12 | [0.74.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.74.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/36?closed=1) |                                                 | * [Runtime ロード後の　SpringBone　スケーリング挙動の修正](https://github.com/vrm-c/UniVRM/issues/922)                                                                        |
| 05/25 | [0.75.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.75.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/37?closed=1) |                                                 | 正規化時にLookAtのパラメーターが落ちてしまうのを修正                                                                                                                          |
| 06/08 | [0.76.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.76.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/38?closed=1) |                                                 | namespace MeshUtilityがUnityEditor.MeshUtility classと競合するのを修正                                                                                                        |
| 06/17 | [0.77.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.77.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/39?closed=1) |                                                 | [API更新](https://vrm-c.github.io/UniVRM/) https://github.com/vrm-c/UniVRM/issues/1022 https://github.com/vrm-c/UniVRM/issues/496                 |
| 06/23 | [0.78.0](https://github.com/vrm-c/UniVRM/releases/tag/v0.78.0) [Milestone](https://github.com/vrm-c/UniVRM/milestone/40?closed=1) |                                                 | https://github.com/vrm-c/UniVRM/pull/1049                                                                                                                                     |

[^material_import]: [\#786](https://github.com/vrm-c/UniVRM/issues/786) [\#788](https://github.com/vrm-c/UniVRM/issues/788)
[^import_bug]: [\#790](https://github.com/vrm-c/UniVRM/issues/790) [\#794](https://github.com/vrm-c/UniVRM/issues/794)
[^NotUnique]: [\#812](https://github.com/vrm-c/UniVRM/pull/812)
[^EncodeToPng]: [\#831](https://github.com/vrm-c/UniVRM/pull/831)
[^MetallicOcclusion]: [\#836](https://github.com/vrm-c/UniVRM/issues/836)
