# v0.36

## Changed Storage Position of Texture Name

Conforming to the GLTF specification.

```json
json.images[i].extra.name
```

After the change

```json
json.images[i].name
```

## Changed Storage Position BlendShape Name

Conforming to the GLTF specification.

* `extras` is not allowed in target
* https://github.com/KhronosGroup/glTF/issues/1036#issuecomment-314078356 

```json
json.meshes[i].primitives[j].targets[k].extra.name
```

After the change 

```json
json.meshes[i].primitives[j].extras.targetNames[k]
```
