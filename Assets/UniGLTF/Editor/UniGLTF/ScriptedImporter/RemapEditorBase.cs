using System;
using System.Collections.Generic;
using System.IO;
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
    public delegate Dictionary<SubAssetKey, UnityEngine.Object> EditorMapGetterFunc();
    public delegate void EditorMapSetterFunc(Dictionary<SubAssetKey, UnityEngine.Object> editorMap);

    public abstract class RemapEditorBase
    {
        static Dictionary<String, Type> s_typeMap;
        static Dictionary<String, Type> TypeMap
        {
            get
            {
                if (s_typeMap == null)
                {
                    s_typeMap = new Dictionary<string, Type>();
                }
                return s_typeMap;
            }
        }

        [Serializable]
        public struct SubAssetPair
        {
            [SerializeField]
            public String Type;

            [SerializeField]
            public String Name;

            public SubAssetKey Key => new SubAssetKey(TypeMap[Type], Name);
            public ScriptedImporter.SourceAssetIdentifier ID => new AssetImporter.SourceAssetIdentifier(TypeMap[Type], Name);

            [SerializeField]
            public UnityEngine.Object Object;

            public SubAssetPair(SubAssetKey key, UnityEngine.Object o)
            {
                Type = key.Type.ToString();
                TypeMap[Type] = key.Type;
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

        protected bool HasKeys => m_keys.Length > 0;

        EditorMapGetterFunc m_getter;
        EditorMapSetterFunc m_setter;

        protected RemapEditorBase(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter)
        {
            m_keys = keys.ToArray();
            m_getter = getter;
            m_setter = setter;
        }

        /// <summary>
        /// Extract 対象がすべて SubAsset に含まれるときに可能である
        /// </summary>
        /// <param name="importer"></param>
        /// <returns></returns>
        protected bool CanExtract(ScriptedImporter importer)
        {
            foreach (var (k, v) in importer.GetExternalObjectMap())
            {
                foreach (var key in m_keys)
                {
                    if (k.type != null && k.type.IsAssignableFrom(key.Type))
                    {
                        return false;
                    }
                }
            }

            return true;
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

        protected static string GetAndCreateFolder(string assetPath, string suffix)
        {
            var path = $"{Path.GetDirectoryName(assetPath)}/{Path.GetFileNameWithoutExtension(assetPath)}{suffix}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// subAsset を 指定された path に extract する
        /// </summary>
        /// <param name="subAsset"></param>
        /// <param name="destinationPath"></param>
        /// <param name="isForceUpdate"></param>
        public static UnityEngine.Object ExtractSubAsset(UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        {
            string assetPath = AssetDatabase.GetAssetPath(subAsset);

            // clone を path に出力(subAsset を出力するため)
            var clone = UnityEngine.Object.Instantiate(subAsset);
            AssetDatabase.CreateAsset(clone, destinationPath);

            // subAsset を clone に対して remap する
            var assetImporter = AssetImporter.GetAtPath(assetPath);
            assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(clone), clone);

            if (isForceUpdate)
            {
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return clone;
        }

        public static void ClearExternalObjects(ScriptedImporter importer, params Type[] targetTypes)
        {
            foreach (var targetType in targetTypes)
            {
                if (!typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                {
                    throw new NotImplementedException();
                }

                foreach (var (key, obj) in importer.GetExternalObjectMap())
                {
                    if (targetType.IsAssignableFrom(key.type))
                    {
                        importer.RemoveRemap(key);
                    }
                }
            }

            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
