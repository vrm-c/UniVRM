using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
        private static ProfilerMarker s_MarkerCreatePrefab = new ProfilerMarker("Create Prefab");

#if !VRM_STOP_ASSETPOSTPROCESSOR
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                var unityPath = UnityPath.FromUnityPath(path);

                if (!unityPath.IsFileExists) {
                    continue;
                }

                if (unityPath.IsStreamingAsset)
                {
                    UniGLTFLogger.Log($"Skip StreamingAssets: {path}");
                    continue;
                }

                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".vrm")
                {
                    try
                    {
                        ImportVrm(unityPath);
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
                UniGLTFLogger.Warning($"out of Asset or writable Packages folder: {prefabPath}");
                return;
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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
                s_MarkerCreatePrefab.Begin();

                var map = texturePaths
                    .Select(x => x.LoadAsset<Texture>())
                    .ToDictionary(x => new SubAssetKey(x), x => x as UnityEngine.Object);

                try
                {
                    AssetDatabase.StartAssetEditing();

                    var settings = new ImporterContextSettings();

                    // 確実に Dispose するために敢えて再パースしている
                    using (var data = new GlbFileParser(vrmPath).Parse())
                    using (var context = new VRMImporterContext(new VRMData(data), externalObjectMap: map, settings: settings))
                    {
                        var editor = new VRMEditorImporterContext(context, prefabPath);
                        foreach (var textureInfo in context.TextureDescriptorGenerator.Get().GetEnumerable())
                        {
                            TextureImporterConfigurator.Configure(textureInfo, context.TextureFactory.ExternalTextures);
                        }
                        var loaded = context.Load();
                        editor.SaveAsAsset(loaded);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                s_MarkerCreatePrefab.End();

                sw.Stop();

                Debug.Log($"Import complete [importMs={sw.ElapsedMilliseconds}]");
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
