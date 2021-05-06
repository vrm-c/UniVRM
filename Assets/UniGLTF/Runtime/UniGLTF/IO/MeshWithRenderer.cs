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
                var uniqueBones = skin.bones.Distinct().ToArray();
                UniqueBones = uniqueBones;
                JointIndexMap = new int[skin.bones.Length];

                var bones = skin.bones;
                for (int i = 0; i < bones.Length; ++i)
                {
                    JointIndexMap[i] = Array.IndexOf(uniqueBones, bones[i]);
                }
            }
            else
            {
                UniqueBones = null;
                JointIndexMap = null;
            }
        }

        /// <summary>
        /// glTF は　skinning の boneList の重複を許可しない
        /// (unity は ok)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetJointIndex(int index)
        {
            if (index < 0)
            {
                return index;
            }

            if (JointIndexMap != null)
            {
                return JointIndexMap[index];
            }
            else
            {
                return index;
            }
        }

        public IEnumerable<Matrix4x4> GetBindPoses()
        {
            var used = new HashSet<int>();
            for (int i = 0; i < JointIndexMap.Length; ++i)
            {
                var index = JointIndexMap[i];
                if (used.Add(index))
                {
                    yield return Mesh.bindposes[i];
                }
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
                if (x.Renderer == null
                || x.Renderer.sharedMaterials == null
                || x.Renderer.sharedMaterials.Length == 0)
                {
                    continue;
                }

                yield return x;
            }
        }

        public bool IsSameMeshAndMaterials(MeshWithRenderer other)
        {
            return IsSameMeshAndMaterials(other.Mesh, other.Renderer.sharedMaterials);
        }

        public bool IsSameMeshAndMaterials(Mesh mesh, Material[] materials)
        {
            if (Mesh != mesh) return false;
            if (Renderer.sharedMaterials.Length != materials.Length) return false;
            for (var i = 0; i < Renderer.sharedMaterials.Length; i++)
            {
                if (Renderer.sharedMaterials[i] != materials[i]) return false;
            }
            return true;
        }
    }
}
