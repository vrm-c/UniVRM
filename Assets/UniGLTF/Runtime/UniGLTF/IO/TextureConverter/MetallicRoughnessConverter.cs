#define SIMPLE_CONV
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 
    /// * https://github.com/dwango/UniVRM/issues/212.
    /// * https://blogs.unity3d.com/jp/2016/01/25/ggx-in-unity-5-3/  
    /// * https://github.com/vrm-c/UniVRM/issues/388
    /// 
    /// Occlusion(glTF): src.r
    /// Roughness(glTF): src.g -> Smoothness(Unity): dst.a (bake smoothnessOrRoughness)
    /// Metallic(glTF) : src.b -> Metallic(Unity)  : dst.r
    /// </summary>
    public class MetallicRoughnessConverter : ITextureConverter
    {
        private readonly float _smoothnessOrRoughness;
        private readonly float _smoothnessOrRoughnessInverse;

        public MetallicRoughnessConverter(float smoothnessOrRoughness)
        {
            _smoothnessOrRoughness = smoothnessOrRoughness;
            _smoothnessOrRoughnessInverse = 1.0f / _smoothnessOrRoughness;
        }

        public Texture2D GetImportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Import, null);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Export, null);
            return converted;
        }

        public Color32 Import(Color32 src)
        {
            var dst = new Color32
            {
                r = src.b,
                g = 0,
                b = 0,
            };

            // Bake _smoothnessOrRoughness into a texture.
#if SIMPLE_CONV
            dst.a = (byte)(255 - src.g * _smoothnessOrRoughness);
#else
            var pixelRoughnessFactor = (src.g * _smoothnessOrRoughness) / 255.0f; // roughness
            var pixelSmoothness = 1.0f - Mathf.Sqrt(pixelRoughnessFactor);
            dst.a = (byte)Mathf.Clamp(pixelSmoothness * 255, 0, 255);
#endif
            return dst;
        }

        public Color32 Export(Color32 src)
        {

            var dst = new Color32
            {
                r = 0,
                b = src.r,
                a = 255,
            };

            // Bake divide _smoothnessOrRoughness from a texture.
#if SIMPLE_CONV
            dst.g = (byte)(255 - src.a);
#else                
            var pixelSmoothness = (src.a * _smoothnessOrRoughness) / 255.0f; // smoothness
            var pixelRoughnessFactorSqrt = (1.0f - pixelSmoothness);
            var pixelRoughnessFactor = pixelRoughnessFactorSqrt * pixelRoughnessFactorSqrt;
            dst.g = (byte)Mathf.Clamp(pixelRoughnessFactor * 255, 0, 255);
#endif                

            return dst;
        }
    }
}
