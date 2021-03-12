using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 
    /// * https://github.com/dwango/UniVRM/issues/212.
    /// * https://blogs.unity3d.com/jp/2016/01/25/ggx-in-unity-5-3/  
    /// * https://github.com/vrm-c/UniVRM/issues/388
    /// 
    /// glTF = Unity
    /// Occlusion: src.r -> dst.g
    /// Roughness: src.g -> dst.a (bake smoothnessOrRoughness)
    /// Metallic : src.b -> dst.r
    /// </summary>
    public class OcclusionMetallicRoughnessConverter : ITextureConverter
    {
        private readonly float _smoothnessOrRoughness;

        public OcclusionMetallicRoughnessConverter(float smoothnessOrRoughness)
        {
            _smoothnessOrRoughness = smoothnessOrRoughness;
        }

        public static Texture2D GetImportTexture(Texture2D texture, float smoothnessOrRoughness)
        {
            TextureConverter.ColorConversion convert = src =>
            {
                return Import(src, smoothnessOrRoughness);
            };
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, convert, null);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Export, null);
            return converted;
        }

        public static Color32 Import(Color32 src, float _smoothnessOrRoughness)
        {
            var dst = new Color32
            {
                r = src.b, // Metallic
                g = src.r, // Occlusion
                b = 0, // not used
                // Roughness to Smoothness. Bake _smoothnessOrRoughness into a texture.
                a = (byte)(255 - src.g * _smoothnessOrRoughness),
            };

            return dst;
        }

        public Color32 Export(Color32 src)
        {
            var dst = new Color32
            {
                r = src.g, // Occlusion
                // Roughness from Smoothness. Bake divide _smoothnessOrRoughness from a texture.
                g = (byte)(255 - src.a),
                b = src.r, // Metallic
                a = 255, // not used
            };

            return dst;
        }
    }
}
