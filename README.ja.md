
* [English](README.md)

# UniVRM

https://github.com/vrm-c/UniVRM

UniVRM は、VRMフォーマットを読み書きする Unity package です。

* [Unityのバージョン](https://vrm.dev/docs/univrm/install/unity_version/)
* [UniVRMのバージョン](https://vrm.dev/docs/univrm/install/univrm_version/)
* [UniVRMマニュアル](https://vrm.dev/docs/univrm/)

「UniVRM」は [VRM](https://vrm.dev/vrm_about/) の Unity 実装で、VRMモデルの作成・インポート・エクスポートができます。

「VRM」はVRアプリケーション向けの人型3Dアバター（3Dモデル）データを扱うためのファイルフォーマットです。
glTF2.0をベースとしており、誰でも自由に利用することができます。

インポートされたモデルは、

* [ヒューマノイド](https://vrm.dev/docs/univrm/humanoid/)
* [モデル情報](https://vrm.dev/docs/univrm/meta/)
* [マテリアル](https://vrm.dev/docs/univrm/shaders/)
* [表情制御](https://vrm.dev/docs/univrm/blendshape/)
* [視線制御](https://vrm.dev/docs/univrm/lookat/)
* [SpringBone](https://vrm.dev/docs/univrm/springbone/)
* [FirstPerson](https://vrm.dev/docs/univrm/firstperson/)

のデータがあります。

## License

* [MIT License](./LICENSE.txt)

## Installation

https://vrm.dev/docs/univrm/install/
### 安定版

トラブルの無さそうなバージョンをピックアップする予定です。
バージョン選択の参考にしてください。
github の右側にある下のようなリンクを辿ってください。

<img width=400 src=./right_latest.jpg>

1. ``UniVRM-0.xx.unitypackage``をダウンロードしてください。
1. ``UniVRM-0.xx.unitypackage``をUnityのプロジェクトにインポートしてください。

### 最新版

リリース時に preview の印を付けることにしました。

1. 本リポジトリの[リリースページ](https://github.com/vrm-c/UniVRM/releases)へ移動してください。
1. 最新の``UniVRM-0.xx.unitypackage``をダウンロードしてください。
1. ``UniVRM-0.xx.unitypackage``をUnityのプロジェクトにインポートしてください。
   
## Script Samples

* [Programming](https://vrm.dev/docs/univrm/programming/)
* [UniVRMサンプル](https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM.Samples)
* [UniVRMTest(テストとサンプル)](https://github.com/vrm-c/UniVRMTest)

## Contributing to UniVRM

以下コマンドでUniVRMリポジトリをクローンして、UnityでUniVRMフォルダを開きます。

```console
$ git clone https://github.com/vrm-c/UniVRM.git
$ cd UniVRM
# MToon などの取得
$ git submodule update --init --recursive
```

[コントリビューションガイド](https://github.com/vrm-c/UniVRM/wiki/コントリビューションガイド(ja))を参照してください。

## UPM

https://vrm.dev/docs/univrm/install/univrm_upm/

`v0.66.0` を任意のバージョンに読み替えてください。

```
{
  "dependencies": {
    // ...
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.66.0",
    "com.vrmc.unigltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.66.0",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.66.0",
    // ...
}
```
