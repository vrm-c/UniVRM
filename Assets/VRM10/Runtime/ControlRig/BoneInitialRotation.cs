using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Represents the rotation at the initial pose (TPose)
    /// </summary>
    public readonly struct BoneInitialRotation
    {
        public readonly Transform Transform;

        public readonly Vector3 InitialLocalPosition;

        public readonly Quaternion InitialLocalRotation;

        public readonly Quaternion InitialGlobalRotation;

        public BoneInitialRotation(Transform transform)
        {
            Transform = transform;
            InitialLocalPosition = transform.localPosition;
            InitialLocalRotation = transform.localRotation;
            InitialGlobalRotation = transform.rotation;
        }

        /// <summary>
        /// Convert the local rotation, including the initial rotation, to a normalized local rotation
        /// </summary>
        public Quaternion NormalizedLocalRotation
        {
            get
            {
                return InitialGlobalRotation * Quaternion.Inverse(InitialLocalRotation) * Transform.localRotation * Quaternion.Inverse(InitialGlobalRotation);
            }
        }
    }
}
