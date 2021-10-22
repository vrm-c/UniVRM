# Import 時に生成される Material をカスタマイズする

`IMaterialDescriptorGenerator` を実装することで import 時に適用されるマテリアルを差し替えることができます。

## materialGenerator で URP 用のマテリアルをロードする

URP マテリアルを生成するようにカスタムする例です。

<https://github.com/vrm-c/UniVRM/issues/1214>

```csharp
async RuntimeGltfInstance Load(GltfData data)
{
    IMaterialDescriptorGenerator materialGenerator = new GltfUrpMaterialDescriptorGenerator();
    using(var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator)
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```
