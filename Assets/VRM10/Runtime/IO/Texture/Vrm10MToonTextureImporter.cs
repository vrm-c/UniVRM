using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRM10.MToon10;

namespace UniVRM10
{
    public static class Vrm10MToonTextureImporter
    {
        public static IEnumerable<(string key, (SubAssetKey, TextureDescriptor))> EnumerateAllTextures(GltfData data, glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            if (TryGetBaseColorTexture(data, material, out var key, out var desc))
            {
                yield return (MToon10Prop.BaseColorTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetEmissiveTexture(data, material, out key, out desc))
            {
                yield return (MToon10Prop.EmissiveTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetNormalTexture(data, material, out key, out desc))
            {
                yield return (MToon10Prop.NormalTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetShadeMultiplyTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.ShadeColorTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetShadingShiftTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.ShadingShiftTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetMatcapTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.MatcapTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetRimMultiplyTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.RimMultiplyTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetOutlineWidthMultiplyTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.OutlineWidthMultiplyTexture.ToUnityShaderLabName(), (key, desc));
            }

            if (TryGetUvAnimationMaskTexture(data, mToon, out key, out desc))
            {
                yield return (MToon10Prop.UvAnimationMaskTexture.ToUnityShaderLabName(), (key, desc));
            }
        }

        private static bool TryGetBaseColorTexture(GltfData data, glTFMaterial src, out SubAssetKey key, out TextureDescriptor desc)
        {
            try
            {
                return GltfPbrTextureImporter.TryBaseColorTexture(data, src, out key, out desc);
            }
            catch (NullReferenceException)
            {
                key = default;
                desc = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                key = default;
                desc = default;
                return false;
            }
        }

        private static bool TryGetEmissiveTexture(GltfData data, glTFMaterial src, out SubAssetKey key, out TextureDescriptor desc)
        {
            try
            {
                return GltfPbrTextureImporter.TryEmissiveTexture(data, src, out key, out desc);
            }
            catch (NullReferenceException)
            {
                key = default;
                desc = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                key = default;
                desc = default;
                return false;
            }

        }

        private static bool TryGetNormalTexture(GltfData data, glTFMaterial src, out SubAssetKey key, out TextureDescriptor desc)
        {
            try
            {
                return GltfPbrTextureImporter.TryNormalTexture(data, src, out key, out desc);
            }
            catch (NullReferenceException)
            {
                key = default;
                desc = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                key = default;
                desc = default;
                return false;
            }
        }

        private static bool TryGetShadeMultiplyTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.ShadeMultiplyTexture), out key, out desc);
        }

        private static bool TryGetShadingShiftTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.ShadingShiftTexture), out key, out desc);
        }

        private static bool TryGetMatcapTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.MatcapTexture), out key, out desc);
        }

        private static bool TryGetRimMultiplyTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetSRGBTexture(data, new Vrm10TextureInfo(mToon.RimMultiplyTexture), out key, out desc);
        }

        private static bool TryGetOutlineWidthMultiplyTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.OutlineWidthMultiplyTexture), out key, out desc);
        }

        private static bool TryGetUvAnimationMaskTexture(GltfData data, VRMC_materials_mtoon mToon, out SubAssetKey key, out TextureDescriptor desc)
        {
            return TryGetLinearTexture(data, new Vrm10TextureInfo(mToon.UvAnimationMaskTexture), out key, out desc);
        }

        private static bool TryGetSRGBTexture(GltfData data, Vrm10TextureInfo info, out SubAssetKey key, out TextureDescriptor desc)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                return GltfTextureImporter.TryCreateSrgb(data, info.index, offset, scale, out key, out desc);
            }
            catch (NullReferenceException)
            {
                key = default;
                desc = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                key = default;
                desc = default;
                return false;
            }
        }
        private static bool TryGetLinearTexture(GltfData data, Vrm10TextureInfo info, out SubAssetKey key, out TextureDescriptor desc)
        {
            try
            {
                var (offset, scale) = GetTextureOffsetAndScale(info);
                return GltfTextureImporter.TryCreateLinear(data, info.index, offset, scale, out key, out desc);
            }
            catch (NullReferenceException)
            {
                key = default;
                desc = default;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                key = default;
                desc = default;
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
