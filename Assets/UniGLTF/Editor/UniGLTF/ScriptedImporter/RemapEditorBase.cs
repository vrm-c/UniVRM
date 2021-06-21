using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
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
        /// ExternalObjectMap は都度変わりうるのに注意。
        /// </summary>
        SubAssetKey[] m_keys;

        protected RemapEditorBase(IEnumerable<SubAssetKey> keys)
        {
            m_keys = keys.ToArray();
        }

        void RemapAndReload<T>(ScriptedImporter self, UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            self.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(self.assetPath);
            AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
        }

        protected void DrawRemapGUI<T>(Dictionary<ScriptedImporter.SourceAssetIdentifier, UnityEngine.Object> externalObjectMap) where T : UnityEngine.Object
        {
            EditorGUI.indentLevel++;
            {
                foreach (var key in m_keys)
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
                    externalObjectMap.TryGetValue(new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), out UnityEngine.Object value);
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
