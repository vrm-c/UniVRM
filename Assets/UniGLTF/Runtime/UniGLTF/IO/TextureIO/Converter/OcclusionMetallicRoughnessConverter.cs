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
        private static Material _importer;
        private static Material Importer
        {
            get
            {
                if (_importer == null)
                {
                    _importer = new Material(Shader.Find("Hidden/UniGLTF/StandardMapImporter"));
                }
                return _importer;
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

            Importer.mainTexture = src;
            Importer.SetTexture("_GltfMetallicRoughnessTexture", metallicRoughnessTexture);
            Importer.SetTexture("_GltfOcclusionTexture", occlusionTexture);
            Importer.SetFloat("_GltfMetallicFactor", metallicFactor);
            Importer.SetFloat("_GltfRoughnessFactor", roughnessFactor);
            Importer.SetFloat("_IsLegacySquaredRoughness", isLegacySquaredRoughness ? 1 : 0);

            var dst = TextureConverter.CopyTexture(src, ColorSpace.Linear, true, Importer);

            Importer.mainTexture = null;
            Importer.SetTexture("_GltfMetallicRoughnessTexture", null);
            Importer.SetTexture("_GltfOcclusionTexture", null);
            Importer.SetFloat("_GltfMetallicFactor", 0);
            Importer.SetFloat("_GltfRoughnessFactor", 0);
            Importer.SetFloat("_IsLegacySquaredRoughness", 0);

            return dst;
        }

        private static Material _exporter;
        private static Material Exporter
        {
            get
            {
                if (_exporter == null)
                {
                    _exporter = new Material(Shader.Find("Hidden/UniGLTF/StandardMapExporter"));
                }
                return _exporter;
            }
        }

        public static Texture2D Export(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                throw new ArgumentNullException("no texture");
            }

            var src = metallicSmoothTexture != null ? metallicSmoothTexture : occlusionTexture;
            Exporter.mainTexture = src;
            Exporter.SetTexture("_UnityMetallicSmoothTexture", metallicSmoothTexture);
            Exporter.SetTexture("_UnityOcclusionTexture", occlusionTexture);
            Exporter.SetFloat("_UnitySmoothness", smoothness);

            var dst = TextureConverter.CopyTexture(src, ColorSpace.Linear, true, Exporter);

            Exporter.mainTexture = null;
            Exporter.SetTexture("_UnityMetallicSmoothTexture", null);
            Exporter.SetTexture("_UnityOcclusionTexture", null);
            Exporter.SetFloat("_UnitySmoothness", 0);

            return dst;
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
