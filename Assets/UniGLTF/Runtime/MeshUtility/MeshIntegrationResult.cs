using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [System.Serializable]
    public class MeshMap
    {
        public List<Mesh> Sources = new List<Mesh>();
        public Mesh Integrated;
    }

    public class MeshIntegrationResult
    {
        public List<SkinnedMeshRenderer> SourceSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<MeshRenderer> SourceMeshRenderers = new List<MeshRenderer>();
        public SkinnedMeshRenderer IntegratedRenderer;

        public MeshMap MeshMap;

        public void CreateMeshMap()
        {
            MeshMap = new MeshMap
            {
                Integrated = IntegratedRenderer.sharedMesh
            };

            foreach (var x in SourceSkinnedMeshRenderers)
            {
                MeshMap.Sources.Add(x.sharedMesh);
            }
            foreach (var x in SourceMeshRenderers)
            {
                var filter = x.GetComponent<MeshFilter>();
                MeshMap.Sources.Add(filter.sharedMesh);
            }
        }
    }
}
