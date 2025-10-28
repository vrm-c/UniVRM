using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10
{
    [Serializable]
    public sealed class PreviewMeshItem
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

        PreviewMeshItem(string path, Transform transform, Material[] materials)
        {
            Path = path;
            m_transform = transform;
            Materials = materials;
        }

        public void Bake(IEnumerable<MorphTargetBinding> values, float weight)
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
                        if (x.Index >= 0 && x.Index < SkinnedMeshRenderer.sharedMesh.blendShapeCount)
                        {
                            SkinnedMeshRenderer.SetBlendShapeWeight(x.Index, x.Weight * weight * MorphTargetBinding.VRM_TO_UNITY);
                        }
                        else
                        {
                            UniGLTFLogger.Warning($"Out of range {SkinnedMeshRenderer.name}: 0 <= {x.Index} < {SkinnedMeshRenderer.sharedMesh.blendShapeCount}");
                        }
                    }
                }
            }
            SkinnedMeshRenderer.BakeMesh(Mesh);
        }

        public static PreviewMeshItem Create(Transform t, Transform root,
            Func<Material, Material> getOrCreateMaterial)
        {
            if (t.TryGetComponent<MeshFilter>(out var meshFilter) && t.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                // copy
                meshRenderer.sharedMaterials = meshRenderer.sharedMaterials.Select(x => getOrCreateMaterial(x)).ToArray();
                return new PreviewMeshItem(t.RelativePathFrom(root), t, meshRenderer.sharedMaterials)
                {
                    Mesh = meshFilter.sharedMesh
                };
            }
            else if (t.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                // copy
                skinnedMeshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials.Select(x => getOrCreateMaterial(x)).ToArray();
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    // bake required
                    var sharedMesh = skinnedMeshRenderer.sharedMesh;
                    return new PreviewMeshItem(t.RelativePathFrom(root), t, skinnedMeshRenderer.sharedMaterials)
                    {
                        SkinnedMeshRenderer = skinnedMeshRenderer,
                        Mesh = new Mesh(), // for bake
                        BlendShapeNames = Enumerable.Range(0, sharedMesh.blendShapeCount).Select(x => sharedMesh.GetBlendShapeName(x)).ToArray()
                    };
                }
                else
                {
                    return new PreviewMeshItem(t.RelativePathFrom(root), t, skinnedMeshRenderer.sharedMaterials)
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
