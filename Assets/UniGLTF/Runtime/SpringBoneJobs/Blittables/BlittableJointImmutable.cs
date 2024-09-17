using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Reconstruct に対して Immutable。
    /// Jointの増減、初期姿勢の変更など構成の変更は Reconstruct が必要。
    /// 変わりにくいスコープ。
    /// </summary>
    [Serializable]
    public struct BlittableJointImmutable
    {
        public int parentTransformIndex;
        public int headTransformIndex;
        public int tailTransformIndex;
        public float length;
        public Quaternion localRotation;
        public Vector3 boneAxis;
    }
}