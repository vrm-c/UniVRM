using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshAttachInfo
    {
        public Mesh Mesh;
        public Material[] Materials;
        public Transform[] Bones;
        public Transform RootBone;
        public void ReplaceMesh(GameObject dst)
        {
            if (Bones != null)
            {
                // recalc bindposes
                Mesh.bindposes = Bones.Select(x => x.worldToLocalMatrix * dst.transform.localToWorldMatrix).ToArray();

                var dstRenderer = dst.GetComponent<SkinnedMeshRenderer>();
                dstRenderer.sharedMesh = Mesh;
                dstRenderer.sharedMaterials = Materials;
                dstRenderer.bones = Bones;
                dstRenderer.rootBone = RootBone;
            }
            else
            {
                var dstFilter = dst.GetComponent<MeshFilter>();
                dstFilter.sharedMesh = Mesh;
                var dstRenderer = dst.gameObject.AddComponent<MeshRenderer>();
                dstRenderer.sharedMaterials = Materials;
            }
        }
    }
}