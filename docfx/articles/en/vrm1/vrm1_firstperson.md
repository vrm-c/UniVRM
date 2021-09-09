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

# Recommended camera layer settings

Suppose your scene has a camera that represents a head-mounted display and other cameras.

* FIRSTPERSON_ONLY_LAYER(The gameObject that specifies this layer disappears from other cameras)
* THIRDPERSON_ONLY_LAYER(The gameObject that specifies this layer disappears from the head-mounted display)

Stops drawing the avatar from the head-mounted display perspective and makes it visible to other cameras.

Example: Stop drawing the avatar's head so you can see the front

VRM reserves layers named `VRMFirstPersonOnly` and` VRMThirdPersonOnly`.

Please set `VRMFirstPersonOnly` and` VRMThirdPersonOnly` to `Project Settings`-`Tags and Layers`.

In the sample, we assign `9` and` 10` to each.

# Specify layer at initialization

It can be specified with an additional argument.

```cs
var created = await controller.Vrm.FirstPerson.SetupAsync(controller.gameObject, firstPersonOnlyLayer: 9, thirdPersonOnlyLayer: 10);
```
