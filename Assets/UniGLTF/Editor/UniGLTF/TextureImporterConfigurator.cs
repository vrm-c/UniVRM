using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class TextureImporterConfigurator
    {
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

        public static void Configure(GetTextureParam textureInfo, IDictionary<string, Texture2D> ExternalMap)
        {
            switch (textureInfo.TextureType)
            {
                case GetTextureParam.TextureTypes.NormalMap:
                    {
                        if (ExternalMap.TryGetValue(textureInfo.GltflName, out Texture2D external))
                        {
                            ConfigureNormalMap(external);
                        }
                    }
                    break;

                case GetTextureParam.TextureTypes.StandardMap:
                    {
                        if (ExternalMap.TryGetValue(textureInfo.ConvertedName, out Texture2D external))
                        {
                            ConfigureLinear(external);
                        }
                    }
                    break;

                case GetTextureParam.TextureTypes.sRGB:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
