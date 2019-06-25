#pragma warning disable 0414, 0649
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;


namespace VRM
{
    public class MeshIntegratorWizard : ScriptableWizard
    {
        [SerializeField]
        GameObject m_root;

        [SerializeField]
        Material[] m_uniqueMaterials;

        [Serializable]
        struct MaterialKey
        {
            public string Shader;
            public KeyValuePair<string, object>[] Properties;

            public override bool Equals(object obj)
            {
                if(!(obj is MaterialKey))
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

        [Header("Result")]
        public Mesh integrated;

        [MenuItem(SkinnedMeshUtility.MENU_KEY + "MeshIntegrator Wizard", priority = SkinnedMeshUtility.MENU_PRIORITY)]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<MeshIntegratorWizard>("MeshIntegrator", "Integrate and close window", "Integrate");
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
            Debug.Log("OnValidate");
            if (m_root == null)
            {
                m_uniqueMaterials = new Material[] { };
                m_duplicateMaterials = new MaterialList[] { };
                return;
            }

            m_uniqueMaterials = MeshIntegrator.EnumerateRenderer(m_root.transform, false)
                .SelectMany(x => x.sharedMaterials)
                .Distinct()
                .ToArray();

            m_duplicateMaterials = m_uniqueMaterials
                .GroupBy(x => GetMaterialKey(x), x => x)
                .Select(x => new MaterialList(x.ToArray()))
                .Where(x => x.Materials.Length > 1)
                .ToArray()
                ;
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

            var renderer = MeshIntegrator.Integrate(m_root);
            if (renderer == null)
            {
                return;
            }

            integrated = renderer.sharedMesh;
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
