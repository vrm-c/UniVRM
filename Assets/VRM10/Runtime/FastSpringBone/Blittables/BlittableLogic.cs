using System;
using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// SpringBoneの各関節に紐付いた計算情報を表すデータ型
    /// </summary>
    [Serializable]
    public struct BlittableLogic
    {
        public int headIndex;
        public int tailIndex;
        public float length;
        public Vector3 currentTail;
        public Vector3 prevTail;
        public Vector3 localDir;
        public Quaternion localRotation;
        public Vector3 boneAxis;
    }
}