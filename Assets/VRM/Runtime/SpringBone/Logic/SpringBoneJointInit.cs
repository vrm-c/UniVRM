using UnityEngine;

namespace VRM.SpringBone
{
    struct JointState
    {
        public Vector3 CurrentTail;
        public Vector3 PrevTail;

        public static JointState Init(Transform center, Transform transform, Vector3 localChildPosition)
        {
            var worldChildPosition = transform.TransformPoint(localChildPosition);
            var tail = center != null
                    ? center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition;
            return new JointState
            {
                CurrentTail = tail,
                PrevTail = tail,
            };
        }

        public static JointState Make(Transform center, Vector3 currentTail, Vector3 nextTail)
        {
            return new JointState
            {
                PrevTail = center != null
                    ? center.InverseTransformPoint(currentTail)
                    : currentTail,
                CurrentTail = center != null
                    ? center.InverseTransformPoint(nextTail)
                    : nextTail,
            };
        }

        public JointState ToWorld(Transform center)
        {
            return new JointState
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