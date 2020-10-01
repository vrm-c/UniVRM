using System;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Export時にMeshを一覧する
    /// </summary>
    [Serializable]
    public class VRMExportMeshes : ScriptableObject
    {
        [Serializable]
        public struct MeshInfo
        {
            public Mesh Mesh;
            public bool Skinned;
            public bool HasVertexColor;

            public MeshInfo(Renderer renderer)
            {
                Mesh = null;
                Skinned = false;
                HasVertexColor = false;
                if (renderer is SkinnedMeshRenderer smr)
                {
                    Skinned = true;
                    Mesh = smr.sharedMesh;
                }
                else if (renderer is MeshRenderer mr)
                {
                    var filter = mr.GetComponent<MeshFilter>();
                    if (filter != null)
                    {
                        Mesh = filter.sharedMesh;
                    }
                }
                if (Mesh == null)
                {
                    return;
                }
                HasVertexColor = Mesh.colors != null && Mesh.colors.Length == Mesh.vertexCount;
            }
        }

        public MeshInfo[] Meshes;
    }
}
