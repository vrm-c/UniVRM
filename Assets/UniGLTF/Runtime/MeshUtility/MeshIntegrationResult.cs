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

        public MeshMap MeshMap = new MeshMap();
    }
}
