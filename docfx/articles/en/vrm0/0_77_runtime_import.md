## `Version 0.77~`

[DisposeOnGameObjectDestroyed](https://github.com/vrm-c/UniVRM/issues/1018)

`ImporterContext` API has been modified.

The function `ImporterContext.DisposeOnGameObjectDestroyed` introduced in `Version 0.68` has been discarded.  
Instead, `ImporterContext.Load` and `RuntimeGltfInstance` are used in `v0.77`.

In addition, `ImporterContext`'s members have been moved to `RuntimeGltfInstance`:

* Root
* EnableUpdateWhenOffscreen()
* ShowMeshes()

To destroy the Importer, use `ImporterContext.Dispose` after `Load` is called.   
By destroying RuntimeGltfInstance, the resources associated with the RuntimeGltfInstance (Texture, Material, Mesh, etc) will be destroyed.

```cs
using UniGLTF;
using UnityEngine;

namespace VRM.Samples
{
    public sealed class LoadVrmSample : MonoBehaviour
    {
        [SerializeField] private string _vrmFilePath;
        private GameObject _vrmGameObject;

        private void Start()
        {
            _vrmGameObject = LoadVrm(_vrmFilePath);
        }

        private void OnDestroy()
        {
            DestroyVrm(_vrmGameObject);
        }

        private GameObject LoadVrm(string vrmFilePath)
        {
            // 1. Call GltfParser function (it has been separated from ImporterContext)
            //    We use GltfParser to obtain JSON information and binary data from the VRM file
            var parser = new GltfParser();
            parser.ParsePath(vrmFilePath);

            // 2. Initialize a new VRMImporterContext object and pass `parser` as an argument to it
            //    The class for loading VRM is VRMImporterContext
            using (var context = new VRMImporterContext(parser))
            {
                // 3. Call Load function to create a VRM GameObject
                RuntimeGltfInstance instance = context.Load(); // <- `v0.77`
                
                // For asynchronous loading, use the following line instead:
                // RuntimeGltfInstance instance = await context.LoadAsync();

                // 4. Enable UpdateWhenOffscreen
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                instance.EnableUpdateWhenOffscreen(); // This function has been moved from ImporterContext to RuntimeGltfInstance in `v0.77`

                // 5. Display the model
                instance.ShowMeshes(); // This function has been moved from ImporterContext to RuntimeGltfInstance `v0.77`

                // 6. Return Root GameObject (VRM model)
                //    Root GameObject is where VRMMeta component is attached
                return instance.Root; // <- changed from ImporterContext to RuntimeGltfInstance in `v0.77`
            }
            // 7. When using statement ends, UnityEngine.Object resources held by VRMImporterContext are destroyed
            //    In step 6, the resources associated with the VRM GameObject will not be destroyed
            //    The unused resources included in the glTF file (not used by the VRM GameObject), i.e. unassigned textures, will be destroyed
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 8. Destroy the generated VRM GameObject
            //    If the VRM GameObject is destroyed, the associated unity resources (Texture, Material, Mesh, etc) will also be destroyed
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```
