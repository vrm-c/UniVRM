using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// SpringBoneの各関節を表すデータ型
    /// </summary>
    public struct BlittableLogic
    {
        public float Length;
        public Vector3 CurrentTail;
        public Vector3 PrevTail;
        public Vector3 LocalDir;
        public Quaternion LocalRotation;
        public Vector3 BoneAxis;
    }
}