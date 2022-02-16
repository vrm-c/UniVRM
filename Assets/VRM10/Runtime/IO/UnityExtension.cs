using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public static class UnityExtension
    {
        // public static Vector3 ToUnityVector3(this System.Numerics.Vector3 value)
        // {
        //     return new Vector3(value.x, value.y, value.z);
        // }

        public static float[] ToFloat3(this Vector3 value)
        {
            return new[] { value.x, value.y, value.z };
        }

        // public static Quaternion ToUnityQuaternion(this System.Numerics.Quaternion value)
        // {
        //     return new Quaternion(value.x, value.y, value.z, value.w);
        // }

        public static float[] ToFloat4(this Quaternion value)
        {
            return new float[] { value.x, value.y, value.z, value.w };
        }

        // public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 value)
        // {
        //     return new System.Numerics.Vector2(value.x, value.y);
        // }

        // public static System.Numerics.Vector3 ToNumericsVector3(this Vector3 value)
        // {
        //     return new System.Numerics.Vector3(value.x, value.y, value.z);
        // }

        // public static System.Numerics.Quaternion ToNumericsQuaternion(this Quaternion value)
        // {
        //     return new System.Numerics.Quaternion(value.x, value.y, value.z, value.w);
        // }
    }
}
