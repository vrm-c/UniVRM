using System;
using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// Transformの必要な機能だけを絞り、Blittableに対応させたクラス
    /// </summary>
    [Serializable]
    public struct BlittableTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Matrix4x4 localToWorldMatrix;
        public Matrix4x4 worldToLocalMatrix;
    }
}