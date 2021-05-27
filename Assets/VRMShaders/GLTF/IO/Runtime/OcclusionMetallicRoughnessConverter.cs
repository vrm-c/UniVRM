using System;
using System.Linq;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;


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
            // TODO: Replace with Shader implementation
            if (metallicRoughnessTexture != null && occlusionTexture != null)
            {
                if (metallicRoughnessTexture == occlusionTexture)
                {
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, ColorSpace.Linear, true, null);
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
                    var copyMetallicRoughness = TextureConverter.CopyTexture(metallicRoughnessTexture, ColorSpace.Linear, true, null);
                    var metallicRoughnessPixels = copyMetallicRoughness.GetPixels32();
                    var copyOcclusion = TextureConverter.CopyTexture(occlusionTexture, ColorSpace.Linear, false, null);
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
                    DestroyTexture(copyOcclusion);
                    return copyMetallicRoughness;
                }
            }
            else if (metallicRoughnessTexture != null)
            {
                var copyTexture = TextureConverter.CopyTexture(metallicRoughnessTexture, ColorSpace.Linear, true, null);
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => ImportPixel(x, metallicFactor, roughnessFactor, default)).ToArray());
                copyTexture.Apply();
                copyTexture.name = metallicRoughnessTexture.name;
                return copyTexture;
            }
            else if (occlusionTexture != null)
            {
                var copyTexture = TextureConverter.CopyTexture(occlusionTexture, ColorSpace.Linear, true, null);
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
            // TODO: Replace with Shader implementation
            if (metallicSmoothTexture != null && occlusionTexture != null)
            {
                if (metallicSmoothTexture == occlusionTexture)
                {
                    var dst = TextureConverter.CreateEmptyTextureWithSettings(metallicSmoothTexture, ColorSpace.Linear, false);
                    var linearTexture = TextureConverter.CopyTexture(metallicSmoothTexture, ColorSpace.Linear, true, null);
                    dst.SetPixels32(linearTexture.GetPixels32().Select(x => ExportPixel(x, smoothness, x)).ToArray());
                    dst.Apply();
                    dst.name = metallicSmoothTexture.name;
                    DestroyTexture(linearTexture);
                    return dst;
                }
                else
                {
                    var dst = TextureConverter.CreateEmptyTextureWithSettings(metallicSmoothTexture, ColorSpace.Linear, false);
                    var linearMetallicSmooth = TextureConverter.CopyTexture(metallicSmoothTexture, ColorSpace.Linear, true, null);
                    var metallicSmoothPixels = linearMetallicSmooth.GetPixels32();
                    var linearOcclusion = TextureConverter.CopyTexture(occlusionTexture, ColorSpace.Linear, false, null);
                    var occlusionPixels = linearOcclusion.GetPixels32();
                    if (metallicSmoothPixels.Length != occlusionPixels.Length)
                    {
                        throw new NotImplementedException();
                    }
                    for (int i = 0; i < metallicSmoothPixels.Length; ++i)
                    {
                        metallicSmoothPixels[i] = ExportPixel(metallicSmoothPixels[i], smoothness, occlusionPixels[i]);
                    }
                    dst.SetPixels32(metallicSmoothPixels);
                    dst.Apply();
                    dst.name = metallicSmoothTexture.name;
                    DestroyTexture(linearMetallicSmooth);
                    DestroyTexture(linearOcclusion);
                    return dst;
                }
            }
            else if (metallicSmoothTexture)
            {
                var dst = TextureConverter.CreateEmptyTextureWithSettings(metallicSmoothTexture, ColorSpace.Linear, false);
                var linearMetallicSmooth = TextureConverter.CopyTexture(metallicSmoothTexture, ColorSpace.Linear, true, null);
                dst.SetPixels32(linearMetallicSmooth.GetPixels32().Select(x => ExportPixel(x, smoothness, default)).ToArray());
                dst.Apply();
                dst.name = metallicSmoothTexture.name;
                DestroyTexture(linearMetallicSmooth);
                return dst;
            }
            else if (occlusionTexture)
            {
                var dst = TextureConverter.CreateEmptyTextureWithSettings(metallicSmoothTexture, ColorSpace.Linear, false);
                var linearOcclusion = TextureConverter.CopyTexture(occlusionTexture, ColorSpace.Linear, false, null);
                dst.SetPixels32(linearOcclusion.GetPixels32().Select(x => ExportPixel(default, smoothness, x)).ToArray());
                dst.Apply();
                dst.name = occlusionTexture.name;
                DestroyTexture(linearOcclusion);
                return dst;
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

        private static void DestroyTexture(Texture obj)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}
