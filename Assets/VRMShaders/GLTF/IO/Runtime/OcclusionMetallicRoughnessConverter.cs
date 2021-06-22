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
        private static Material exporter;
        private static Material Exporter
        {
            get
            {
                if (exporter == null)
                {
                    exporter = new Material(Shader.Find("Hidden/UniGLTF/StandardMapImporter"));
                }
                return exporter;
            }
        }

        /// <summary>
        /// Import glTF Metallic-Roughness texture to Unity Metallic-Smoothness-Occlusion texture.
        ///
        /// isLegacySquaredRoughness:
        ///     Before UniGLTF v0.69, roughness value in the texture was invalid squared value.
        /// </summary>
        public static Texture2D Import(Texture2D metallicRoughnessTexture,
            float metallicFactor, float roughnessFactor, Texture2D occlusionTexture, bool isLegacySquaredRoughness)
        {
            if (metallicRoughnessTexture == null && occlusionTexture == null)
            {
                throw new ArgumentNullException("no texture");
            }

            var src = metallicRoughnessTexture != null ? metallicRoughnessTexture : occlusionTexture;

            Exporter.mainTexture = src;
            Exporter.SetTexture("_GltfMetallicRoughnessTexture", metallicRoughnessTexture);
            Exporter.SetTexture("_GltfOcclusionTexture", occlusionTexture);
            Exporter.SetFloat("_GltfMetallicFactor", metallicFactor);
            Exporter.SetFloat("_GltfRoughnessFactor", roughnessFactor);
            Exporter.SetFloat("_IsLegacySquaredRoughness", isLegacySquaredRoughness ? 1 : 0);

            var dst = TextureConverter.CopyTexture(src, ColorSpace.Linear, true, Exporter);

            Exporter.mainTexture = null;
            Exporter.SetTexture("_GltfMetallicRoughnessTexture", null);
            Exporter.SetTexture("_GltfOcclusionTexture", null);
            Exporter.SetFloat("_GltfMetallicFactor", 0);
            Exporter.SetFloat("_GltfRoughnessFactor", 0);
            Exporter.SetFloat("_IsLegacySquaredRoughness", 0);

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
                var dst = TextureConverter.CreateEmptyTextureWithSettings(occlusionTexture, ColorSpace.Linear, false);
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
