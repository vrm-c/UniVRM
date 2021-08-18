
* [English](README.md)

# UniVRM

UniVRM は、[VRMフォーマット](https://vrm.dev/vrm_about/) を読み書きする Unity package です。
VRMモデルの作成・インポート・エクスポートができます。

「VRM」はVRアプリケーション向けの人型3Dアバター（3Dモデル）データを扱うためのファイルフォーマットです。
glTF2.0をベースとしており、誰でも自由に利用することができます。

## Manual

* [UniVRMマニュアル](https://vrm.dev/docs/univrm/)
* [サポートするUnityのバージョン](https://vrm.dev/docs/univrm/install/unity_version/)
* [UniVRMのバージョン](https://vrm.dev/docs/univrm/install/univrm_version/)

## License

* [MIT License](./LICENSE.txt)

## Install

バージョン `v0.80.0` から、`Unity-2019.4LTS` 以降をサポートしています。

https://vrm.dev/docs/univrm/install/

### UnityPackage

[Release](https://github.com/vrm-c/UniVRM/releases) から unitypackage をダウンロードしてください。

`v0.81.0` からパッケージの分割方法を変更しました。

|                             | UniGLTF_VRMShaders | VRM     | VRM-1.0β |
|-----------------------------|--------------------|---------|----------|
| VRMを使うとき               | install            | install |          |
| VRM1を使うとき              | install            |         | install  |
| VRMとVRM1両方使うとき       | instlal            | install | install  |
| GLTF か Shader だけ使うとき | install            |         |          |

### UPM(開発者向け)

https://vrm.dev/docs/univrm/install/univrm_upm/

`v0.80.0` を任意のバージョンに読み替えてください。

```json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.80.0",
    "com.vrmc.unigltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.80.0",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.80.0",
    // VRM-1.0βを試す場合
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.80.0",
}
```

#### サンプル

Unity の `PackageManager` Windows から Sample を install できます。

* `com.vrmc.univrm` と `com.vrmc.vrm` にサンプルがあります。

* [Programming](https://vrm.dev/en/docs/univrm/programming/)
* [UniVRM Samples](https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM.Samples)
* [UniVRMTest](https://github.com/vrm-c/UniVRMTest)
