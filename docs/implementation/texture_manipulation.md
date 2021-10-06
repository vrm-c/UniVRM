# Texture関連

## Unity の linear の テクスチャーの挙動について

### Runtime

new する場合は、 

```csharp
var texture = new Texture2D(width, height, format, mipChain, linear = true);
```

とする。

### Editor(Asset)

AssetFolder の png/jpg からloadする場合は、
事前に設定が必用。

```csharp
var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
textureImporter.sRGBTexture = false; // Linear
textureImporter.SaveAndReimport();

// linear でロードされます
var texture =AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
```

## Unity の NormalMap の挙動について

> MToon の NormalMap も同じ

### Runtime

new する場合は、 

```csharp
var texture = new Texture2D(width, height, format, mipChain, linear = true);
```

とする。
また、 `DXT5nm` という仕様で格納する必要があるので変換します。
y と w の２要素だけを使います。

```hlsl
half4 normal;
normal.x = 1.0;
normal.y = col.y;
normal.z = 1.0;
normal.w = col.x;
```

### Editor(Asset)

AssetFolder の png/jpg からloadする場合は、
ロードする前に設定が必用。

```csharp
var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
textureImporter.textureType = TextureImporterType.NormalMap; // normalMap
textureImporter.SaveAndReimport();

// DXT5nm でロードされます。
var texture =AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
```
