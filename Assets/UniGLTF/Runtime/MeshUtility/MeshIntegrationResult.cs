using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [System.Serializable]
    public class MeshMap
    {
        public List<Mesh> Sources = new List<Mesh>();
        public Mesh Integrated;
        public Material[] SharedMaterials;
        public Transform[] Bones;
    }

    public class MeshIntegrationResult
    {
        public List<SkinnedMeshRenderer> SourceSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<MeshRenderer> SourceMeshRenderers = new List<MeshRenderer>();
        public MeshMap MeshMap = new MeshMap();
        public SkinnedMeshRenderer IntegratedRenderer;

        public void AddIntegratedRendererTo(GameObject parent)
        {
            var go = new GameObject(MeshMap.Integrated.name);
            go.transform.SetParent(parent.transform, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = MeshMap.Integrated;
            smr.sharedMaterials = MeshMap.SharedMaterials;
            smr.bones = MeshMap.Bones;

            IntegratedRenderer = smr;
        }

        public void DestroySourceRenderer()
        {
            throw new NotImplementedException();
        }
    }
}
