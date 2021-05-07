using System;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10MToonMaterialImporter
    {
        public static bool TryGetBaseColorTexture(GltfParser parser, glTFMaterial src, out (SubAssetKey, TextureImportParam) pair)
        {
            try
            {
                pair = GltfPBRMaterial.BaseColorTexture(parser, src);
                return true;
            }
            catch (NullReferenceException)
            {
                pair = default;
                return false;
            }
        }

        public static bool TryGetShadeMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(mToon.ShadeMultiplyTexture);
                pair = GltfTextureImporter.CreateSRGB(parser, mToon.ShadeMultiplyTexture.Index ?? -1, offset, scale);
                return true;
            }
            catch (NullReferenceException)
            {
                pair = default;
                return false;
            }
        }

        private static (Vector2, Vector2) GetTextureOffsetAndScale(TextureInfo textureInfo)
        {
            if (glTF_KHR_texture_transform.TryGet(new Vrm10TextureInfo(textureInfo), out var textureTransform))
            {
                return GltfMaterialImporter.GetTextureOffsetAndScale(textureTransform);
            }
            return (new Vector2(0, 0), new Vector2(1, 1));
        }

        /// <summary>
        /// MToon 定義内の TextureInfo を glTFTextureInfo として扱う入れ物
        /// </summary>
        private sealed class Vrm10TextureInfo : glTFTextureInfo
        {
            public Vrm10TextureInfo(TextureInfo info)
            {
                index = info.Index ?? -1;
                texCoord = info.TexCoord ?? -1;
                extensions = info.Extensions as glTFExtension;
                extras = info.Extras as glTFExtension;
            }
        }
    }
}