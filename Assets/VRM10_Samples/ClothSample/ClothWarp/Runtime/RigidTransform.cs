using UnityEngine;

namespace UniVRM10.ClothWarp
{
    public struct RigidTransform
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public RigidTransform(in Vector3 pos, in Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }

        public RigidTransform(Transform t)
        {
            Position = t.position;
            Rotation = t.rotation;
        }
    }
}