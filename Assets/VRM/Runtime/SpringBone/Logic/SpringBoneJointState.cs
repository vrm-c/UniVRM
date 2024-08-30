using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// 毎フレーム更新される Verlet 積分の位置状態
    /// </summary>
    struct SpringBoneJointState
    {
        public Vector3 CurrentTail;
        public Vector3 PrevTail;

        public static SpringBoneJointState Init(Transform center, Transform transform, Vector3 localChildPosition)
        {
            var worldChildPosition = transform.TransformPoint(localChildPosition);
            var tail = center != null
                    ? center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition;
            return new SpringBoneJointState
            {
                CurrentTail = tail,
                PrevTail = tail,
            };
        }

        public static SpringBoneJointState Make(Transform center, Vector3 currentTail, Vector3 nextTail)
        {
            return new SpringBoneJointState
            {
                PrevTail = center != null
                    ? center.InverseTransformPoint(currentTail)
                    : currentTail,
                CurrentTail = center != null
                    ? center.InverseTransformPoint(nextTail)
                    : nextTail,
            };
        }

        public SpringBoneJointState ToWorld(Transform center)
        {
            return new SpringBoneJointState
            {
                CurrentTail = center != null
                    ? center.TransformPoint(CurrentTail)
                    : CurrentTail,
                PrevTail = center != null
                    ? center.TransformPoint(PrevTail)
                    : PrevTail,
            };
        }
    };
}