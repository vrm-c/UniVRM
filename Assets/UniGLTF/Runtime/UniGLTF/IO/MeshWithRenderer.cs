using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public struct MeshWithRenderer
    {
        public readonly Mesh Mesh;
        public readonly Renderer Renderer;
        public readonly Transform[] UniqueBones;
        readonly int[] JointIndexMap;

        public MeshWithRenderer(Transform x)
        {
            Mesh = x.GetSharedMesh();
            Renderer = x.GetComponent<Renderer>();

            if (Renderer is SkinnedMeshRenderer skin && skin.bones != null && skin.bones.Length > 0)
            {
                // has joints
                UniqueBones = skin.bones;
                // UniqueBones = skin.bones.Distinct().ToArray();

                JointIndexMap = new int[skin.bones.Length];

                var bones = skin.bones;
                for (int i = 0; i < bones.Length; ++i)
                {
                    JointIndexMap[i] = Array.IndexOf(bones, bones[i]);
                }
            }
            else
            {
                UniqueBones = null;
                JointIndexMap = null;
            }
        }

        public int GetJointIndex(int index)
        {
            if (index < 0)
            {
                return index;
            }

            // 重複した index をひとつ目に変更する
            if (JointIndexMap != null)
            {
                return JointIndexMap[index];
            }
            else
            {
                return index;
            }
        }

        public static IEnumerable<MeshWithRenderer> FromNodes(IEnumerable<Transform> nodes)
        {
            foreach (var node in nodes)
            {
                var x = new MeshWithRenderer(node);
                if (x.Mesh == null)
                {
                    continue; ;
                }
                if (x.Renderer.sharedMaterials == null
                || x.Renderer.sharedMaterials.Length == 0)
                {
                    continue;
                }

                yield return x;
            }
        }
    }
}
