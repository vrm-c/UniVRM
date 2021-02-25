using System;
using UnityEngine;

namespace UniGLTF
{
    public enum Axises
    {
        Z,
        X,
    }

    public struct AxisInverter
    {
        public Func<Vector3, Vector3> InvertVector3;
        public Func<Vector4, Vector4> InvertVector4;
        public Func<Quaternion, Quaternion> InvertQuaternion;
        public Func<Matrix4x4, Matrix4x4> InvertMat4;

        public static AxisInverter ReverseZ => new AxisInverter
        {
            InvertVector3 = x => x.ReverseZ(),
            InvertVector4 = x => x.ReverseZ(),
            InvertQuaternion = x => x.ReverseZ(),
            InvertMat4 = x => x.ReverseZ(),
        };

        public static AxisInverter ReverseX => new AxisInverter
        {
            InvertVector3 = x => x.ReverseX(),
            InvertVector4 = x => x.ReverseX(),
            InvertQuaternion = x => x.ReverseX(),
            InvertMat4 = x => x.ReverseX(),
        };
    }
}
