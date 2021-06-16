using System;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public struct RendererFirstPersonFlags
    {
        /// <summary>
        ///  RendererへのModelRootからの相対パス
        /// </summary>
        [SerializeField]
        public String Renderer;

        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.FirstPersonType FirstPersonFlag;

        public static RendererFirstPersonFlags Create(Transform root, Renderer r, UniGLTF.Extensions.VRMC_vrm.FirstPersonType flag)
        {
            return new RendererFirstPersonFlags
            {
                Renderer = r.transform.RelativePathFrom(root),
                FirstPersonFlag = flag,
            };
        }

        public Renderer GetRenderer(Transform root)
        {
            var node = root.Find(Renderer);
            return node?.GetComponent<Renderer>();
        }

        public Mesh GetSharedMesh(Transform root)
        {
            var renderer = GetRenderer(root);
            switch (renderer)
            {
                case SkinnedMeshRenderer smr:
                    return smr.sharedMesh;

                case MeshRenderer meshRenderer:
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null)
                    {
                        return filter.sharedMesh;
                    }
                    return null;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
