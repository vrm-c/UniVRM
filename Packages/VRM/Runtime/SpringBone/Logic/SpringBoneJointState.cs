using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// 毎フレーム更新される Verlet 積分の位置状態
    /// </summary>
    readonly struct SpringBoneJointState
    {
        public readonly Vector3 CurrentTail;
        public readonly Vector3 PrevTail;

        public SpringBoneJointState(Vector3 currentTail, Vector3 prevTail) =>
            (CurrentTail, PrevTail) = (currentTail, prevTail);

        public static SpringBoneJointState Init(Transform center, Transform transform, Vector3 localChildPosition)
        {
            var worldChildPosition = transform.TransformPoint(localChildPosition);
            var tail = center != null
                    ? center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition;
            return new SpringBoneJointState(currentTail: tail, prevTail: tail);
        }

        public static SpringBoneJointState Make(Transform center, Vector3 currentTail, Vector3 nextTail)
        {
            return new SpringBoneJointState
            (
                prevTail: center != null
                    ? center.InverseTransformPoint(currentTail)
                    : currentTail,
                currentTail: center != null
                    ? center.InverseTransformPoint(nextTail)
                    : nextTail);
        }

        public SpringBoneJointState ToWorld(Transform center)
        {
            return new SpringBoneJointState
            (
                currentTail: center != null
                    ? center.TransformPoint(CurrentTail)
                    : CurrentTail,
                prevTail: center != null
                    ? center.TransformPoint(PrevTail)
                    : PrevTail);
        }
    };
}