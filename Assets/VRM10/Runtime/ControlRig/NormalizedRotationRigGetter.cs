using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 正規化済みのヒエラルキーの getter。
    /// localRotation をコピーするだけなのだが、
    /// GetNormalizedLocalRotation にて、ボーンの有無(upperChestなど)の違いの対応をする予定(未実装)。
    /// </summary>
    public class NormalizedRigGetter : IControlRigGetter
    {
        Transform m_root;
        Animator m_animator;

        public NormalizedRigGetter(Transform root, Animator animator)
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

        public Vector3 GetRootPosition()
        {
            // TODO: from model root ?
            return m_animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        }
    }
}
