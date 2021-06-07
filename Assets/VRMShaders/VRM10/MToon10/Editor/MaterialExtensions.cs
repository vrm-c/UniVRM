using UnityEngine;

namespace VRMShaders.VRM10.MToon10.Editor
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

        public static int GetInt(this Material mat, Prop prop)
        {
            return mat.GetInt(prop.ToName());
        }

        public static void SetInt(this Material mat, Prop prop, int val)
        {
            mat.SetInt(prop.ToName(), val);
        }
    }
}