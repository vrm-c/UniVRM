# RuntimeLoad

`Assets\VRM10\Samples\VRM10Viewer\VRM10ViewerUI.cs`

## RuntimeLoad

```csharp
Vrm10Instance vrm10Instance = await Vrm10.LoadPathAsync(path);
```

## Migration(VRM-0 to VRM-1)

`VRM-1.0` は、 `VRM-0.x` もロードできます。
その場合、あたらしい meta への変換が発生し互換性の無い部分はすべて `不許可` の値になります。
変換前のライセンスにアクセスできます。

サンプルの `Assets\VRM10\Samples\VRM10Viewer\VRM10ViewerUI.cs` も参照してください。

```csharp
async Task<Vrm10Instance> LoadAsync(string path)
{
    GltfData data = new AutoGltfFileParser(path).Parse();

    // doMigrate: true で旧バージョンの vrm をロードできます。
    if (Vrm10Data.TryParseOrMigrate(data, doMigrate: true, out Vrm10Data vrm))
    {
        // vrm
        using (var loader = new Vrm10Importer(vrm))
        {
            // migrate しても thumbnail は同じ
            var thumbnail  = await loader.LoadVrmThumbnailAsync();

            if (vrm.OriginalMetaBeforeMigration != null)
            {
                // migrated from vrm-0.x. use OriginalMetaBeforeMigration
                UpdateMeta(vrm.OriginalMetaBeforeMigration, thumbnail);
            }
            else
            {
                // load vrm-1.0. use newMeta
                UpdateMeta(vrm.VrmExtension.Meta, thumbnail);
            }

            // モデルをロード
            RuntimeGltfInstance instance = await loader.LoadAsync();
            return instance.GetComponent<Vrm10Instance>();
        }
    }
    else{
        throw new Exception("not vrm");
    }
}
```
