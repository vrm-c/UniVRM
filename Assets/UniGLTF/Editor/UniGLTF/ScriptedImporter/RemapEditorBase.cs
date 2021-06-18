using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using VRMShaders;

namespace UniGLTF
{
    public abstract class RemapEditorBase
    {
        public struct SubAssetPair
        {
            public readonly SubAssetKey Key;
            public readonly UnityEngine.Object Object;

            public SubAssetPair(SubAssetKey key, UnityEngine.Object o)
            {
                Key = key;
                Object = o;
            }

            public void Deconstruct(out SubAssetKey key, out UnityEngine.Object value)
            {
                key = Key;
                value = Object;
            }
        }

        /// <summary>
        /// Remap 対象は、このエディタのライフサイクル中に不変
        /// 
        /// apply, clear 時には ScriptedImporter は reimport され、新しい引数で new される
        /// 
        /// </summary>
        SubAssetPair[] m_keyValues;

        protected RemapEditorBase(
            IEnumerable<SubAssetKey> keys,
            Dictionary<ScriptedImporter.SourceAssetIdentifier, UnityEngine.Object> externalObjectMap)
        {
            m_keyValues = keys.Select(x =>
            {
                var id = new ScriptedImporter.SourceAssetIdentifier(x.Type, x.Name);
                if (externalObjectMap.TryGetValue(id, out UnityEngine.Object value))
                {
                    return new SubAssetPair(x, value);
                }
                else
                {
                    return new SubAssetPair(x, null);
                }
            }).ToArray();
        }

        void RemapAndReload<T>(ScriptedImporter self, UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            self.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(self.assetPath);
            AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
        }

        protected void DrawRemapGUI<T>() where T : UnityEngine.Object
        {
            EditorGUI.indentLevel++;
            {
                foreach (var (key, value) in m_keyValues)
                {
                    if (!typeof(T).IsAssignableFrom(key.Type))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(key.Name))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(key.Name);
                    var asset = EditorGUILayout.ObjectField(value, typeof(T), true) as T;
                    if (asset != value)
                    {
                        // update
                        // RemapAndReload(importer, new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), asset);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
