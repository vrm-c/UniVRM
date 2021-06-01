using System;
using UnityEngine;

namespace UniVRM10
{
    public enum VRM10SpringBoneColliderTypes
    {
        Sphere,
        Capsule,
    }

    [Serializable]
    public class VRM10SpringBoneCollider : MonoBehaviour
    {
        public VRM10SpringBoneColliderTypes ColliderType;

        /// <summary>bone local position</summary>
        public Vector3 Offset;

        [Range(0, 1.0f)]
        public float Radius;

        /// <summary>bone local position</summary>
        public Vector3 Tail;

        public static int SelectedGuid;

        public bool IsSelected => GetInstanceID() == SelectedGuid;
    }
}
