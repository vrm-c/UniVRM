#pragma warning disable 0414, 0649
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UniGLTF.MeshUtility;
using System.IO;

namespace VRM
{
    public class MeshIntegratorWizard : ScriptableWizard
    {
        [SerializeField]
        GameObject m_root;

        [Header("Validation")]
        [SerializeField]
        Material[] m_uniqueMaterials;

        [Serializable]
        struct MaterialKey
        {
            public string Shader;
            public KeyValuePair<string, object>[] Properties;

            public override bool Equals(object obj)
            {
                if (!(obj is MaterialKey))
                {
                    return base.Equals(obj);
                }

                var key = (MaterialKey)obj;

                return Shader == key.Shader
                    && Properties.SequenceEqual(key.Properties)
                    ;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Serializable]
        struct MaterialList
        {
            public Material[] Materials;

            public MaterialList(Material[] list)
            {
                Materials = list;
            }
        }
        [SerializeField]
        MaterialList[] m_duplicateMaterials;

        [Serializable]
        public class ExcludeItem
        {
            public Mesh Mesh;
            public bool Exclude;
        }

        [Header("Options")]
        [SerializeField]
        List<ExcludeItem> m_excludes = new List<ExcludeItem>();

        [Header("Result")]
        [SerializeField]
        MeshMap[] integrationResults;

        public static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<MeshIntegratorWizard>("MeshIntegratorWizard", "Integrate and close window", "Integrate");
        }

        private void OnEnable()
        {
            m_root = Selection.activeGameObject;
            OnValidate();
        }

        static object GetPropertyValue(Shader shader, int i, Material m)
        {
            var propType = ShaderUtil.GetPropertyType(shader, i);
            switch (propType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    return m.GetColor(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.Range:
                case ShaderUtil.ShaderPropertyType.Float:
                    return m.GetFloat(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.Vector:
                    return m.GetVector(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.TexEnv:
                    return m.GetTexture(ShaderUtil.GetPropertyName(shader, i));

                default:
                    throw new NotImplementedException(propType.ToString());
            }
        }

        static MaterialKey GetMaterialKey(Material m)
        {
            var key = new MaterialKey
            {
                Shader = m.shader.name,
            };

            key.Properties = Enumerable.Range(0, ShaderUtil.GetPropertyCount(m.shader))
                .Select(x => new KeyValuePair<string, object>(
                    ShaderUtil.GetPropertyName(m.shader, x),
                    GetPropertyValue(m.shader, x, m))
                    )
                .OrderBy(x => x.Key)
                .ToArray()
                    ;

            return key;
        }

        void OnValidate()
        {
            if (m_root == null
            || !PrefabUtility.IsPartOfAnyPrefab(m_root) || m_root.transform.parent != null)
            {
                Debug.LogWarning("Invalidate");
                m_uniqueMaterials = new Material[] { };
                m_duplicateMaterials = new MaterialList[] { };
                m_excludes.Clear();
                return;
            }

            Debug.Log("OnValidate");
            m_uniqueMaterials = MeshIntegratorUtility.EnumerateSkinnedMeshRenderer(m_root.transform, false)
                .SelectMany(x => x.sharedMaterials)
                .Distinct()
                .ToArray();

            m_duplicateMaterials = m_uniqueMaterials
                .GroupBy(x => GetMaterialKey(x), x => x)
                .Select(x => new MaterialList(x.ToArray()))
                .Where(x => x.Materials.Length > 1)
                .ToArray()
                ;

            var exclude_map = new Dictionary<Mesh, ExcludeItem>();
            var excludes = new List<ExcludeItem>();
            foreach (var x in m_root.GetComponentsInChildren<Renderer>())
            {
                var mesh = x.GetMesh();
                if (mesh == null)
                {
                    continue;
                }
                var item = new ExcludeItem { Mesh = mesh };
                excludes.Add(item);
                exclude_map[mesh] = item;
            }
            foreach (var x in m_excludes)
            {
                if (exclude_map.TryGetValue(x.Mesh, out ExcludeItem item))
                {
                    // update
                    item.Exclude = x.Exclude;
                }
            }
            m_excludes.Clear();
            foreach (var kv in exclude_map)
            {
                m_excludes.Add(kv.Value);
            }
        }

        void OnWizardUpdate()
        {
            helpString = "select target mesh root";
        }

        void Integrate()
        {
            if (m_root == null)
            {
                Debug.LogWarning("no root object");
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(m_root);
            var path = EditorUtility.SaveFilePanel("save prefab", Path.GetDirectoryName(prefabPath), m_root.name, "prefab");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var assetPath = UniGLTF.UnityPath.FromFullpath(path);
            if (!assetPath.IsUnderAssetsFolder)
            {
                Debug.LogWarning($"{path} is not asset path");
                return;
            }

            var excludes = m_excludes.Where(x => x.Exclude).Select(x => x.Mesh);
            integrationResults = MeshIntegratorEditor.Integrate(m_root, assetPath, excludes).Select(x => x.MeshMap).ToArray();
        }

        void OnWizardCreate()
        {
            Debug.Log("OnWizardCreate");
            Integrate();

            // close
        }

        void OnWizardOtherButton()
        {
            Debug.Log("OnWizardOtherButton");
            Integrate();
        }
    }
}
