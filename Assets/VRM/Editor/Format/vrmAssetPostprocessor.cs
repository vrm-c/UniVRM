using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VRMShaders;


namespace VRM
{
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
#if !VRM_STOP_ASSETPOSTPROCESSOR
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (UnityPath.FromUnityPath(path).IsStreamingAsset)
                {
                    Debug.LogFormat("Skip StreamingAssets: {0}", path);
                    continue;
                }

                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".vrm")
                {
                    try
                    {
                        ImportVrm(UnityPath.FromUnityPath(path));
                    }
                    catch (NotVrm0Exception)
                    {
                        // is not vrm0
                    }
                }
            }
        }
#endif

        static void ImportVrm(UnityPath vrmPath)
        {
            if (!vrmPath.IsUnderWritableFolder)
            {
                throw new Exception();
            }

            var prefabPath = vrmPath.Parent.Child(vrmPath.FileNameWithoutExtension + ".prefab");

            ImportVrmAndCreatePrefab(vrmPath.FullPath, prefabPath);
        }

        public static void ImportVrmAndCreatePrefab(string vrmPath, UnityPath prefabPath)
        {
            if (!prefabPath.IsUnderWritableFolder)
            {
                Debug.LogWarningFormat("out of Asset or writable Packages folder: {0}", prefabPath);
                return;
            }

            /// <summary>
            /// これは EditorApplication.delayCall により呼び出される。
            /// 
            /// * delayCall には UnityEngine.Object 持ち越すことができない
            /// * vrmPath のみを持ち越す
            /// 
            /// </summary>
            /// <value></value>
            Action<IEnumerable<UnityPath>> onCompleted = texturePaths =>
            {
                var map = texturePaths
                    .Select(x => x.LoadAsset<Texture>())
                    .ToDictionary(x => new SubAssetKey(x), x => x as UnityEngine.Object);

                // 確実に Dispose するために敢えて再パースしている
                using (var data = new GlbFileParser(vrmPath).Parse())
                using (var context = new VRMImporterContext(new VRMData(data), externalObjectMap: map, loadAnimation: true))
                {
                    var editor = new VRMEditorImporterContext(context, prefabPath);
                    foreach (var textureInfo in context.TextureDescriptorGenerator.Get().GetEnumerable())
                    {
                        VRMShaders.TextureImporterConfigurator.Configure(textureInfo, context.TextureFactory.ExternalTextures);
                    }
                    var loaded = context.Load();
                    editor.SaveAsAsset(loaded);
                }

            };

            using (var data = new GlbFileParser(vrmPath).Parse())
            using (var context = new VRMImporterContext(new VRMData(data)))
            {
                var editor = new VRMEditorImporterContext(context, prefabPath);
                // extract texture images
                editor.ConvertAndExtractImages(onCompleted);
            }
        }
    }
}
