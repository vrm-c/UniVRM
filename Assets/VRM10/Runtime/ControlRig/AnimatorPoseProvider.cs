using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Animator から Pose 供給する。
    /// この Animator は以下の条件を満たすこと。
    /// 
    /// * TPoseであること
    /// * 正規化済みであること
    /// 
    /// </summary>
    public sealed class AnimatorPoseProvider : INormalizedPoseProvider, ITPoseProvider
    {
        Transform m_root;
        Animator m_animator;
        Dictionary<HumanBodyBones, Vector3> m_posMap = new Dictionary<HumanBodyBones, Vector3>();

        public AnimatorPoseProvider(Transform root, Animator animator)
        {
            m_root = root;
            m_animator = animator;

            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            HipTPoseWorldPosition = root.worldToLocalMatrix.MultiplyPoint(hips.position);

            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if(bone==HumanBodyBones.LastBone)
                {
                    continue;
                }
                var t = animator.GetBoneTransform(bone);
                if (t != null)
                {
                    m_posMap[bone] = root.worldToLocalMatrix.MultiplyPoint(t.position);
                }
            }
        }

        #region INormalizedPoseProvider
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

        public Vector3 GetRawHipsPosition()
        {
            // TODO: from model root ?
            return m_animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        }
        #endregion

        #region ITPoseProvider
        public Vector3 HipTPoseWorldPosition { get; }

        public IEnumerable<(HumanBodyBones Bone, HumanBodyBones Parent)> EnumerateBoneParentPairs()
        {
            throw new System.NotImplementedException();
        }

        public Quaternion? GetBoneWorldRotation(HumanBodyBones bone)
        {
            throw new System.NotImplementedException();
        }

        public Vector3? GetBoneWorldPosition(HumanBodyBones bone)
        {
            return m_posMap[bone];
        }
        #endregion
    }
}
