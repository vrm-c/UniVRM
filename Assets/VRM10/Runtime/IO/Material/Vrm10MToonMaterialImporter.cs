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
            catch (ArgumentOutOfRangeException)
            {
                pair = default;
                return false;
            }
        }
        
        public static bool TryGetNormalTexture(GltfParser parser, glTFMaterial src, out (SubAssetKey, TextureImportParam) pair)
        {
            try
            {
                pair = GltfPBRMaterial.NormalTexture(parser, src);
                return true;
            }
            catch (NullReferenceException)
            {
                pair = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                pair = default;
                return false;
            }
        }

        public static bool TryGetShadeMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.ShadeMultiplyTexture), out pair);
        }

        public static bool TryGetShadingShiftTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }
        
        public static bool TryGetMatcapTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }
        
        public static bool TryGetRimMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.RimMultiplyTexture), out pair);
        }

        public static bool TryGetOutlineWidthMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.OutlineWidthMultiplyTexture), out pair);
        }

        public static bool TryGetUvAnimationMaskTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureImportParam) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.UvAnimationMaskTexture), out pair);
        }

        private static bool TryGetSRGBTexture(GltfParser parser, Vrm10TextureInfo info, out (SubAssetKey, TextureImportParam) pair)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                pair = GltfTextureImporter.CreateSRGB(parser, info.index, offset, scale);
                return true;
            }
            catch (NullReferenceException)
            {
                pair = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                pair = default;
                return false;
            }
        }
        private static bool TryGetLinearTexture(GltfParser parser, Vrm10TextureInfo info, out (SubAssetKey, TextureImportParam) pair)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                pair = GltfTextureImporter.CreateLinear(parser, info.index, offset, scale);
                return true;
            }
            catch (NullReferenceException)
            {
                pair = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                pair = default;
                return false;
            }
        }

        private static (Vector2, Vector2) GetTextureOffsetAndScale(Vrm10TextureInfo textureInfo)
        {
            if (glTF_KHR_texture_transform.TryGet(textureInfo, out var textureTransform))
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
                if (info == null) return;
                
                index = info.Index ?? -1;
                texCoord = info.TexCoord ?? -1;
                extensions = info.Extensions as glTFExtension;
                extras = info.Extras as glTFExtension;
            }

            public Vrm10TextureInfo(ShadingShiftTextureInfo info)
            {
                if (info == null) return;
                
                index = info.Index ?? -1;
                texCoord = info.TexCoord ?? -1;
                extensions = info.Extensions as glTFExtension;
                extras = info.Extras as glTFExtension;
            }
        }
    }
}