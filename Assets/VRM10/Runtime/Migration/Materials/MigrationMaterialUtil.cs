using System;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    internal static class MigrationMaterialUtil
    {
        private const string ShaderNameKey = "shader";
        private const string VectorPropertiesKey = "vectorProperties";
        private const string FloatPropertiesKey = "floatProperties";
        private const string TexturePropertiesKey = "textureProperties";
        private const string MainTexKey = "_MainTex";
        private const string ColorKey = "_Color";
        private const string CutoffKey = "_Cutoff";

        public static string GetShaderName(JsonNode vrm0XMaterial)
        {
            try
            {
                return vrm0XMaterial[ShaderNameKey].GetString();
            }
            catch (Exception)
            {
                Debug.LogWarning($"Migration Warning: ShaderName fallback default.");
                return string.Empty;
            }
        }

        public static float[] GetBaseColorFactor(JsonNode vrm0XMaterial)
        {
            try
            {
                var factor = vrm0XMaterial[VectorPropertiesKey][ColorKey];
                if (!factor.IsArray() || factor.GetArrayCount() != 4)
                {
                    throw new Exception("not float4");
                }
                return factor.ArrayItems().Select(x => ListTreeNodeExtensions.GetSingle(x)).ToArray();
            }
            catch (Exception)
            {
                Debug.LogWarning($"Migration Warning: BaseColorFactor fallback default.");
                return new float[] {1, 1, 1, 1};
            }
        }

        public static glTFMaterialBaseColorTextureInfo GetBaseColorTexture(JsonNode vrm0XMaterial)
        {
            try
            {
                return new glTFMaterialBaseColorTextureInfo
                {
                    index = vrm0XMaterial[TexturePropertiesKey][MainTexKey].GetInt32(),
                };
            }
            catch (Exception)
            {
                Debug.LogWarning($"Migration Warning: BaseColorTexture fallback default.");
                return null;
            }
        }

        public static float GetCutoff(JsonNode vrm0XMaterial)
        {
            try
            {
                return vrm0XMaterial[FloatPropertiesKey][CutoffKey].GetSingle();
            }
            catch (Exception)
            {
                Debug.LogWarning($"Migration Warning: Cutoff fallback default.");
                return 0.5f;
            }
        }
    }
}