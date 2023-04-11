# `v0.76` ITextureDeserializer(テクスチャーローダー)

このインタフェースを使うとテクスチャーロードをカスタマイズできます。

## UnityTextureDeserializer

UniVRM の実装は、`UnityTextureDeserializer` です。

<https://github.com/vrm-c/UniVRM/blob/master/Assets/VRMShaders/GLTF/IO/Runtime/Texture/Importer/UnityTextureDeserializer.cs>

[ImageConversion.LoadImage](https://docs.unity3d.com/ja/2020.3/ScriptReference/ImageConversion.LoadImage.html) を使用して `png` や `jpeg` をロードできます。
通常の `glTF` はテクスチャー形式として `png` と `jpeg` を格納できます。

`ImageConversion.LoadImage` はメインスレッドをブロックします。
大きなテクスチャーや大量のテクスチャーをロードすると画面が固まることがあります。

`png` や `jpeg` から `raw pixel` を取得する部分をスレッドに乗せて非同期処理にすることで
パフォーマンスを向上させる余地があります。

## 差し替え方法

`new UniGLTF.ImporterContext` の引き数で指定することができます。

```cs
        public ImporterContext(
            GltfData data,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈 これ
            IMaterialDescriptorGenerator materialGenerator = null)
```

`new VRM.VRMImporterContext` の引き数で指定することができます。

```cs
        public VRMImporterContext(
            VRMData data,
            IReadOnlyDictionary<SubAssetKey, Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈 これ
            IMaterialDescriptorGenerator materialGenerator = null,
            bool loadAnimation = false)
```

`new UniVRM10.Vrm10Importer` の引き数で指定することができます。

```cs
        public Vrm10Importer(
            Vrm10Data vrm,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈 これ
            IMaterialDescriptorGenerator materialGenerator = null,
            bool useControlRig = false
            )
```

## UnityAsyncImageLoader に置き換える例

ISSUE に使用例をいただきました。

<https://github.com/vrm-c/UniVRM/issues/1982>

[UnityAsyncImageLoader](https://github.com/Looooong/UnityAsyncImageLoader)を使ってパフォーマンスを向上できます。
