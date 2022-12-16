using System;
using UnityEngine;

namespace UniVRM10
{
    public class AnimatorControlRigInput : IControlRigInput
    {
        Animator animator_;
        Transform hips_;

        public AnimatorControlRigInput(Animator animator)
        {
            if (animator == null)
            {
                throw new ArgumentNullException();
            }
            animator_ = animator;
            hips_ = animator_.GetBoneTransform(HumanBodyBones.Hips);
        }

        public Vector3 RootPosition => hips_.localPosition;

        public bool TryGetBoneLocalRotation(HumanBodyBones bone, Quaternion parent, out Quaternion rotation)
        {
            var t = animator_.GetBoneTransform(bone);
            if (t == null)
            {
                rotation = default;
                return false;
            }
            rotation = t.localRotation;
            return true;
        }
    }
}
