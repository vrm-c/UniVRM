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
#if VRM_DEVELOP
            Debug.Log($"{texture} => normalmap");
#endif
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.SaveAndReimport();
        }

        public static void ConfigureLinear(Texture texture, TextureImporter textureImporter)
        {
#if VRM_DEVELOP
            Debug.Log($"{texture} => linear");
#endif
            textureImporter.sRGBTexture = false;
            textureImporter.SaveAndReimport();
        }

        public static void ConfigureSampler(TextureImportParam param, TextureImporter textureImporter)
        {
            textureImporter.mipmapEnabled = param.Sampler.EnableMipMap;
            textureImporter.filterMode = param.Sampler.FilterMode;
            textureImporter.wrapModeU = param.Sampler.WrapModesU;
            textureImporter.wrapModeV = param.Sampler.WrapModesV;
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

        static void Configure(TextureImportParam textureInfo, Texture external, TextureImporter importer)
        {
            switch (textureInfo.TextureType)
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

            ConfigureSampler(textureInfo, importer);
        }

        public static void Configure(TextureImportParam textureInfo, IReadOnlyDictionary<SubAssetKey, Texture> ExternalMap)
        {
            if (ExternalMap.TryGetValue(textureInfo.SubAssetKey, out Texture external))
            {
                if (ImporterGetter.TryGetImporter(external, out ImporterGetter getter))
                {
                    using (getter)
                    {
                        Configure(textureInfo, external, getter.Importer);
                    }
                }
            }
        }
    }
}
