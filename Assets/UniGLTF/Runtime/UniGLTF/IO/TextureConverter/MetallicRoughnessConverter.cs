using UnityEngine;

namespace UniGLTF
{
    public class MetallicRoughnessConverter : ITextureConverter
    {
        private float _smoothnessOrRoughness;

        public MetallicRoughnessConverter(float smoothnessOrRoughness)
        {
            _smoothnessOrRoughness = smoothnessOrRoughness;
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
            // Roughness(glTF): dst.g -> Smoothness(Unity): src.a (with conversion)
            // Metallic(glTF) : dst.b -> Metallic(Unity)  : src.r

            var pixelRoughnessFactor = (src.g * _smoothnessOrRoughness) / 255.0f; // roughness
            var pixelSmoothness = 1.0f - Mathf.Sqrt(pixelRoughnessFactor);

            return new Color32
            {
                r = src.b,
                g = 0,
                b = 0,
                // Bake roughness values into a texture.
                // See: https://github.com/dwango/UniVRM/issues/212.
                a = (byte)Mathf.Clamp(pixelSmoothness * 255, 0, 255),
            };
        }

        public Color32 Export(Color32 src)
        {
            // Smoothness(Unity): src.a -> Roughness(glTF): dst.g (with conversion)
            // Metallic(Unity)  : src.r -> Metallic(glTF) : dst.b

            var pixelSmoothness = (src.a * _smoothnessOrRoughness) / 255.0f; // smoothness
            // https://blogs.unity3d.com/jp/2016/01/25/ggx-in-unity-5-3/
            var pixelRoughnessFactorSqrt = (1.0f - pixelSmoothness);
            var pixelRoughnessFactor = pixelRoughnessFactorSqrt * pixelRoughnessFactorSqrt;

            return new Color32
            {
                r = 0,
                // Bake smoothness values into a texture.
                // See: https://github.com/dwango/UniVRM/issues/212.
                g = (byte)Mathf.Clamp(pixelRoughnessFactor * 255, 0, 255),
                b = src.r,
                a = 255,
            };
        }
    }

}
