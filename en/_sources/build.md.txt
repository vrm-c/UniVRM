# アプリケーションのビルド

UniVRMを使うアプリケーションのビルドに関する注意事項

## ビルドに含めるシェーダー

`Project Settings = Graphics - Always Included Shaders` などに設定して、ビルドに含まれるようにしてください。

### Unlit

* `Assets\VRMShaders\GLTF\UniUnlit\Resources\UniGLTF\UniUnlit.shader`

### MToon

* `Assets\VRMShaders\VRM\MToon\MToon\Resources\Shaders\MToon.shader`

### ランタイム import/export 時のテクスチャー変換用のシェーダー

* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\NormalMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapImporter.shader`
