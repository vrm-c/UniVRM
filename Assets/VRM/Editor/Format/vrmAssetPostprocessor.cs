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

        static void ImportVrm(UnityPath path)
        {
            if (!path.IsUnderAssetsFolder)
            {
                throw new Exception();
            }

            var parser = new GltfParser();
            parser.ParseGlb(File.ReadAllBytes(path.FullPath));

            var prefabPath = path.Parent.Child(path.FileNameWithoutExtension + ".prefab");

            Action<IEnumerable<string>> onCompleted = texturePaths =>
            {
                var map = texturePaths.Select(x =>
                {
                    var texture = AssetDatabase.LoadAssetAtPath(x, typeof(Texture2D));
                    return (texture.name, texture);
                }).ToArray();

                using (var context = new VRMImporterContext(parser, null, map))
                {
                    var editor = new VRMEditorImporterContext(context, prefabPath);
                    foreach (var textureInfo in parser.EnumerateTextures())
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
                editor.ConvertAndExtractImages(path, onCompleted);
            }
        }
    }
#endif
}
