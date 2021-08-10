using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class RemapEditorAnimation : RemapEditorBase
    {
        public RemapEditorAnimation(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter) : base(keys, getter, setter)
        { }

        public void OnGUI(ScriptedImporter importer, GltfData data)
        {
            if (!HasKeys)
            {
                EditorGUILayout.HelpBox("no animations", MessageType.Info);
                return;
            }

            if (CanExtract(importer))
            {
                if (GUILayout.Button("Extract Animation ..."))
                {
                    Extract(importer, data);
                }
                EditorGUILayout.HelpBox("Extract subasset to external object and overwrite remap", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Clear extraction"))
                {
                    ClearExternalObjects(importer, typeof(AnimationClip));
                }
                EditorGUILayout.HelpBox("Clear remap. All remap use subAsset", MessageType.Info);
            }

            DrawRemapGUI<AnimationClip>(importer.GetExternalObjectMap());
        }

        public static void Extract(ScriptedImporter importer, GltfData data)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }

            var path = GetAndCreateFolder(importer.assetPath, ".Animations");
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(importer.assetPath))
            {
                if (asset is AnimationClip)
                {
                    ExtractSubAsset(asset, $"{path}/{asset.name}.asset", false);
                }
            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
