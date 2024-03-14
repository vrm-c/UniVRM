using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    public struct DrawCount
    {
        public int Count;
        public Material Material;
    }

    [Serializable]
    public class MeshInfo
    {
        public Mesh Mesh;
        public List<DrawCount> SubMeshes = new List<DrawCount>();

        public SkinnedMeshRenderer IntegratedRenderer;

        public void AddIntegratedRendererTo(GameObject parent, Transform[] bones)
        {
            var go = new GameObject(Mesh.name);
            go.transform.SetParent(parent.transform, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = Mesh;
            smr.sharedMaterials = SubMeshes.Where(x => x.Count > 0).Select(x => x.Material).ToArray();
            smr.bones = bones;
            IntegratedRenderer = smr;
        }
    }

    public class MeshIntegrationResult
    {
        public List<SkinnedMeshRenderer> SourceSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<MeshRenderer> SourceMeshRenderers = new List<MeshRenderer>();

        public List<Mesh> Sources = new List<Mesh>();
        public MeshInfo Integrated;
        public MeshInfo IntegratedNoBlendShape;
        public Transform[] Bones;

        public void DestroySourceRenderer()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GameObject> AddIntegratedRendererTo(GameObject parent)
        {
            int count = 0;
            if (Integrated != null)
            {
                Integrated.AddIntegratedRendererTo(parent, Bones);
                ++count;
                yield return Integrated.IntegratedRenderer.gameObject;
            }
            if (IntegratedNoBlendShape != null)
            {
                IntegratedNoBlendShape.AddIntegratedRendererTo(parent, Bones);
                ++count;
                yield return IntegratedNoBlendShape.IntegratedRenderer.gameObject;
            }

            if (count == 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
