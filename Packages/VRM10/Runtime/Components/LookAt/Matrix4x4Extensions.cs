using System;
using UnityEngine;


namespace UniVRM10
{
    public static class Matrix4x4Extensions
    {
        /// <summary>
        /// from AimConstraint
        /// </summary>
        /// <param name="Yaw"></param>
        /// <param name="m"></param>
        /// <param name="target"></param>
        /// <returns>yaw, pitch degree</returns>
        public static (float Yaw, float Pitch) CalcYawPitch(this Matrix4x4 m, Vector3 target)
        {
            var localPosition = m.inverse.MultiplyPoint(target);

            var zaxis = Vector3.Project(localPosition, Vector3.forward);
            var yaxis = Vector3.Project(localPosition, Vector3.up);
            var xaxis = Vector3.Project(localPosition, Vector3.right);

            var xDot = Vector3.Dot(xaxis, Vector3.right) > 0;
            var yDot = Vector3.Dot(yaxis, Vector3.up) > 0;
            var zDot = Vector3.Dot(zaxis, Vector3.forward) > 0;

            // x z plane
            var yaw = (float)Math.Atan2(xaxis.magnitude, zaxis.magnitude) * Mathf.Rad2Deg;
            if (xDot && zDot)
            {
                // 1st(0-90)

            }
            else if (xDot && !zDot)
            {
                // 2nd(90-180)
                yaw = 180 - yaw;
            }
            else if (!xDot && !zDot)
            {
                // 3rd
                yaw = -180 + yaw;
            }
            else if (!xDot && zDot)
            {
                // 4th
                yaw = -yaw;
            }
            else
            {
                throw new NotImplementedException();
            }

            // x+y z plane
            var pitch = (float)Math.Atan2(yaxis.magnitude, (xaxis + zaxis).magnitude) * Mathf.Rad2Deg;
            if (yDot)
            {
                pitch = -pitch;
            }

            return (yaw, pitch);
        }

        /// <summary>
        /// yaw: 右が+
        /// pitch: 上が+
        /// という仕様。vrm-0.x から据え置き
        /// </summary>
        /// <param name="m"></param>
        /// <param name="target"></param>
        /// <param name="yaw">Degree</param>
        /// <param name="pitch">eegree</param>
        public static void CalcYawPitch(this Matrix4x4 m, Vector3 target, out float yaw, out float pitch)
        {
            var z = Vector3.Dot(target, m.GetColumn(2));
            var x = Vector3.Dot(target, m.GetColumn(0));
            yaw = Mathf.Atan2(x, z) * Mathf.Rad2Deg;

            // x+y z plane
            var xz = Mathf.Sqrt(x * x + z * z);
            var y = Vector3.Dot(target, m.GetColumn(1));
            pitch = Mathf.Atan2(y, xz) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// yaw: 右が+
        /// pitch: 上が+
        /// という仕様。vrm-0.x から据え置き
        /// </summary>
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
