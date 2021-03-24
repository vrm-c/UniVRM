using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
#if !VRM_STOP_ASSETPOSTPROCESSOR
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
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
                    catch (VRMImporterContext.NotVrm0Exception)
                    {
                        // is not vrm0
                    }
                }
            }
        }

        static void ImportVrm(UnityPath vrmPath)
        {
            if (!vrmPath.IsUnderAssetsFolder)
            {
                throw new Exception();
            }

            var parser = new GltfParser();
            parser.ParseGlb(File.ReadAllBytes(vrmPath.FullPath));

            var prefabPath = vrmPath.Parent.Child(vrmPath.FileNameWithoutExtension + ".prefab");

            Action<IEnumerable<UnityPath>> onCompleted = texturePaths =>
            {
                var map = texturePaths.Select(x =>
                {
                    var texture = x.LoadAsset<Texture2D>();
                    return (texture.name, texture: texture as UnityEngine.Object);
                }).ToArray();

                using (var context = new VRMImporterContext(parser, map))
                {
                    var editor = new VRMEditorImporterContext(context, prefabPath);
                    foreach (var textureInfo in new VRMTextureEnumerator(context.VRM).Enumerate(parser))
                    {
                        TextureImporterConfigurator.Configure(textureInfo, map.ToDictionary(x => x.name, x => x.texture as Texture2D));
                    }
                    context.Load();
                    editor.SaveAsAsset();
                }
            };

            // extract texture images
            using (var context = new VRMImporterContext(parser))
            {
                var editor = new VRMEditorImporterContext(context, prefabPath);
                editor.ConvertAndExtractImages(onCompleted);
            }
        }
    }
#endif
}
