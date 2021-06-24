using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class RemapEditorAnimation : RemapEditorBase
    {
        public RemapEditorAnimation(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter) : base(keys, getter, setter)
        { }

        public void OnGUI(ScriptedImporter importer, GltfParser parser)
        {
            if (!HasKeys)
            {
                EditorGUILayout.HelpBox("no animations", MessageType.Info);
                return;
            }

            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is AnimationClip);
            using (new EditorGUI.DisabledScope(hasExternal))
            {
                if (GUILayout.Button("Extract Animation ..."))
                {
                    Extract(importer, parser);
                }
            }

            DrawRemapGUI<AnimationClip>(importer.GetExternalObjectMap());
        }

        public static void Extract(ScriptedImporter importer, GltfParser parser)
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
