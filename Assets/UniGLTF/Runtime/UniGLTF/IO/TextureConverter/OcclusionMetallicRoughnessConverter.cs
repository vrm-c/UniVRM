using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 
    /// * https://github.com/vrm-c/UniVRM/issues/781 
    /// 
    /// Unity = glTF
    /// Occlusion: unity.g = glTF.r
    /// Roughness: unity.a = 1 - glTF.g * roughnessFactor
    /// Metallic : unity.r = glTF.b * metallicFactor
    /// 
    /// glTF = Unity
    /// Occlusion: glTF.r = unity.g
    /// Roughness: glTF.g = 1 - unity.a * smoothness
    /// Metallic : glTF.b = unity.r
    /// 
    /// </summary>
    public class OcclusionMetallicRoughnessConverter : ITextureConverter
    {
        private readonly float _smoothnessOrRoughness;

        public OcclusionMetallicRoughnessConverter(float smoothnessOrRoughness)
        {
            _smoothnessOrRoughness = smoothnessOrRoughness;
        }

        public static Texture2D GetImportTexture(Texture2D texture, float metallicFactor, float roughnessFactor)
        {
            TextureConverter.ColorConversion convert = src =>
            {
                return Import(src, metallicFactor, roughnessFactor);
            };
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, convert, null);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Export, null);
            return converted;
        }

        public static Color32 Import(Color32 src, float metallicFactor, float roughnessFactor)
        {
            var dst = new Color32
            {
                r = (byte)(src.b * metallicFactor), // Metallic
                g = src.r, // Occlusion
                b = 0, // not used               
                a = (byte)(255 - src.g * roughnessFactor), // Roughness to Smoothness
            };

            return dst;
        }

        public Color32 Export(Color32 src)
        {
            var dst = new Color32
            {
                r = src.g, // Occlusion               
                g = (byte)(255 - src.a), // Roughness from Smoothness
                b = src.r, // Metallic
                a = 255, // not used
            };

            return dst;
        }
    }
}
