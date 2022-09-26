# Folder 構成

`v0.104` 時点で UniVRM リポジトリの `Assets` 下には６つのフォルダがあります。

## Assets 下のフォルダ
### VRMShaders

UPM パッケージ `com.vrmc.vrmshaders` です。

`gltf`, `vrm-0.x`, `vrm-1.0` に関連する `Shader` や `Material` や `Texture` に関連する機能を集めています。
`UniUnlit` `MToon` `MToon-1.0` が含まれています。

個々のシェーダーについては、<https://vrm.dev/univrm/shaders/index.html> を参照してください。

### UniGLTF

UPM パッケージ `com.vrmc.gltf` です。
`com.vrmc.vrmshaders` に依存します。

`gltf`, `glb` の import / export 機能が有ります。
拡張子、 `.gltf`, `.glb` のファイルの Editor import 機能が有ります。

### VRM

UPM パッケージ `com.vrmc.univrm` です。
`com.vrmc.vrmshaders` と `com.vrmc.gltf` に依存します。

`vrm-0.x` の import / export 機能が有ります。
拡張子、 `.vrm` のファイルの Editor import 機能が有ります。
もし、`vrm-1.0` だった場合は import できません。

### VRM_Samples

`vrm-0.x` のサンプルシーンが有ります。
動作に `VRM`, `UniGLTF`, `VRMShaders` が必要です。

### VRM10

UPM パッケージ `com.vrmc.vrm` です。
`com.vrmc.vrmshaders` と `com.vrmc.gltf` に依存します。

`vrm-1.0` の import / export 機能が有ります。
拡張子、 `.vrm` のファイルの Editor import 機能が有ります。
もし、`vrm-0.x` だった場合は マイグレート が可能です。

### VRM10_Samples

`vrm-1.0` のサンプルシーンが有ります。
動作に `VRM10`, `UniGLTF`, `VRMShaders` が必要です。

## フォルダの組み合わせ

フォルダは、依存フォルダの条件を満たす範囲で組み合わせることができます。
すべてのフォルダがあると `vrm-0.x` と `vrm-1.0` の両方が動きます。
片方だけ使う場合は、不要な方を削除することができます。

### 例: VRM-0.X だけインストールする

* VRMShaders
* UniGLTF
* VRM

### 例: VRM-1.0 と VRM-0.X 両方 インストールする

* VRMShaders
* UniGLTF
* VRM
* VRM10

### 例: VRM-1.0 だけインストールする

* VRMShaders
* UniGLTF
* VRM10

### 例: UniGLTF だけインストールする

* VRMShaders
* UniGLTF

### 動かない例: UniGLTF だけインストールする

* UniGLTF

VRMShaders が無いので動きません。
