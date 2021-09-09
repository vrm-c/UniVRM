The VR First Person settings.

1. Load
2. Get VRM10Controller
3. Call `controller.Vrm.FirstPerson.SetupAsync`
4. Add the result of `controller.Vrm.FirstPerson.SetupAsync` to `RuntimeGltfInstance`
5. ShowMeshes

```cs
async Task<RuntimeGltfInstance> LoadAsync(string path)
{
    var data = new GlbFileParser(path).Parse();
    if (!Vrm10Data.TryParseOrMigrate(data, true, out Vrm10Data vrm))
    {
        throw new System.Exception("vrm parse error !");
    }
    using (var loader = new Vrm10Importer(vrm))
    {
        // 1.
        var instance = await loader.LoadAsync();

        // 2.
        var controller = instance.GetComponent<VRM10Controller>();

        // 3.
        var created = await controller.Vrm.FirstPerson.SetupAsync(controller.gameObject);

        // 4.
        instance.AddRenderers(created);

        // 5.
        instance.ShowMeshes();

        return instance;
    }
}
```
