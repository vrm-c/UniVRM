using System.Linq;
using UnityEngine;
using UniGLTF;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRM
{
    [Serializable]
    public class MeshPreviewItem
    {
        public string Path
        {
            get;
            private set;
        }

        public SkinnedMeshRenderer SkinnedMeshRenderer
        {
            get;
            private set;
        }

        public Mesh Mesh
        {
            get;
            private set;
        }

        public string[] BlendShapeNames
        {
            get;
            private set;
        }
        public int BlendShapeCount
        {
            get { return BlendShapeNames.Length; }
        }

        Material[] m_materials;
        public Material[] Materials
        {
            get { return m_materials; }
            private set
            {
                m_materials = value;
                m_materialNames = value.Select(x => x.name).ToArray();

#if UNITY_EDITOR
                int i = 0;
                foreach (var x in value)
                {
                    var propNames = Enumerable.Range(0, ShaderUtil.GetPropertyCount(x.shader))
                        .Where(y => ShaderUtil.GetPropertyType(x.shader, y) == ShaderUtil.ShaderPropertyType.Color)
                        .Select(y => ShaderUtil.GetPropertyName(x.shader, y))
                        .ToArray();
                    m_materialPropNamesList.Add(propNames);

                    var propTypes = Enumerable.Range(0, ShaderUtil.GetPropertyCount(x.shader))
                        .Select(y => ShaderUtil.GetPropertyType(x.shader, y))
                        .Where(y => y == ShaderUtil.ShaderPropertyType.Color)
                        .ToArray();
                    m_materialPropTypesList.Add(propTypes);

                    var propValues = propNames.Select(y => (Vector4)x.GetColor(y)).ToArray();
                    m_materialPropDefaultValueList.Add(propValues);
                }
#endif
            }
        }

        string[] m_materialNames;
        public string[] MaterialsNames
        {
            get { return m_materialNames; }
        }

        List<string[]> m_materialPropNamesList = new List<string[]>();
        public string[] GetMaterialPropNames(int materialIndex)
        {
            if(m_materialPropNamesList==null || materialIndex<0 || materialIndex >= m_materialPropNamesList.Count)
            {
                return null;
            }
            return m_materialPropNamesList[materialIndex];
        }

        List<ShaderUtil.ShaderPropertyType[]> m_materialPropTypesList = new List<ShaderUtil.ShaderPropertyType[]>();
        public ShaderUtil.ShaderPropertyType[] GetMaterialPropTypes(int materialIndex)
        {
            if (m_materialPropTypesList == null || materialIndex < 0 || materialIndex >= m_materialPropTypesList.Count)
            {
                return null;
            }
            return m_materialPropTypesList[materialIndex];
        }

        List<Vector4[]> m_materialPropDefaultValueList = new List<Vector4[]>();
        public Vector4[] GetMaterialPropBaseValues(int materialIndex)
        {
            if (m_materialPropDefaultValueList == null || materialIndex < 0 || materialIndex >= m_materialPropDefaultValueList.Count)
            {
                return null;
            }
            return m_materialPropDefaultValueList[materialIndex];
        }

        Transform m_transform;
        public Vector3 Position
        {
            get { return m_transform.position; }
        }
        public Quaternion Rotation
        {
            get { return m_transform.rotation; }
        }

        MeshPreviewItem(string path, Transform transform)
        {
            Path = path;
            m_transform = transform;
        }

        public void Bake(BlendShapeBinding[] values, MaterialValueBinding[] materialValues)
        {
            if (SkinnedMeshRenderer == null) return;

            // clear
            for (int i = 0; i < BlendShapeCount; ++i)
            {
                SkinnedMeshRenderer.SetBlendShapeWeight(i, 0);
            }

            if (values != null)
            {
                foreach (var x in values)
                {
                    if (x.RelativePath == Path)
                    {
                        SkinnedMeshRenderer.SetBlendShapeWeight(x.Index, x.Weight);
                    }
                }
            }

            if (materialValues != null)
            {
                // clear
                for (int i = 0; i < Materials.Length; ++i)
                {
                    var names = m_materialPropNamesList[i];
                    var defaultValues = m_materialPropDefaultValueList[i];
                    for (int j = 0; j < names.Length; ++j)
                    {
                        Materials[i].SetColor(names[j], defaultValues[j]);
                    }
                }

                foreach (var x in materialValues)
                {
                    if(x.RelativePath == Path)
                    {
                        var material = Materials[x.Index];
                        material.SetColor(x.ValueName, x.TargetValue);
                    }
                }
            }

            SkinnedMeshRenderer.BakeMesh(Mesh);
        }

        public void Clean()
        {
            foreach (var x in Materials)
            {
                UnityEngine.Object.DestroyImmediate(x);
            }
        }

        public static MeshPreviewItem Create(Transform t, Transform root, Func<Material, Material> getOrCreateMaterial)
        {
            var meshFilter = t.GetComponent<MeshFilter>();
            var meshRenderer = t.GetComponent<MeshRenderer>();
            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (meshFilter != null && meshRenderer != null)
            {
                // copy
                meshRenderer.sharedMaterials = meshRenderer.sharedMaterials.Select(x => getOrCreateMaterial(x)).ToArray();
                return new MeshPreviewItem(t.RelativePathFrom(root), t)
                {
                    Mesh = meshFilter.sharedMesh,
                    Materials = meshRenderer.sharedMaterials
                };
            }
            else if (skinnedMeshRenderer != null)
            {
                // copy
                skinnedMeshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials.Select(x => getOrCreateMaterial(x)).ToArray();
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    // bake required
                    var sharedMesh = skinnedMeshRenderer.sharedMesh;
                    return new MeshPreviewItem(t.RelativePathFrom(root), t)
                    {
                        SkinnedMeshRenderer = skinnedMeshRenderer,
                        Mesh = new Mesh(), // for bake
                        BlendShapeNames = Enumerable.Range(0, sharedMesh.blendShapeCount).Select(x => sharedMesh.GetBlendShapeName(x)).ToArray(),
                        Materials = skinnedMeshRenderer.sharedMaterials
                    };
                }
                else
                {
                    return new MeshPreviewItem(t.RelativePathFrom(root), t)
                    {
                        Mesh = skinnedMeshRenderer.sharedMesh,
                        Materials = skinnedMeshRenderer.sharedMaterials
                    };
                }
            }
            else
            {
                return null;
            }
        }
    }
}
