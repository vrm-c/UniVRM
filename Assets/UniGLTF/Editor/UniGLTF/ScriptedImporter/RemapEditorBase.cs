using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using VRMShaders;

namespace UniGLTF
{
    public abstract class RemapEditorBase
    {
        void RemapAndReload<T>(ScriptedImporter self, UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            self.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(self.assetPath);
            AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
        }

        protected void DrawRemapGUI<T>(ScriptedImporter importer, IEnumerable<SubAssetKey> keys) where T : UnityEngine.Object
        {
            EditorGUI.indentLevel++;
            {
                var map = importer.GetExternalObjectMap();
                foreach (var key in keys)
                {
                    if (string.IsNullOrEmpty(key.Name))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(key.Name);
                    map.TryGetValue(new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), out UnityEngine.Object value);
                    var asset = EditorGUILayout.ObjectField(value, typeof(T), true) as T;
                    if (asset != value)
                    {
                        // update
                        RemapAndReload(importer, new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), asset);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
