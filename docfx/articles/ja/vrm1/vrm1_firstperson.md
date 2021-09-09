VR向け FirstPerson 設定の初期化手順です。

1. Load する
2. VRM10Controller を取得する
3. `controller.Vrm.FirstPerson.SetupAsync` を呼び出す
4. `controller.Vrm.FirstPerson.SetupAsync` した結果新規に作成されたモデルを `RuntimeGltfInstance` に渡す
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
