using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 正規化済みのヒエラルキーの getter。
    /// 回転の変換はしないが、ボーンの有無(upperChestなど)の対応はする。
    /// </summary>
    public class NormalizedRigGetter : IControlRigGetter
    {
        Animator m_animator;

        public NormalizedRigGetter(Animator animator)
        {
            m_animator = animator;
        }

        public Quaternion GetNormalizedRotation(HumanBodyBones bone, HumanBodyBones parentBone)
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

        public Vector3 GetRootPosition()
        {
            // TODO: from model root ?
            return m_animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        }
    }
}
