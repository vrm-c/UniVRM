using System;
using System.Collections.Generic;
using System.IO;
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
                    ImportVrm(UnityPath.FromUnityPath(path));
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
            try
            {
                parser.ParseGlb(File.ReadAllBytes(path.FullPath));
            }
            catch (KeyNotFoundException)
            {
                // invalid VRM-0.X.
                // maybe VRM-1.0.do nothing
                return;
            }

            var prefabPath = path.Parent.Child(path.FileNameWithoutExtension + ".prefab");

            // save texture assets !
            LoadTextureAsyncFunc textureLoader = async (caller, textureIndex, used) =>
            {
                var gltfTexture = parser.GLTF.textures[textureIndex];
                var gltfImage = parser.GLTF.images[gltfTexture.source];
                var assetPath = prefabPath.Parent.Child(gltfImage.uri);
                var texture = await UniGLTF.AssetTextureLoader.LoadTaskAsync(assetPath, parser.GLTF, textureIndex);
                return new TextureLoadInfo(texture, used, false);
            };

            using (var context = new VRMImporterContext(parser, textureLoader))
            {
                var editor = new VRMEditorImporterContext(context);
                editor.ExtractImages(prefabPath);
            }

            EditorApplication.delayCall += () =>
            {
                //
                // after textures imported
                //
                using (var context = new VRMImporterContext(parser, textureLoader))
                {
                    var editor = new VRMEditorImporterContext(context);
                    context.Load();
                    editor.SaveAsAsset(prefabPath);
                }
            };
        }
    }
#endif
}
