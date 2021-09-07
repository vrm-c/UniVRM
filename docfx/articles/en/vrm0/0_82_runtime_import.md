* `Version 0.82.0`: Please use  `0.82.1` or later
* `Version 0.82.1~`

Material replacement for URP when Import is implemented .

Below step is needed.

1. Parse VRM, and Get VrmData.
1. Create VrmImporter, and load Unity hierarchy.
1. Use loaded instance(ShowMeshes)

# 1. Parse

`v0.82.1` VrmData is introduced.

```cs
var data = new UniGLTF.GlbFileParser(path).Parse();
VrmData vrm = new VRM.VRMData(data);
```

# 2. Load

`v0.82.1` VrmData is introduced.

```cs
var loader = new VRM.VRMImporterContext(vrm);
RuntimeGltfInstance instance = context.Load();
```

## Set `materialGenerator` argument for URP

Set `materialGenerator` Argument, material generation is could customized.
If omitted, default `materialGenerator` for `built-in` pipeline is used.

```cs
var loader = new VRM.VRMImporterContext(vrm, materialGenerator: new VRMUrpMaterialDescriptorGenerator(vrm.VrmExtension));
```

* `MToonShader for URP` has not been implemented yet, fallback to `UniUnlit`.

# 3. Instance
## ShowMeshes

`v0.77` ShowMeshes method is move to RuntimeGltfInstance from Importer.

```cs
instance.ShowMeshes();
```

## Destroy

`v0.77` When destroy, related assets(meshes, materials and textures etc...) is destroyed.

```cs
GameObject.Destroy(instance);
```
