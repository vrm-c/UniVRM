## `Version 0.79`

Separate `GltfData` from `GltfParser`

```cs
var parser = new GltfParser();
parser.ParsePath(path);
```

```cs
GltfData data = new GlbFileParser(path).Parse();
```
