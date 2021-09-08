This is `0.82.1` API.
If use `0.82.0`, please upgrade.

GLB load is below steps.

1. Parse `GLB` / `GLTF`, get a `GltfData`
2. Load `Unity Hierarchy` from the `GltfData`, get a `RuntimeGltfInstance`, dispose loader.
3. Use the `RuntimeGltfInstance`, destroy the `RuntimeGltfInstance`.

# 1. Parse

## Parse glb from file path

* same as `vrm`

```cs
GltfData Load(string path)
{
    return new GlbFileParser(path).Parse();
}
```

## Parse glb byte array

* same as `vrm`

```cs
GltfData Load(byte[] bytes)
{
    return new GlbBinaryParser(bytes, "LOAD_NAME").parse();
}
```

## Parse gltf from file path

```cs
GltfData Load(string path)
{
    return new GltfFileWithResourceFilesParser(path).Parse();
}
```

## Parse gltf in zip archive from file path

Zip archive that contain gltf and related files can parsed (experimental).

```cs
GltfData Load(string path)
{
    return new ZipArchivedGltfFileParser(path).Parse();
}
```

## Parse by file extension

See `SimpleViewer` sample.

```cs
GltfData Load(string path)
{
    // Detect type by file extension automatically
    return new AutoGltfFileParser(path).Parse();
}
```

# 2. Load

## sync

```cs
RuntimeGltfInstance Load(GltfData data)
{
    // Call ImporterContext.Dispose after load.
    // Automatically load dispose by using.
    using(var loader = new UniGLTF.ImporterContext(data)
    {
        var instance = loader.Load();
        return instance;
    }
}
```

## async

```cs
async RuntimeGltfInstance Load(GltfData data)
{
    // Call ImporterContext.Dispose after load.
    // Automatically load dispose by using.
    using(var loader = new UniGLTF.ImporterContext(data)
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```

## Load URP material by materialGenerator argument

You can load URP materials by setting the `materialGenerator` Argument (optional).

```cs
async RuntimeGltfInstance Load(GltfData data)
{
    var materialGenerator = new GltfUrpMaterialDescriptorGenerator();
    using(var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator)
    {
        var instance = await loader.LoadAsync();
        return instance;
    }
}
```

# 3. Use instance

```cs
// Setup for SkinnedMeshRenderer
instance.EnableUpdateWhenOffscreen();
// Show meshes when ready(invisible by default)
instance.ShowMeshes();
```

Destroy after use and discard related assets (textures, materials, meshes, etc.)

```cs
GameObject.Destroy(instance);
```
