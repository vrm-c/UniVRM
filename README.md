# UniVRM

ToDo: 英文バージョンとファイルを分ける

## License

* [MIT License](./LICENSE.txt)

## 0.47から MToon 以外の submodule をやめて一体化します

submodule の更新を、上流に反映するのがつらかったので。

## 内包するサブライブラリ

`Assets/VRM` のフォルダがサブライブラリに対応します。

### MToon

VRM 向け ToonShader です。
`submodule` 運用を続けます。

* https://github.com/Santarh/MToon

### UniJSON

JSON の読み書きなど。
importerは、まだ主に UnityEngine.JsonUtility を使っています。

依存

* .Net3.5(Unity とは独立して動作することができます)

### DepthFirstScheduler

`Task` の無い `.Net3.5` での `Task` 代用プラスアルファ。

処理を細かい単位に分けることでメインスレッドのブロックを最小限にすることを目的にしています。処理単位ごとに、`MainThread`, `Coroutine`, `スレッド実行` を調整します。
高速化や並列化は目的ではないです。
RuntimeImporter の非同期実行の部品です。

依存

* Unity-5.6

### UniUnlit

GLTFで記述できて、UnityのデフォルトのUnlitシェーダーで表現できないところをカバーするための Unlit 統合シェーダーです。

依存

* Unity-5.6

### UniHumanoid

UnityのHumanoidアバター作成ユーティリティです。
BVHローダーなども入っています。

依存

* Unity-5.6

### UniGLTF

拡張無しのGLTF読み書きを実装しています。
実際には、 MorphTarget の名前を extras に保存したり、 `KHR_material_unlit` 対応するなど少し拡張もあります。

依存

* UniJSON
* DepthFirstScheduler

### UniVRM

VRM拡張の読み書きと実行時に必要なコンポーネント群の実装など。

依存

* MToon
* UniGLTF
* UniHumanoid

## [VRM](https://dwango.github.io/vrm/)
###
"VRM" is a file format for using 3d humanoid avatars (and models) in VR applications.  
VRM is based on glTF2.0. And if you comply with the MIT license, you are free to use it.  
* The document is still only Japanese.

###
「VRM」はVRアプリケーション向けの人型3Dアバター（3Dモデル）データを扱うためのファイルフォーマットです。
glTF2.0をベースとしており、誰でも自由に利用することができます。

## [UniVRM](https://github.com/dwango/UniVRM)
###
"UniVRM" is an implementation for Unity of VRM. It can create, import and export for VRM models.
* Currently, support for Unity 5.6+  

###
「UniVRM」はVRMのUnity実装で、VRMモデルの作成・インポート・エクスポートができます。  
* Unity 5.6以降をサポートしています。

## Download
###
1. Go to the [releases page](https://github.com/dwango/UniVRM/releases)
1. Download latest ``UniVRM-0.xx.unitypackage``
1. Import to Unity project.

### 
1. 本リポジトリの[リリースページ](https://github.com/dwango/UniVRM/releases)へ移動してください。
1. 最新の``UniVRM-0.xx.unitypackage``をダウンロードしてください。
1. ``UniVRM-0.xx.unitypackage``をUnityのプロジェクトにインポートしてください。

## Script Samples

* [UniVRMTest](https://github.com/dwango/UniVRMTest)

