using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public delegate Dictionary<SubAssetKey, UnityEngine.Object> EditorMapGetterFunc();
    public delegate void EditorMapSetterFunc(Dictionary<SubAssetKey, UnityEngine.Object> editorMap);

    public abstract class RemapEditorBase
    {
        public static Dictionary<String, Type> s_typeMap = new Dictionary<string, Type>();

        [Serializable]
        public struct SubAssetPair
        {
            [SerializeField]
            public String Type;

            [SerializeField]
            public String Name;

            public SubAssetKey Key => new SubAssetKey(s_typeMap[Type], Name);
            public ScriptedImporter.SourceAssetIdentifier ID => new AssetImporter.SourceAssetIdentifier(s_typeMap[Type], Name);

            [SerializeField]
            public UnityEngine.Object Object;

            public SubAssetPair(SubAssetKey key, UnityEngine.Object o)
            {
                Type = key.Type.ToString();
                s_typeMap[Type] = key.Type;
                Name = key.Name;
                Object = o;
            }
        }

        /// <summary>
        /// Remap 対象は、このエディタのライフサイクル中に不変
        ///
        /// ExternalObjectMap は都度変わりうるのに注意。
        /// </summary>
        SubAssetKey[] m_keys;

        EditorMapGetterFunc m_getter;
        EditorMapSetterFunc m_setter;

        protected RemapEditorBase(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter)
        {
            m_keys = keys.ToArray();
            m_getter = getter;
            m_setter = setter;
        }

        protected void DrawRemapGUI<T>(
            Dictionary<ScriptedImporter.SourceAssetIdentifier, UnityEngine.Object> externalObjectMap
        ) where T : UnityEngine.Object
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

                    var editorMap = m_getter();
                    if (editorMap.TryGetValue(key, out UnityEngine.Object value))
                    {
                    }
                    else
                    {
                        externalObjectMap.TryGetValue(new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), out value);
                    }

                    var newValue = EditorGUILayout.ObjectField(value, typeof(T), true) as T;
                    if (newValue != value)
                    {
                        editorMap[key] = newValue;
                        m_setter(editorMap);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
