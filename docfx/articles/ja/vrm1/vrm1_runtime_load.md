# RuntimeLoad

`VRM-1.0` は、 `VRM-0.x` もロードできます。
その場合、あたらしい meta への変換が発生し互換性の無い部分はすべて `不許可` の値になります。
このため、変換前のライセンスにアクセスする API を提供します。

サンプルの `Assets\VRM10\Samples\VRM10Viewer\VRM10ViewerUI.cs` も参照してください。

```cs
async Task<RuntimeGltfInstance> LoadAsync(string path)
{
    GltfData data = new GltfZipOrGlbFileParser(path).Parse();

    // doMigrate: true で旧バージョンの vrm をロードできます。
    if (Vrm10Data.TryParseOrMigrate(data, doMigrate: true, out Vrm10Data vrm))
    {
        // vrm
        using (var loader = new Vrm10Importer(vrm, 
        materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn)))
        {
            // migrate しても thumbnail は同じ
            var thumbnail  = await loader.LoadVrmThumbnailAsync();

            if (vrm.OldMeta != null)
            {
                // migrated from vrm-0.x. use OldMeta
                UpdateMeta(vrm.OldMeta, thumbnail);
            }
            else
            {
                // load vrm-1.0. use newMeta
                UpdateMeta(vrm.VrmExtension.Meta, thumbnail);
            }

            // モデルをロード
            RuntimeGltfInstance instance = await loader.LoadAsync();
            return instance;
        }
    }
    else{
        throw new Exception("not vrm");
    }
}
```
