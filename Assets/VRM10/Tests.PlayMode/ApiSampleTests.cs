using System;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;

namespace UniVRM10.Test
{
    public class ApiSampleTests
    {
        VrmLib.Model ReadModel(string path)
        {
            var bytes = File.ReadAllBytes(path);

            if (!VrmLib.Glb.TryParse(bytes, out VrmLib.Glb glb, out Exception ex))
            {
                Debug.LogError($"fail to Glb.TryParse: {path} => {ex}");
                return null;
            }

            var model = UniVRM10.VrmLoader.CreateVrmModel(path);
            return model;
        }

        ModelAsset BuildGameObject(VrmLib.Model model, bool showMesh)
        {
            var assets = RuntimeUnityBuilder.ToUnityAsset(model, showMesh);
            UniVRM10.ComponentBuilder.Build10(model, assets);
            return assets;
        }

        VrmLib.Model ToModel(UnityEngine.GameObject target)
        {
            var exporter = new UniVRM10.RuntimeVrmConverter();
            var model = exporter.ToModelFrom10(target);
            return model;
        }

        byte[] ToVrm10(VrmLib.Model model)
        {
            // 右手系に変換
            VrmLib.ModelExtensionsForCoordinates.ConvertCoordinate(model, VrmLib.Coordinates.Gltf);
            var bytes = UniVRM10.ModelExtensions.ToGlb(model);
            return bytes;
        }

        [UnityTest]
        public bool Sample()
        {
            var path = "Tests/Models/Alicia_vrm-1.00/AliciaSolid_vrm-1.00.vrm";
            Debug.Log($"load: {path}");

            // import
            var srcModel = ReadModel(path);
            Debug.Log(srcModel);

            var asset = BuildGameObject(srcModel, false);
            Debug.Log(asset);

            // renderer setting
            foreach (var render in asset.Renderers)
            {
                // show when RuntimeUnityBuilder.ToUnity(showMesh = false)
                render.enabled = true;
                // avoid culling
                if (render is SkinnedMeshRenderer skinned)
                {
                    skinned.updateWhenOffscreen = true;
                }
            }

            // export
            var dstModel = ToModel(asset.Root);
            Debug.Log(dstModel);

            var vrmBytes = ToVrm10(dstModel);
            Debug.Log($"export {vrmBytes.Length} bytes");

            return true;
        }
    }
}
