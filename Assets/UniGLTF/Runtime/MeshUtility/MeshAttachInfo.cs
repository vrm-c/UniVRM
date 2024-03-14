using System;
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
            if (dst == null)
            {
                throw new ArgumentNullException();
            }

            if (Bones != null)
            {
                // recalc bindposes
                Mesh.bindposes = Bones.Select(x => x.worldToLocalMatrix * dst.transform.localToWorldMatrix).ToArray();

                if (dst.GetComponent<SkinnedMeshRenderer>() is SkinnedMeshRenderer dstRenderer)
                {
                    dstRenderer.sharedMesh = Mesh;
                    dstRenderer.sharedMaterials = Materials;
                    dstRenderer.bones = Bones;
                    dstRenderer.rootBone = RootBone;
                }
                else
                {
                    Debug.LogError($"SkinnedMeshRenderer not found", dst);
                }
            }
            else
            {
                if (dst.GetComponent<MeshFilter>() is MeshFilter dstFilter)
                {
                    dstFilter.sharedMesh = Mesh;
                    if (dst.gameObject.GetComponent<MeshRenderer>() is MeshRenderer dstRenderer)
                    {
                        dstRenderer.sharedMaterials = Materials;
                    }
                    else
                    {
                        Debug.LogError($"MeshRenderer not found", dst);
                    }
                }
                else
                {
                    Debug.LogError($"MeshFilter not found", dst);
                }
            }
        }
    }
}