using System;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public static class Matrix4x4Extensions
    {
        public static void CalcYawPitch(this Matrix4x4 m, Vector3 target, out float yaw, out float pitch)
        {
            var zaxis = Vector3.Project(target, m.GetColumn(2));
            var yaxis = Vector3.Project(target, m.GetColumn(1));
            var xaxis = Vector3.Project(target, m.GetColumn(0));

            var yawPlusMinus = Vector3.Dot(xaxis, m.GetColumn(0)) > 0 ? 1.0f : -1.0f;
            yaw = (float)Math.Atan2(xaxis.magnitude, zaxis.magnitude) * yawPlusMinus * Mathf.Rad2Deg;

            var pitchPlusMinus = Vector3.Dot(yaxis, m.GetColumn(1)) > 0 ? 1.0f : -1.0f;
            pitch = (float)Math.Atan2(yaxis.magnitude, (xaxis + zaxis).magnitude) * pitchPlusMinus * Mathf.Rad2Deg;
        }

        public static Quaternion YawPitchRotation(this Matrix4x4 m, float yaw, float pitch)
        {
            return Quaternion.AngleAxis(yaw, m.GetColumn(1)) * Quaternion.AngleAxis(-pitch, m.GetColumn(0));
        }

        public static Matrix4x4 RotationToWorldAxis(this Matrix4x4 m)
        {
            return UnityExtensions.Matrix4x4FromColumns(
                m.MultiplyVector(Vector3.right),
                m.MultiplyVector(Vector3.up),
                m.MultiplyVector(Vector3.forward),
                new Vector4(0, 0, 0, 1)
                );
        }
    }
}
