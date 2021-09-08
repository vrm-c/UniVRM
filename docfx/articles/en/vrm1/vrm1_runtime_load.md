# RuntimeLoad

`VRM-1.0` can load `VRM-0.x`.
In that case, incompatible migrated meta properties are not allowed.
Therefore, we provide an API that allows you to access the original meta before migration.

See `Assets\VRM10\Samples\VRM10Viewer\VRM10ViewerUI.cs`.

```cs
async Task<RuntimeGltfInstance> LoadAsync(string path)
{
    GltfData data = new GltfZipOrGlbFileParser(path).Parse();

    // The doMigrate argument allows you to load that older version of the vrm.
    if (Vrm10Data.TryParseOrMigrate(data, doMigrate: true, out Vrm10Data vrm))
    {
        // vrm
        using (var loader = new Vrm10Importer(vrm, 
        materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn)))
        {
            // It has been migrated, but it is the same thumbnail
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

            // load model
            RuntimeGltfInstance instance = await loader.LoadAsync();
            return instance;
        }
    }
    else{
        throw new Exception("not vrm");
    }
}
```
