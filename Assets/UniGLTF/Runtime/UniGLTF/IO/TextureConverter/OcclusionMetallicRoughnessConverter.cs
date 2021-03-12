using System;
using System.Linq;
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

        public delegate Color32 ColorConversion(Color32 metallicRoughness, Color32 occlusion);

        public static Texture2D Convert(Texture2D metallicRoughnessTexture, Texture2D occlusionTexture, Material convertMaterial,
            float metallicFactor, float roughnessFactor)
        {
            if (metallicRoughnessTexture != null && occlusionTexture != null)
            {
                if (metallicRoughnessTexture != occlusionTexture)
                {
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, RenderTextureReadWrite.Linear, convertMaterial);
                    var metallicRoughnessPixels = copyMetallicRoughness.GetPixels32();
                    var copyOcclusion = TextureConverter.CopyTexture(occlusionTexture, RenderTextureReadWrite.Linear, convertMaterial);
                    var occlusionPixels = copyOcclusion.GetPixels32();
                    if (metallicRoughnessPixels.Length != occlusionPixels.Length)
                    {
                        throw new NotImplementedException();
                    }
                    for (int i = 0; i < metallicRoughnessPixels.Length; ++i)
                    {
                        metallicRoughnessPixels[i] = Import(metallicRoughnessPixels[i], metallicFactor, roughnessFactor, occlusionPixels[i]);
                    }
                    copyMetallicRoughness.SetPixels32(metallicRoughnessPixels);
                    copyMetallicRoughness.Apply();
                    copyMetallicRoughness.name = metallicRoughnessTexture.name;
                    return copyMetallicRoughness;
                }
                else
                {
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, RenderTextureReadWrite.Linear, convertMaterial);
                    var metallicRoughnessPixels = copyMetallicRoughness.GetPixels32();
                    for (int i = 0; i < metallicRoughnessPixels.Length; ++i)
                    {
                        metallicRoughnessPixels[i] = Import(metallicRoughnessPixels[i], metallicFactor, roughnessFactor, metallicRoughnessPixels[i]);
                    }
                    copyMetallicRoughness.SetPixels32(metallicRoughnessPixels);
                    copyMetallicRoughness.Apply();
                    copyMetallicRoughness.name = metallicRoughnessTexture.name;
                    return copyMetallicRoughness;
                }
            }
            else if (metallicRoughnessTexture != null)
            {
                var copyTexture = TextureConverter.CopyTexture(metallicRoughnessTexture, RenderTextureReadWrite.Linear, convertMaterial);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => Import(x, metallicFactor, roughnessFactor, default)).ToArray());
                copyTexture.Apply();
                copyTexture.name = metallicRoughnessTexture.name;
                return copyTexture;
            }
            else if (occlusionTexture != null)
            {
                throw new NotImplementedException("occlusion only");
            }
            else
            {
                throw new ArgumentNullException("no texture");
            }
        }

        public static Texture2D GetImportTexture(Texture2D metallicRoughnessTexture, float metallicFactor, float roughnessFactor, Texture2D occlusionTexture)
        {
            return Convert(metallicRoughnessTexture, occlusionTexture, null, metallicFactor, roughnessFactor);
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Export, null);
            return converted;
        }

        public static Color32 Import(Color32 metallicRoughness, float metallicFactor, float roughnessFactor, Color32 occlusion)
        {
            var dst = new Color32
            {
                r = (byte)(metallicRoughness.b * metallicFactor), // Metallic
                g = occlusion.r, // Occlusion
                b = 0, // not used               
                a = (byte)(255 - metallicRoughness.g * roughnessFactor), // Roughness to Smoothness
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
