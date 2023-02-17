using UnityEngine;

namespace UniVRM10
{
    public readonly struct Vrm10ControlBone
    {
        public readonly Transform Transform;

        public readonly Vector3 InitialLocalPosition;

        public readonly Quaternion InitialLocalRotation;

        public readonly Quaternion InitialGlobalRotation;

        public Vrm10ControlBone(Transform transform)
        {
            Transform = transform;
            InitialLocalPosition = transform.localPosition;
            InitialLocalRotation = transform.localRotation;
            InitialGlobalRotation = transform.rotation;
        }

        /// <summary>
        /// 初期姿勢からの相対的な回転。
        /// 
        /// VRM-0.X 互換リグでは localRotation と同じ値を示す。
        /// </summary>
        public Quaternion NormalizedLocalRotation
        {
            get
            {
                return InitialGlobalRotation * Quaternion.Inverse(InitialLocalRotation) * Transform.localRotation * Quaternion.Inverse(InitialGlobalRotation);
            }
        }

        public void Reset()
        {
            Transform.localPosition = InitialLocalPosition;
            Transform.localRotation = InitialLocalRotation;
        }
    }
}
