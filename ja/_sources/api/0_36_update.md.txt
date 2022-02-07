# `v0.36` GltfUpdate

## テクスチャ名の格納位置の修正

GLTFの仕様に準拠しました。

* extraはextrasの間違い
* imageはnameを持っていた

```js
json.images[i].extra.name
```

変更後

```js
json.images[i].name
```

## ブレンドシェイプ名の格納位置の修正

GLTFの仕様に準拠しました。

* extraはextrasの間違い
* targetにextrasは不許可
* https://github.com/KhronosGroup/glTF/issues/1036#issuecomment-314078356 

```js
json.meshes[i].primitives[j].targets[k].extra.name
```

変更後

```js
json.meshes[i].primitives[j].extras.targetNames[k]
```
