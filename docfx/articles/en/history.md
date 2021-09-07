# Version

| version |                                       |
|---------|---------------------------------------|
| v0.82.1 | Material replacement when(for URP)    |
| v0.82.0 | Material replacement when(for URP)    |
| v0.77   | Reorganize ImporterContext            |
| v0.68   | Reorganize ImporterContext            |
| v0.63.2 | Update gltf extensions implementation |
| v0.56   | Update BlendShapeKey spec             |

[Rework BlendShapeKey's Interface](https://github.com/vrm-c/UniVRM/wiki/ReleaseNote-v0.56.0%28en%29#reworks-blendshapekeys-interface)

## v0.36

### Changed Storage Position of Texture Name

Conforming to the GLTF specification.

```json
json.images[i].extra.name
```

After the change

```json
json.images[i].name
```

### Changed Storage Position BlendShape Name

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
