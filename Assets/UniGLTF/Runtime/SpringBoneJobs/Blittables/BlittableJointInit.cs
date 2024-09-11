using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// 初期状態から計算できる Joint のパラメーター
    /// </summary>
    [Serializable]
    public struct BlittableJointInit
    {
        public int parentTransformIndex;
        public int headTransformIndex;
        public int tailTransformIndex;
        public float length;
        public Quaternion localRotation;
        public Vector3 boneAxis;
    }
}