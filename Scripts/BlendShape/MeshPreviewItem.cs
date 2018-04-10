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
#if UNITY_EDITOR
    [Serializable]
    public struct PropItem
    {
        public ShaderUtil.ShaderPropertyType PropertyType;
        public Vector4 DefaultValues;
    }
#endif

    [Serializable]
    public class MaterialItem
    {
        public Material Material { get; private set; }
#if UNITY_EDITOR
        public Dictionary<string, PropItem> PropMap = new Dictionary<string, PropItem>();

        public string[] PropNames
        {
            get;
            private set;
        }
#endif

        public static MaterialItem Create(Material material)
        {
            var item = new MaterialItem
            {
                Material = material
            };
#if UNITY_EDITOR

            var propNames = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); ++i)
            {
                var propType = ShaderUtil.GetPropertyType(material.shader, i);
                if (propType == ShaderUtil.ShaderPropertyType.Color)
                {
                    var name = ShaderUtil.GetPropertyName(material.shader, i);
                    item.PropMap.Add(name, new PropItem
                    {
                        PropertyType = propType,
                        DefaultValues = material.GetColor(name),
                    });
                    propNames.Add(name);
                }
            }
            item.PropNames = propNames.ToArray();
#endif
            return item;
        }
    }

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

        public Material[] Materials
        {
            get;
            private set;
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

        MeshPreviewItem(string path, Transform transform, Material[] materials)
        {
            Path = path;
            m_transform = transform;
            Materials = materials;
        }

        public void Bake(BlendShapeBinding[] values, float weight)
        {
            if (SkinnedMeshRenderer == null) return;

            // Update baked mesh
            if (values != null)
            {
                // clear
                for (int i = 0; i < BlendShapeCount; ++i)
                {
                    SkinnedMeshRenderer.SetBlendShapeWeight(i, 0);
                }

                foreach (var x in values)
                {
                    if (x.RelativePath == Path)
                    {
                        SkinnedMeshRenderer.SetBlendShapeWeight(x.Index, x.Weight * weight);
                    }
                }
            }
            SkinnedMeshRenderer.BakeMesh(Mesh);
        }

        public static MeshPreviewItem Create(Transform t, Transform root,
            Func<Material, Material> getOrCreateMaterial)
        {
            //Debug.Log("create");

            var meshFilter = t.GetComponent<MeshFilter>();
            var meshRenderer = t.GetComponent<MeshRenderer>();
            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (meshFilter != null && meshRenderer != null)
            {
                // copy
                meshRenderer.sharedMaterials = meshRenderer.sharedMaterials.Select(x => getOrCreateMaterial(x)).ToArray();
                return new MeshPreviewItem(t.RelativePathFrom(root), t, meshRenderer.sharedMaterials)
                {
                    Mesh = meshFilter.sharedMesh
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
                    return new MeshPreviewItem(t.RelativePathFrom(root), t, skinnedMeshRenderer.sharedMaterials)
                    {
                        SkinnedMeshRenderer = skinnedMeshRenderer,
                        Mesh = new Mesh(), // for bake
                        BlendShapeNames = Enumerable.Range(0, sharedMesh.blendShapeCount).Select(x => sharedMesh.GetBlendShapeName(x)).ToArray()
                    };
                }
                else
                {
                    return new MeshPreviewItem(t.RelativePathFrom(root), t, skinnedMeshRenderer.sharedMaterials)
                    {
                        Mesh = skinnedMeshRenderer.sharedMesh,
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
