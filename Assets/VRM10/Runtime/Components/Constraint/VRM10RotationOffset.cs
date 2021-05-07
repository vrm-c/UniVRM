using System;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public struct VRM10RotationOffset
    {
        [SerializeField]
        public Quaternion Rotation;

        public static VRM10RotationOffset Identity => new VRM10RotationOffset
        {
            Rotation = Quaternion.identity,
        };
    }
}
