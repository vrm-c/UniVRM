using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [System.Serializable]
    public class MeshIntegrationResult
    {
        public List<SkinnedMeshRenderer> SourceSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<MeshRenderer> SourceMeshRenderers = new List<MeshRenderer>();
        public SkinnedMeshRenderer IntegratedRenderer;
    }
}
