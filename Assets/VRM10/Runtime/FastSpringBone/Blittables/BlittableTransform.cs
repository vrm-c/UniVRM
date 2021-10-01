using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// Transformの必要な機能だけを絞り、Blittableに対応させたクラス
    /// </summary>
    public struct BlittableTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public Matrix4x4 LocalToWorldMatrix;
        public Matrix4x4 WorldToLocalMatrix;
    }
}