using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class TextureImporterConfigurator
    {
        public static void ConfigureSize(Texture2D texture)
        {
            var path = UnityPath.FromAsset(texture);
            if (AssetImporter.GetAtPath(path.Value) is TextureImporter textureImporter)
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
            else
            {
                throw new System.IO.FileNotFoundException($"{path}");
            }
        }

        public static void ConfigureNormalMap(Texture2D texture)
        {
            var path = UnityPath.FromAsset(texture);
            if (AssetImporter.GetAtPath(path.Value) is TextureImporter textureImporter)
            {
#if VRM_DEVELOP
                Debug.Log($"{path} => normalmap");
#endif
                textureImporter.textureType = TextureImporterType.NormalMap;
                textureImporter.SaveAndReimport();
            }
            else
            {
                throw new System.IO.FileNotFoundException($"{path}");
            }
        }

        public static void ConfigureLinear(Texture2D texture)
        {
            var path = UnityPath.FromAsset(texture);
            if (AssetImporter.GetAtPath(path.Value) is TextureImporter textureImporter)
            {
#if VRM_DEVELOP
                Debug.Log($"{path} => linear");
#endif
                textureImporter.sRGBTexture = false;
                textureImporter.SaveAndReimport();
            }
            else
            {
                throw new System.IO.FileNotFoundException($"{path}");
            }
        }

        public static void Configure(TextureImportParam textureInfo, IDictionary<string, Texture2D> ExternalMap)
        {
            switch (textureInfo.TextureType)
            {
                case TextureImportTypes.NormalMap:
                    {
                        if (ExternalMap.TryGetValue(textureInfo.GltfName, out Texture2D external))
                        {
                            ConfigureSize(external);
                            ConfigureNormalMap(external);
                        }
                    }
                    break;

                case TextureImportTypes.StandardMap:
                    {
                        if (ExternalMap.TryGetValue(textureInfo.ConvertedName, out Texture2D external))
                        {
                            ConfigureSize(external);
                            ConfigureLinear(external);
                        }
                    }
                    break;

                case TextureImportTypes.sRGB:
                    {
                        if (ExternalMap.TryGetValue(textureInfo.GltfName, out Texture2D external))
                        {
                            ConfigureSize(external);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
