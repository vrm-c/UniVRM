using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRMShaders;
using VRMShaders.VRM10.MToon10.Runtime;

namespace UniVRM10
{
    public static class Vrm10MToonTextureImporter
    {
        public static IEnumerable<(string key, (SubAssetKey, TextureDescriptor))> EnumerateAllTextures(IGltfData data, glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            if (TryGetBaseColorTexture(data, material, out var litTex))
            {
                yield return (MToon10Prop.BaseColorTexture.ToUnityShaderLabName(), litTex);
            }

            if (TryGetEmissiveTexture(data, material, out var emissiveTex))
            {
                yield return (MToon10Prop.EmissiveTexture.ToUnityShaderLabName(), emissiveTex);
            }

            if (TryGetNormalTexture(data, material, out var normalTex))
            {
                yield return (MToon10Prop.NormalTexture.ToUnityShaderLabName(), normalTex);
            }

            if (TryGetShadeMultiplyTexture(data, mToon, out var shadeTex))
            {
                yield return (MToon10Prop.ShadeColorTexture.ToUnityShaderLabName(), shadeTex);
            }

            if (TryGetShadingShiftTexture(data, mToon, out var shadeShiftTex))
            {
                yield return (MToon10Prop.ShadingShiftTexture.ToUnityShaderLabName(), shadeShiftTex);
            }

            if (TryGetMatcapTexture(data, mToon, out var matcapTex))
            {
                yield return (MToon10Prop.MatcapTexture.ToUnityShaderLabName(), matcapTex);
            }

            if (TryGetRimMultiplyTexture(data, mToon, out var rimTex))
            {
                yield return (MToon10Prop.RimMultiplyTexture.ToUnityShaderLabName(), rimTex);
            }

            if (TryGetOutlineWidthMultiplyTexture(data, mToon, out var outlineTex))
            {
                yield return (MToon10Prop.OutlineWidthMultiplyTexture.ToUnityShaderLabName(), outlineTex);
            }

            if (TryGetUvAnimationMaskTexture(data, mToon, out var uvAnimMaskTex))
            {
                yield return (MToon10Prop.UvAnimationMaskTexture.ToUnityShaderLabName(), uvAnimMaskTex);
            }
        }

        private static bool TryGetBaseColorTexture(IGltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.BaseColorTexture(data, src);
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

        private static bool TryGetEmissiveTexture(IGltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.EmissiveTexture(data, src);
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

        private static bool TryGetNormalTexture(IGltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                pair = GltfPbrTextureImporter.NormalTexture(data, src);
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

        private static bool TryGetShadeMultiplyTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.ShadeMultiplyTexture), out pair);
        }

        private static bool TryGetShadingShiftTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }

        private static bool TryGetMatcapTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out pair);
        }

        private static bool TryGetRimMultiplyTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.RimMultiplyTexture), out pair);
        }

        private static bool TryGetOutlineWidthMultiplyTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.OutlineWidthMultiplyTexture), out pair);
        }

        private static bool TryGetUvAnimationMaskTexture(IGltfData data, VRMC_materials_mtoon mToon, out (SubAssetKey, TextureDescriptor) pair)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.UvAnimationMaskTexture), out pair);
        }

        private static bool TryGetSRGBTexture(IGltfData data, Vrm10TextureInfo info, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                pair = GltfTextureImporter.CreateSRGB(data, info.index, offset, scale);
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
        private static bool TryGetLinearTexture(IGltfData data, Vrm10TextureInfo info, out (SubAssetKey, TextureDescriptor) pair)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                pair = GltfTextureImporter.CreateLinear(data, info.index, offset, scale);
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
