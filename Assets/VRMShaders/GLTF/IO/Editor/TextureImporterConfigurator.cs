using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VRMShaders
{
    public static class TextureImporterConfigurator
    {
        public static void ConfigureSize(Texture texture, TextureImporter textureImporter)
        {
            var maxSize = Mathf.Max(texture.width, texture.height);
            textureImporter.maxTextureSize
                = maxSize > 4096 ? 8192 :
                maxSize > 2048 ? 4096 :
                maxSize > 1024 ? 2048 :
                maxSize > 512 ? 1024 :
                512;
            textureImporter.SaveAndReimport();
        }

        public static void ConfigureNormalMap(Texture texture, TextureImporter textureImporter)
        {
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.SaveAndReimport();
        }

        public static void ConfigureLinear(Texture texture, TextureImporter textureImporter)
        {
            textureImporter.sRGBTexture = false;
            textureImporter.SaveAndReimport();
        }

        public static void ConfigureSampler(TextureDescriptor texDesc, TextureImporter textureImporter)
        {
            textureImporter.mipmapEnabled = texDesc.Sampler.EnableMipMap;
            textureImporter.filterMode = texDesc.Sampler.FilterMode;
            textureImporter.wrapModeU = texDesc.Sampler.WrapModesU;
            textureImporter.wrapModeV = texDesc.Sampler.WrapModesV;
        }

        class ImporterGetter : IDisposable
        {
            public TextureImporter Importer;

            ImporterGetter(TextureImporter importer)
            {
                Importer = importer;
            }

            public void Dispose()
            {
                Importer.SaveAndReimport();
            }

            public static bool TryGetImporter(Texture texture, out ImporterGetter getter)
            {
                var path = AssetDatabase.GetAssetPath(texture);
                if (String.IsNullOrEmpty(path))
                {
                    Debug.LogWarning($"{path} is not asset");
                }
                else
                {
                    if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                    {
                        getter = new ImporterGetter(importer);
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"{path}: fail to get TextureImporter");
                    }
                }
                getter = default;
                return false;
            }
        }

        static void Configure(TextureDescriptor texDesc, Texture external, TextureImporter importer)
        {
            switch (texDesc.TextureType)
            {
                case TextureImportTypes.NormalMap:
                    {
                        ConfigureSize(external, importer);
                        ConfigureNormalMap(external, importer);
                    }
                    break;

                case TextureImportTypes.StandardMap:
                    {
                        ConfigureSize(external, importer);
                        ConfigureLinear(external, importer);
                    }
                    break;

                case TextureImportTypes.sRGB:
                    {
                        ConfigureSize(external, importer);
                    }
                    break;

                case TextureImportTypes.Linear:
                    {
                        ConfigureSize(external, importer);
                        ConfigureLinear(external, importer);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            ConfigureSampler(texDesc, importer);
        }

        public static void Configure(TextureDescriptor texDesc, IReadOnlyDictionary<SubAssetKey, Texture> ExternalMap)
        {
            if (ExternalMap.TryGetValue(texDesc.SubAssetKey, out Texture external))
            {
                if (ImporterGetter.TryGetImporter(external, out ImporterGetter getter))
                {
                    using (getter)
                    {
                        Configure(texDesc, external, getter.Importer);
                    }
                }
            }
        }
    }
}
