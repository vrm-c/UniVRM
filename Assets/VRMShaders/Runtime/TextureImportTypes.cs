using System;
using UnityEngine;

namespace VRMShaders
{
    public enum TextureImportTypes
    {
        // runtime:
        //   new Texture2D(linear = false)
        // extract:
        sRGB,
        // runtime:
        //   new Texture2D(linear = true)
        //   encode to DXT5nm
        // extract:
        //   TextureImporter.textureType = TextureImporterType.NormalMap;
        NormalMap,
        // runtime:
        //   new Texture2D(linear = true)
        //   converted(Occlusion + Metallic + Smoothness)
        // extract:
        //   converted(Occlusion + Metallic + Smoothness)
        //   TextureImporter.sRGBTexture = false;
        StandardMap,
        // runtime:
        //   new Texture2D(linear = true)
        // extract:
        //   TextureImporter.sRGBTexture = false;
        Linear,
    }

    public static class TextureImportTypesExtensions
    {
        public static RenderTextureReadWrite GetColorSpace(this TextureImportTypes textureType)
        {
            switch (textureType)
            {
                case TextureImportTypes.sRGB:
                    return RenderTextureReadWrite.sRGB;
                case TextureImportTypes.Linear:
                case TextureImportTypes.StandardMap:
                case TextureImportTypes.NormalMap:
                    return RenderTextureReadWrite.Linear;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
