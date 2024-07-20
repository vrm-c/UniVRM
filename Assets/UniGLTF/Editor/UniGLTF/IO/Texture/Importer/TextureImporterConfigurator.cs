using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VRMShaders
{
    public static class TextureImporterConfigurator
    {
        private static void ConfigureSize(TextureImporter textureImporter)
        {
            if (!EditorTextureUtility.TryGetOriginalTexturePixelSize(textureImporter, out var originalSize)) return;

            var originalMaxSize = Mathf.Max(originalSize.x, originalSize.y);
            textureImporter.maxTextureSize = originalMaxSize > 4096 ? 8192 :
                originalMaxSize > 2048 ? 4096 :
                originalMaxSize > 1024 ? 2048 :
                originalMaxSize > 512 ? 1024 :
                512;
        }

        private static void ConfigureNormalMap(TextureImporter textureImporter)
        {
            textureImporter.textureType = TextureImporterType.NormalMap;
        }

        private static void ConfigureLinear(TextureImporter textureImporter)
        {
            textureImporter.sRGBTexture = false;
        }

        private static void ConfigureSampler(TextureDescriptor texDesc, TextureImporter textureImporter)
        {
            textureImporter.mipmapEnabled = texDesc.Sampler.EnableMipMap;
            textureImporter.filterMode = texDesc.Sampler.FilterMode;
            textureImporter.wrapModeU = texDesc.Sampler.WrapModesU;
            textureImporter.wrapModeV = texDesc.Sampler.WrapModesV;
        }

        private static void Configure(TextureDescriptor texDesc, TextureImporter importer)
        {
            switch (texDesc.TextureType)
            {
                case TextureImportTypes.NormalMap:
                    {
                        ConfigureSize(importer);
                        ConfigureNormalMap(importer);
                    }
                    break;

                case TextureImportTypes.StandardMap:
                    {
                        ConfigureSize(importer);
                        ConfigureLinear(importer);
                    }
                    break;

                case TextureImportTypes.sRGB:
                    {
                        ConfigureSize(importer);
                    }
                    break;

                case TextureImportTypes.Linear:
                    {
                        ConfigureSize(importer);
                        ConfigureLinear(importer);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            ConfigureSampler(texDesc, importer);
        }

        public static void Configure(TextureDescriptor texDesc, IReadOnlyDictionary<SubAssetKey, Texture> externalMap)
        {
            if (!externalMap.TryGetValue(texDesc.SubAssetKey, out var externalTexture)) return;
            if (!EditorTextureUtility.TryGetAsEditorTexture2DAsset(externalTexture, out var texture2D, out var importer)) return;

            Configure(texDesc, importer);
            importer.SaveAndReimport();
        }
    }
}
