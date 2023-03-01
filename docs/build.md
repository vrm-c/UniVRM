# アプリケーションのビルド

UniVRMを使うアプリケーションのビルドに関する注意事項

## ビルドに含めるシェーダー

`Project Settings = Graphics - Always Included Shaders` などに設定して、以下のシェーダーがビルドに含まれるようにしてください。

### Standard

* `Standard`

GLTF の PBR マテリアルが使用します。

```{admonition} Always Included Shaders
:class: warning
明示的に指定する必要があります
```

### Unlit

* `Assets\VRMShaders\GLTF\UniUnlit\Resources\UniGLTF\UniUnlit.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

### MToon

* `Assets\VRMShaders\VRM\MToon\MToon\Resources\Shaders\MToon.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

### ランタイム import/export 時のテクスチャー変換用のシェーダー

* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\NormalMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapImporter.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```
