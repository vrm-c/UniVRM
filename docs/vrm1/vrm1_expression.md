# 🚧Expression

表情周りの操作方法。

> VRM-1.0 の `BlendShapeProxy` は、`Vrm10Instance.Expression` になります。

VRM-0.X の例

```csharp
void SetExpression(GameObject root)
{
    var controller = root.GetComponent<BlendShapeProxy>();
}
```

VRM-1.0 の例

```csharp
void SetExpression(GameObject root)
{
    var controller = root.GetComponent<Vrm10Instance>();
}
```
