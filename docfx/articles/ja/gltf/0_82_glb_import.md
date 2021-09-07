
## GLB

`materialGenerator` 引き数(省略可能)を指定することで URP マテリアルを生成するようにカスタムできます。

```cs
var data = new GlbFileParser(path).Parse();
var loader = new UniGLTF.ImporterContext(data, materialGenerator: new GltfUrpMaterialDescriptorGenerator());
```
