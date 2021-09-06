## `Version 0.68~`

### API Changes

ImporterContext has been reworked.

* Loading processing has been divided into two steps: `Parse` and `Load`
    * `Parse` processing can be processed by other than the main thread
* The implementation of asynchronous loading function `ImporterContext.LoadAsync` has changed to `Task`
* The method of explicitly destroying `UnityEngine.Object` resources is now available. As such, resource leaks can be prevented
* The timing of calling `ImporterContext.Dispose` has been changed to when the loading process ends
    * Call `ImporterContext.DisposeOnGameObjectDestroyed` function (described below) before `ImporterContext.Dispose` function is called
    * In the previous versions, `ImporterContext.Dispose` is called when the generated VRM model is destroyed
* Added `ImporterContext.DisposeOnGameObjectDestroyed` function
    * The duty of destroying VRM resources (Texture, Material, Mesh, etc) has been transferred to GameObject
    * The resources (Texture, Material, Mesh, etc) will be destroyed when VRM's GameObject is destroyed


### Sample Codes (Synchronous Loading)

```cs
using UniGLTF;
using UnityEngine;
using VRM;

namespace YourNameSpace
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
            //    VRMImporterContext is the class for loading VRM
            using (var context = new VRMImporterContext(parser))
            {
                // 3. Call Load function to create a VRM GameObject
                context.Load();

                // 4. Enable UpdateWhenOffscreen
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                context.EnableUpdateWhenOffscreen();

                // 5. Display the model
                context.ShowMeshes();

                // 6. By calling this function, unity resources such as Texture, Material, Mesh, etc. used by VRM GameObject can be associated
                //    In other words, when the VRM GameObject is destroyed, resources (Texture, Material, Mesh, etc) that are actually used by the VRM GameObject can be destroyed
                context.DisposeOnGameObjectDestroyed();

                // 7. Return Root GameObject (VRM model)
                //    Root GameObject is where VRMMeta component is attached
                return context.Root;
            }
            // 8. When using statement ends, UnityEngine.Object resources held by VRMImporterContext are destroyed
            //    As mentioned in step 6, the resources associated with the VRM GameObject will not be destroyed
            //    The unused resources (not used by the VRM GameObject), i.e. unassigned textures, will be destroyed
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 9. Destroy the generated VRM GameObject
            //    If the VRM GameObject is destroyed, the associated unity resources (Texture, Material, Mesh, etc) will be destroyed, too
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```

### Sample Codes (Asynchronous Loading)

```cs
using System.IO;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRM;

namespace YourNameSpace
{
    public sealed class LoadVrmAsyncSample : MonoBehaviour
    {
        [SerializeField] private string _vrmFilePath;
        private GameObject _vrmGameObject;

        private async void Start()
        {
            _vrmGameObject = await LoadVrmAsync(_vrmFilePath);
        }

        private void OnDestroy()
        {
            DestroyVrm(_vrmGameObject);
        }

        private async Task<GameObject> LoadVrmAsync(string vrmFilePath)
        {
            // 1. Call GltfParser function (it has been separated from ImporterContext)
            //    We use GltfParser to obtain JSON information and binary data from the VRM file
            //    GltfParser can be run by other than the Unity's main thread
            var parser = new GltfParser();
            await Task.Run(() =>
            {
                var file = File.ReadAllBytes(vrmFilePath);
                parser.ParseGlb(file);
            });

            // 2. Initialize a new VRMImporterContext object and pass `parser` as an argument to it
            //    VRMImporterContext is the class for loading VRM
            using (var context = new VRMImporterContext(parser))
            {
                // 3. Call LoadAsync function to create a VRM GameObject
                //    For loading process it will take several frames
                await context.LoadAsync();

                // 4. Enable UpdateWhenOffscreen
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                context.EnableUpdateWhenOffscreen();

                // 5. Display the model
                context.ShowMeshes();

                // 6. By calling this function, unity resources such as Texture, Material, Mesh, etc. used by VRM GameObject can be associated
                //    In other words, when the VRM GameObject is destroyed, resources (Texture, Material, Mesh, etc) that are actually used by the VRM GameObject can be destroyed
                context.DisposeOnGameObjectDestroyed();

                // 7. Return Root GameObject (VRM model)
                //    Root GameObject is where VRMMeta component is attached
                return context.Root;
            }
            // 8. When using statement ends, UnityEngine.Object resources held by VRMImporterContext are destroyed
            //    As mentioned in step 6, the resources associated with the VRM GameObject will not be destroyed
            //    The unused resources (not used by the VRM GameObject), i.e. unassigned textures, will be destroyed
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 9. Destroy the generated VRM GameObject
            //    If the VRM GameObject is destroyed, the associated unity resources (Texture, Material, Mesh, etc) will be destroyed, too
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```
