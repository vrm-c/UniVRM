using System.Linq;
using UnityEngine;
using UniGLTF;


namespace VRM
{
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

        public int BlendShapeCount
        {
            get;
            private set;
        }

        Material[] m_materials;
        public Material[] Materials
        {
            get { return m_materials; }
            private set
            {
                m_materials = value;
                m_MaterialNames = value.Select(x => x.name).ToArray();
            }
        }

        string[] m_MaterialNames;
        public string[] MaterialsNames
        {
            get { return m_MaterialNames; }
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

        public void Bake(BlendShapeBinding[] values)
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
            SkinnedMeshRenderer.BakeMesh(Mesh);
        }

        public static MeshPreviewItem Create(Transform t, Transform root)
        {
            var meshFilter = t.GetComponent<MeshFilter>();
            var meshRenderer = t.GetComponent<MeshRenderer>();
            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (meshFilter != null && meshRenderer != null)
            {
                return new MeshPreviewItem(t.RelativePathFrom(root), t)
                {
                    Mesh = meshFilter.sharedMesh,
                    Materials = meshRenderer.sharedMaterials
                };
            }
            else if (skinnedMeshRenderer != null)
            {
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    // bake required
                    return new MeshPreviewItem(t.RelativePathFrom(root), t)
                    {
                        SkinnedMeshRenderer = skinnedMeshRenderer,
                        Mesh = new Mesh(), // for bake
                        BlendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount,
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
