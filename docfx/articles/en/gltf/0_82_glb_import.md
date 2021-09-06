
## GLB

Set `materialGenerator` argument, Material is replaced by URP version.

```cs
var data = new GlbFileParser(path).Parse();
var loader = new UniGLTF.ImporterContext(data, materialGenerator: new GltfUrpMaterialDescriptorGenerator());
```
