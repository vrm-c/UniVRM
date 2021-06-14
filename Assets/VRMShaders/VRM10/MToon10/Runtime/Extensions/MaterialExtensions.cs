using UnityEngine;
using VRMShaders.VRM10.MToon10.Runtime;

namespace VRMShaders.VRM10.MToon10.Runtime
{
    public static class MaterialExtensions
    {
        public static void SetKeyword(this Material mat, string keyword, bool isEnabled)
        {
            if (isEnabled)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }

        public static int GetInt(this Material mat, MToon10Prop prop)
        {
            return mat.GetInt(prop.ToUnityShaderLabName());
        }

        public static void SetInt(this Material mat, MToon10Prop prop, int val)
        {
            mat.SetInt(prop.ToUnityShaderLabName(), val);
        }

        public static Texture GetTexture(this Material mat, MToon10Prop prop)
        {
            return mat.GetTexture(prop.ToUnityShaderLabName());
        }

        public static Vector2 GetTextureScale(this Material mat, MToon10Prop prop)
        {
            return mat.GetTextureScale(prop.ToUnityShaderLabName());
        }

        public static Vector2 GetTextureOffset(this Material mat, MToon10Prop prop)
        {
            return mat.GetTextureOffset(prop.ToUnityShaderLabName());
        }

        public static float GetFloat(this Material mat, MToon10Prop prop)
        {
            return mat.GetFloat(prop.ToUnityShaderLabName());
        }

        public static Color GetColor(this Material mat, MToon10Prop prop)
        {
            return mat.GetColor(prop.ToUnityShaderLabName());
        }
    }
}