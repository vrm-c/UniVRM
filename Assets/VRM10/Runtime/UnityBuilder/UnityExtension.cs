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

        public static Color ToUnityColor(this System.Numerics.Vector3 value)
        {
            return new Color(value.X, value.Y, value.Z, 1);
        }

        public static Vector4 ToUnityVector4(this System.Numerics.Vector4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }

        public static Color ToUnityColor(this System.Numerics.Vector4 value)
        {
            return new Color(value.X, value.Y, value.Z, value.W);
        }

        public static Color ToUnitySRGB(this VrmLib.LinearColor value)
        {
            return value.RGBA.ToUnityColor().gamma;
        }

        public static Color ToUnityLinear(this VrmLib.LinearColor value)
        {
            return value.RGBA.ToUnityColor();
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

        public static System.Numerics.Vector4 ToNumericsVector4(this Vector4 value)
        {
            return new System.Numerics.Vector4(value.x, value.y, value.z, value.w);
        }

        /// UnityのMaterialのColor値はSRGBで格納されている
        public static VrmLib.LinearColor FromUnitySrgbToLinear(this Color value)
        {
            value = value.linear;
            return new VrmLib.LinearColor
            {
                RGBA = new System.Numerics.Vector4(value.r, value.g, value.b, value.a)
            };
        }

        public static VrmLib.LinearColor FromUnityLinear(this Color value)
        {
            return new VrmLib.LinearColor
            {
                RGBA = new System.Numerics.Vector4(value.r, value.g, value.b, value.a)
            };
        }

        public static System.Numerics.Vector4 ToVector4(this Color value)
        {
            return new System.Numerics.Vector4(value.r, value.g, value.b, value.a);
        }

        public static System.Numerics.Quaternion ToNumericsQuaternion(this Quaternion value)
        {
            return new System.Numerics.Quaternion(value.x, value.y, value.z, value.w);
        }

        public static System.Numerics.Matrix4x4 ToNumericsMatrix4x4(this Matrix4x4 value)
        {
            return new System.Numerics.Matrix4x4(
                value.m00, value.m01, value.m02, value.m03,
                value.m10, value.m11, value.m12, value.m13,
                value.m20, value.m21, value.m22, value.m23,
                value.m30, value.m31, value.m32, value.m33
                );
        }

        public static VrmLib.Image ToPngImage(this UnityEngine.Texture2D texture, VrmLib.ImageUsage imageUsage)
        {
            if (texture != null)
            {
                return new VrmLib.Image(texture.name, "image/png", imageUsage, new System.ArraySegment<byte>(texture.EncodeToPNG()));
            }
            else
            {
                return null;
            }
        }
    }
}