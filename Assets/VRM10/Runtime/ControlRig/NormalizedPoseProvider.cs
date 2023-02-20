using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 正規化済みのヒエラルキーの getter。
    /// localRotation をコピーするだけなのだが、
    /// GetNormalizedLocalRotation にて、ボーンの有無(upperChestなど)の違いの対応をする予定(未実装)。
    /// </summary>
    public sealed class NormalizedPoseProvider : INormalizedPoseProvider, ITPoseProvider
    {
        Transform m_root;
        Animator m_animator;

        public Vector3 HipTPoseWorldPosition => throw new System.NotImplementedException();

        public NormalizedPoseProvider(Transform root, Animator animator)
        {
            m_root = root;
            m_animator = animator;
        }

        public Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
        {
            if (m_animator.GetBoneTransform(bone) is Transform t)
            {
                // TODO: parentBone relative
                return t.localRotation;
            }
            else
            {
                // Debug.LogWarning($"{bone} not found");
                return Quaternion.identity;
            }
        }

        public Vector3 GetHipsPosition()
        {
            // TODO: from model root ?
            return m_animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        }

        public Quaternion GetBoneTPoseWorldRotation(HumanBodyBones bone)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBones()
        {
            throw new System.NotImplementedException();
        }
    }
}
