using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10MToonTextureImporter
    {
        public static IEnumerable<(string key, (SubAssetKey, TextureDescriptor))> EnumerateAllTextures(GltfParser parser, glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            if (TryGetBaseColorTexture(parser, material, out var litTex))
            {
                yield return (MToon.Utils.PropMainTex, litTex);
            }

            if (TryGetEmissiveTexture(parser, material, out var emissiveTex))
            {
                yield return (MToon.Utils.PropEmissionMap, emissiveTex);
            }

            if (TryGetNormalTexture(parser, material, out var normalTex))
            {
                yield return ("_BumpMap", normalTex);
            }

            if (TryGetShadeMultiplyTexture(parser, mToon, out var shadeTex))
            {
                yield return (MToon.Utils.PropShadeTexture, shadeTex);
            }

            if (TryGetShadingShiftTexture(parser, mToon, out var shadeShiftTex))
            {
                Debug.LogWarning("Need VRM 1.0 MToon implementation.");
                yield return ("_NEED_IMPLEMENTATION_MTOON_1_0_shadingShiftTexture", shadeShiftTex);
            }

            if (TryGetMatcapTexture(parser, mToon, out var matcapTex))
            {
                yield return (MToon.Utils.PropSphereAdd, matcapTex);
            }

            if (TryGetRimMultiplyTexture(parser, mToon, out var rimTex))
            {
                yield return (MToon.Utils.PropRimTexture, rimTex);
            }

            if (TryGetOutlineWidthMultiplyTexture(parser, mToon, out var outlineTex))
            {
                yield return (MToon.Utils.PropOutlineWidthTexture, outlineTex);
            }

            if (TryGetUvAnimationMaskTexture(parser, mToon, out var uvAnimMaskTex))
            {
                yield return (MToon.Utils.PropUvAnimMaskTexture, uvAnimMaskTex);
            }
        }

        private static bool TryGetBaseColorTexture(GltfParser parser, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.BaseColorTexture(parser, src);
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

        private static bool TryGetEmissiveTexture(GltfParser parser, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.EmissiveTexture(parser, src);
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

        private static bool TryGetNormalTexture(GltfParser parser, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.NormalTexture(parser, src);
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

        private static bool TryGetShadeMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.ShadeMultiplyTexture), out pair);
        }

        private static bool TryGetShadingShiftTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }

        private static bool TryGetMatcapTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }

        private static bool TryGetRimMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(parser, new Vrm10TextureInfo(mToon.RimMultiplyTexture), out pair);
        }

        private static bool TryGetOutlineWidthMultiplyTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.OutlineWidthMultiplyTexture), out pair);
        }

        private static bool TryGetUvAnimationMaskTexture(GltfParser parser, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(parser, new Vrm10TextureInfo(mToon.UvAnimationMaskTexture), out pair);
        }

        private static bool TryGetSRGBTexture(GltfParser parser, Vrm10TextureInfo info, out (SubAssetKey, TextureDescriptor) pair)
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
        private static bool TryGetLinearTexture(GltfParser parser, Vrm10TextureInfo info, out (SubAssetKey, TextureDescriptor) pair)
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
                return GltfTextureImporter.GetTextureOffsetAndScale(textureTransform);
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
