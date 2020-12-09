using System;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public struct OffsetOnTransform
    {
        public Transform Transform;
        public Matrix4x4 OffsetRotation;

        public Matrix4x4 WorldMatrix
        {
            get
            {
                if (Transform == null) return Matrix4x4.identity;
                return Transform.localToWorldMatrix * OffsetRotation;
            }
        }

        public Vector3 WorldForward
        {
            get
            {
                var m = WorldMatrix;
                return m.GetColumn(2); // zaxis
            }
        }

        Matrix4x4 m_initialLocalMatrix;
        public void Setup()
        {
            if (Transform == null) return;
            m_initialLocalMatrix = Transform.parent.worldToLocalMatrix * Transform.localToWorldMatrix;
        }

        public Matrix4x4 InitialWorldMatrix
        {
            get
            {
                return Transform.parent.localToWorldMatrix * m_initialLocalMatrix;
            }
        }

        public static OffsetOnTransform Create(Transform transform)
        {
            var coordinate = new OffsetOnTransform
            {
                Transform = transform
            };

            if (transform != null)
            {
                coordinate.OffsetRotation = transform.worldToLocalMatrix.RotationToWorldAxis();
            }

            return coordinate;
        }
    }
}
