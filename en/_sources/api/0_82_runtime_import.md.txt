# `v0.82.1` RuntimeImport VRMData

以下の手順で import します。

1. VRMをパースして、`GltfData` を得る。
1. `GltfData` から `VRMData` を得る。
1. `VrmData` から `RuntimeGltfInstance` をロードする。
1. `RuntimeGltfInstance` を使う。

サンプルの `Assets\VRM\Samples\SimpleViewer\ViewerUI.cs` も参照してください。

## 1. `GltfData` を得る

```csharp
GltfData Load(string path)
{
    return new GlbFileParser(path).Parse();
}
```

[GLB import](0_82_glb_import.md) も参照してください。

## 2. `VRMData` を得る

```csharp
VRMData vrm = new VRMData(data);
```

## 3. Load する

```csharp
async RuntimeGltfInstance Load(VRMData vrm)
{
    // 使用後に Dispose で VRMImporterContext を破棄してください。
    using(var loader = new VRMImporterContext(vrm))
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```

### URP 向けに `materialGenerator` を指定する(実験)

`materialGenerator` 引き数(省略可能)を指定することで URP マテリアルを生成するようにカスタムできます。
指定しない場合は `built-in` 向けのデフォルトが使用されます。

```csharp
async RuntimeGltfInstance Load(VRMData vrm)
{
    var materialGenerator = new VRMUrpMaterialDescriptorGenerator(vrm.VrmExtension);
    using(var loader = new VRM.VRMImporterContext(vrm, materialGenerator: materialGenerator))
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```

* まだ URP 向け MToonShader が作成されていないので、`UniUnlit` にフォールバックします。

## 4. Instance

```csharp
// SkinnedMeshRenderer に対する指示
instance.EnableUpdateWhenOffscreen();
// 準備ができたら表示する(デフォルトでは非表示)
instance.ShowMeshes();
```

使用後に以下のように破棄してください。関連する Asset(Texture, Material, Meshなど)も破棄されます。
```csharp
// GameObject.Destroy(instance);

// RuntimeGltfInstance ではなくて、その GameObject を Destroy します。
GameObject.Destroy(instance.gameObject);
```
