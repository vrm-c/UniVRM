* `Version 0.82.0`: Please use  `0.82.1` or later
* `Version 0.82.1~`

Material replacement for URP when Import is implemented .

Below step is needed.

1. Parse VRM, and Get `GltfData`.
1. Get `VRMData` from `GltfData`.
1. Load a `RuntimeGltfInstance` from `VRMData`.
1. Use the `RuntimeGltfInstance`

See `Assets\VRM\Samples\SimpleViewer\ViewerUI.cs`

# 1. Get `GltfData`

```cs
GltfData Load(string path)
{
    return new GlbFileParser(path).Parse();
}
```

See [GLB import](../gltf/0_82_glb_import.md).

# 2. Get `VRMData`

```cs
VRMData vrm = new VRMData(data);
```

# 3. Load

```cs
async RuntimeGltfInstance Load(VRMData vrm)
{
    // Dispose VRMImporterContext, after load
    using(var loader = new VRMImporterContext(vrm))
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```

## Set `materialGenerator` argument for URP(Experimental)

Set `materialGenerator` Argument, material generation is could customized.
If omitted, default `materialGenerator` for `built-in` pipeline is used.

```cs
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

* `MToonShader for URP` has not been implemented yet, fallback to `UniUnlit`.

# 4. Instance


```cs
// Setup for SkinnedMeshRenderer
instance.EnableUpdateWhenOffscreen();
// Show meshes when ready(invisible by default)
instance.ShowMeshes();
```

When destroy, related assets(meshes, materials and textures etc...) is destroyed.

```cs
// GameObject.Destroy(instance);

// Destroy GameObject not RuntimeGltfInstance
GameObject.Destroy(instance.gameObject);
```
