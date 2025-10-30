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
                Mesh.bindposes = Bones.Select(x =>
                {
                    if (x != null)
                    {
                        return x.worldToLocalMatrix * dst.transform.localToWorldMatrix;
                    }
                    else
                    {
                        // ボーンが削除された
                        return dst.transform.localToWorldMatrix;
                    }
                }
                    ).ToArray();

                if (dst.TryGetComponent<SkinnedMeshRenderer>(out var dstRenderer))
                {
                    dstRenderer.sharedMesh = Mesh;
                    dstRenderer.sharedMaterials = Materials;
                    dstRenderer.bones = Bones;
                    dstRenderer.rootBone = RootBone;
                }
                else
                {
                    UniGLTFLogger.Error($"SkinnedMeshRenderer not found", dst);
                }
            }
            else
            {
                if (dst.TryGetComponent<MeshFilter>(out var dstFilter))
                {
                    dstFilter.sharedMesh = Mesh;
                    if (dst.gameObject.TryGetComponent<MeshRenderer>(out var dstRenderer))
                    {
                        dstRenderer.sharedMaterials = Materials;
                    }
                    else
                    {
                        UniGLTFLogger.Error($"MeshRenderer not found", dst);
                    }
                }
                else
                {
                    UniGLTFLogger.Error($"MeshFilter not found", dst);
                }
            }
        }
    }
}