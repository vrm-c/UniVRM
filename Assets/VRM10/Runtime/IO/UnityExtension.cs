using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public static class UnityExtension
    {
        public static Vector3 ToUnityVector3(this System.Numerics.Vector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static float[] ToFloat3(this System.Numerics.Vector3 value)
        {
            return new[] { value.X, value.Y, value.Z };
        }

        public static Quaternion ToUnityQuaternion(this System.Numerics.Quaternion value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }

        public static float[] ToFloat4(this System.Numerics.Quaternion value)
        {
            return new float[] { value.X, value.Y, value.Z, value.W };
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 value)
        {
            return new System.Numerics.Vector2(value.x, value.y);
        }

        public static System.Numerics.Vector3 ToNumericsVector3(this Vector3 value)
        {
            return new System.Numerics.Vector3(value.x, value.y, value.z);
        }

        public static System.Numerics.Quaternion ToNumericsQuaternion(this Quaternion value)
        {
            return new System.Numerics.Quaternion(value.x, value.y, value.z, value.w);
        }
    }
}
