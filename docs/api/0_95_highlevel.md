# `v0.95` 簡単API

API が複雑化してきたので、よくある用途を簡単にできる高レベル API を追加しました。
`GltfUtility` と `VrmUtility` が中で using してくれるので await で呼び出すだけで使えます。

## GLTF

```{gitinclude} v0.95.0 Assets/UniGLTF/Runtime/UniGLTF/IO/GltfUtility.cs
:language: csharp
:linenos:
```

## VRM

```{gitinclude} v0.95.0 Assets/VRM/Runtime/IO/VrmUtility.cs
:language: csharp
:linenos:
```

## 使用例

```{gitinclude} v0.95.0 Assets/VRM_Samples/SimpleViewer/ViewerUI.cs
:language: csharp
:linenos:
:lines: 370-392
:emphasize-lines: 10, 20
:caption:
```
