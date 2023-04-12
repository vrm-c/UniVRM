# `v0.76` ITextureDeserializer(テクスチャーローダー)

このインタフェースを使うと, glTF のテクスチャーをロードして Unity の Texture2D を作成する工程をカスタマイズできます。

## UnityTextureDeserializer

UniVRM が使用するデフォルトの実装は `UnityTextureDeserializer` です。

<https://github.com/vrm-c/UniVRM/blob/master/Assets/VRMShaders/GLTF/IO/Runtime/Texture/Importer/UnityTextureDeserializer.cs>

[ImageConversion.LoadImage](https://docs.unity3d.com/ja/2020.3/ScriptReference/ImageConversion.LoadImage.html) を使用して `png` や `jpeg` から Texture2D に変換します。

:::{admonition} ITextureDeserializer でパフォーマンスを改善
Texture2D の生成には複数のステップがあります。

- `png` や `jpeg` から `raw pixel` を取り出す
- 取り出した `raw pixel` を使って `Texture2D` を作成する

前者をスレッドに乗せて非同期処理にすることで
パフォーマンスを向上させる余地があります。
:::

:::{warning}
`ImageConversion.LoadImage` は `raw pixel` 取り出しと `Texture2D` 作成を一度に実行し、
その間メインスレッドをブロックします。
大きなテクスチャーや大量のテクスチャーをロードすると画面が固まりやすくなります。
:::

:::{admonition} ITextureDeserializer で対応する画像形式を拡張する
通常の `glTF` はテクスチャー画像として `png` と `jpeg` を格納できます。

もし、`EXT_texture_webp` `KHR_texture_basisu` などで別の形式の画像を利用する場合は
`ITextureDeserializer` で対応可能です。
:::

## 差し替え方法

`new UniGLTF.ImporterContext` の引き数で指定することができます。

```cs
        public ImporterContext(
            GltfData data,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈
            IMaterialDescriptorGenerator materialGenerator = null)
```

`new VRM.VRMImporterContext` の引き数で指定することができます。

```cs
        public VRMImporterContext(
            VRMData data,
            IReadOnlyDictionary<SubAssetKey, Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈
            IMaterialDescriptorGenerator materialGenerator = null,
            bool loadAnimation = false)
```

`new UniVRM10.Vrm10Importer` の引き数で指定することができます。

```cs
        public Vrm10Importer(
            Vrm10Data vrm,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null, // 👈
            IMaterialDescriptorGenerator materialGenerator = null,
            bool useControlRig = false
            )
```

## UnityAsyncImageLoader に置き換える例

ISSUE に使用例をいただきました。

<https://github.com/vrm-c/UniVRM/issues/1982>

[UnityAsyncImageLoader](https://github.com/Looooong/UnityAsyncImageLoader)を使ってパフォーマンスを向上できます。
