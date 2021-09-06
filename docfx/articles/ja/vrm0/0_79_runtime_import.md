## `Version 0.79`

`GltfParser` と `GltfData` の分割

```cs
var parser = new GltfParser();
parser.ParsePath(path);
```

```cs
GltfData data = new GlbFileParser(path).Parse();
```
