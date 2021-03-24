using System;
using System.Linq;
using UnityEngine;


namespace VRMShaders
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
    public static class OcclusionMetallicRoughnessConverter
    {
        public static Texture2D Import(Texture2D metallicRoughnessTexture,
            float metallicFactor, float roughnessFactor, Texture2D occlusionTexture)
        {
            if (metallicRoughnessTexture != null && occlusionTexture != null)
            {
                if (metallicRoughnessTexture == occlusionTexture)
                {
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, TextureImportTypes.StandardMap, null);
                    var metallicRoughnessPixels = copyMetallicRoughness.GetPixels32();
                    for (int i = 0; i < metallicRoughnessPixels.Length; ++i)
                    {
                        metallicRoughnessPixels[i] = ImportPixel(metallicRoughnessPixels[i], metallicFactor, roughnessFactor, metallicRoughnessPixels[i]);
                    }
                    copyMetallicRoughness.SetPixels32(metallicRoughnessPixels);
                    copyMetallicRoughness.Apply();
                    copyMetallicRoughness.name = metallicRoughnessTexture.name;
                    return copyMetallicRoughness;
                }
                else
                {
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, TextureImportTypes.StandardMap, null);
                    var metallicRoughnessPixels = copyMetallicRoughness.GetPixels32();
                    var copyOcclusion = TextureConverter.CopyTexture(occlusionTexture, TextureImportTypes.StandardMap, null);
                    var occlusionPixels = copyOcclusion.GetPixels32();
                    if (metallicRoughnessPixels.Length != occlusionPixels.Length)
                    {
                        throw new NotImplementedException();
                    }
                    for (int i = 0; i < metallicRoughnessPixels.Length; ++i)
                    {
                        metallicRoughnessPixels[i] = ImportPixel(metallicRoughnessPixels[i], metallicFactor, roughnessFactor, occlusionPixels[i]);
                    }
                    copyMetallicRoughness.SetPixels32(metallicRoughnessPixels);
                    copyMetallicRoughness.Apply();
                    copyMetallicRoughness.name = metallicRoughnessTexture.name;
                    return copyMetallicRoughness;
                }
            }
            else if (metallicRoughnessTexture != null)
            {
                var copyTexture = TextureConverter.CopyTexture(metallicRoughnessTexture, TextureImportTypes.StandardMap, null);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ImportPixel(x, metallicFactor, roughnessFactor, default)).ToArray());
                copyTexture.Apply();
                copyTexture.name = metallicRoughnessTexture.name;
                return copyTexture;
            }
            else if (occlusionTexture != null)
            {
                var copyTexture = TextureConverter.CopyTexture(occlusionTexture, TextureImportTypes.StandardMap, null);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ImportPixel(default, metallicFactor, roughnessFactor, x)).ToArray());
                copyTexture.Apply();
                copyTexture.name = occlusionTexture.name;
                return copyTexture;
            }
            else
            {
                throw new ArgumentNullException("no texture");
            }
        }

        public static Color32 ImportPixel(Color32 metallicRoughness, float metallicFactor, float roughnessFactor, Color32 occlusion)
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

        public static Texture2D Export(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture != null && occlusionTexture != null)
            {
                if (metallicSmoothTexture == occlusionTexture)
                {
                    var copyTexture = TextureConverter.CopyTexture(metallicSmoothTexture, TextureImportTypes.StandardMap, null);
                    copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ExportPixel(x, smoothness, x)).ToArray());
                    copyTexture.Apply();
                    copyTexture.name = metallicSmoothTexture.name;
                    return copyTexture;
                }
                else
                {
                    var copyMetallicSmooth = TextureConverter.CopyTexture(metallicSmoothTexture, TextureImportTypes.StandardMap, null);
                    var metallicSmoothPixels = copyMetallicSmooth.GetPixels32();
                    var copyOcclusion = TextureConverter.CopyTexture(occlusionTexture, TextureImportTypes.StandardMap, null);
                    var occlusionPixels = copyOcclusion.GetPixels32();
                    if (metallicSmoothPixels.Length != occlusionPixels.Length)
                    {
                        throw new NotImplementedException();
                    }
                    for (int i = 0; i < metallicSmoothPixels.Length; ++i)
                    {
                        metallicSmoothPixels[i] = ExportPixel(metallicSmoothPixels[i], smoothness, occlusionPixels[i]);
                    }
                    copyMetallicSmooth.SetPixels32(metallicSmoothPixels);
                    copyMetallicSmooth.Apply();
                    copyMetallicSmooth.name = metallicSmoothTexture.name;
                    return copyMetallicSmooth;
                }
            }
            else if (metallicSmoothTexture)
            {
                var copyTexture = TextureConverter.CopyTexture(metallicSmoothTexture, TextureImportTypes.StandardMap, null);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ExportPixel(x, smoothness, default)).ToArray());
                copyTexture.Apply();
                copyTexture.name = metallicSmoothTexture.name;
                return copyTexture;
            }
            else if (occlusionTexture)
            {
                var copyTexture = TextureConverter.CopyTexture(occlusionTexture, TextureImportTypes.StandardMap, null);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ExportPixel(default, smoothness, x)).ToArray());
                copyTexture.Apply();
                copyTexture.name = occlusionTexture.name;
                return copyTexture;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public static Color32 ExportPixel(Color32 metallicSmooth, float smoothness, Color32 occlusion)
        {
            var dst = new Color32
            {
                r = occlusion.g, // Occlusion               
                g = (byte)(255 - metallicSmooth.a * smoothness), // Roughness from Smoothness
                b = metallicSmooth.r, // Metallic
                a = 255, // not used
            };

            return dst;
        }
    }
}
