using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class RemapEditorAnimation: RemapEditorBase
    {
        public void OnGUI(ScriptedImporter importer, GltfParser parser)
        {
            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is AnimationClip);
            using (new EditorGUI.DisabledScope(hasExternal))
            {
                if (GUILayout.Button("Extract Animation ..."))
                {
                    Extract(importer, parser);
                }
            }

            DrawRemapGUI<AnimationClip>(importer, AnimationImporterUtil.EnumerateSubAssetKeys(parser.GLTF));

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects(
                    typeof(UnityEngine.AnimationClip)
                    );
            }
        }

        static string GetAndCreateFolder(string assetPath, string suffix)
        {
            var path = $"{Path.GetDirectoryName(assetPath)}/{Path.GetFileNameWithoutExtension(assetPath)}{suffix}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static void Extract(ScriptedImporter importer, GltfParser parser)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }


            {
                var path = GetAndCreateFolder(importer.assetPath, ".Animations");
                foreach (var (key, asset) in importer.GetSubAssets<AnimationClip>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }
            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }       
    }
}
