using System;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class TextureSamplerUtil
    {
        #region FilterMode
        // MagFilter は ２種類だけ
        public static glFilter ExportMagFilter(Texture2D texture)
        {
            switch (texture.filterMode)
            {
                case FilterMode.Point:
                    return glFilter.NEAREST;

                case FilterMode.Bilinear:
                case FilterMode.Trilinear:
                    return glFilter.LINEAR;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// MIPMAP: disable, enable, blend
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static glFilter ExportMinFilter(Texture2D texture)
        {
            if (texture.mipmapCount > 1)
            {
                switch (texture.filterMode)
                {
                    // mipmap: enable
                    case FilterMode.Point:
                        return glFilter.NEAREST_MIPMAP_NEAREST;

                    // mipmap: enable
                    case FilterMode.Bilinear:
                        return glFilter.LINEAR_MIPMAP_NEAREST;

                    // mipmap: blend
                    // glFilter.NEAREST_MIPMAP_LINEAR is not exists
                    case FilterMode.Trilinear:
                        return glFilter.LINEAR_MIPMAP_LINEAR;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                // mipmap: disable
                switch (texture.filterMode)
                {
                    case FilterMode.Point:
                        return glFilter.NEAREST;

                    case FilterMode.Bilinear:
                        return glFilter.LINEAR;

                    case FilterMode.Trilinear:
                        // ありえない？
                        return glFilter.LINEAR;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// https://www.khronos.org/registry/OpenGL-Refpages/es2.0/xhtml/glTexParameter.xml
        /// </summary>
        /// <param name="FilterMode"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        public static (FilterMode FilterMode, bool EnableMipMap) ImportFilterMode(glFilter filterMode)
        {
            switch (filterMode)
            {
                // mipmap: disable
                case glFilter.NEAREST:
                    return (FilterMode.Point, false);

                // mipmap: disable
                case glFilter.LINEAR:
                    return (FilterMode.Bilinear, false);

                // mipmap: enable
                case glFilter.NEAREST_MIPMAP_NEAREST:
                    return (FilterMode.Point, true);

                // mipmap: enable
                case glFilter.LINEAR_MIPMAP_NEAREST:
                    return (FilterMode.Bilinear, true);

                // mipmap: blend
                case glFilter.NEAREST_MIPMAP_LINEAR:
                    // not exists in unity.
                    // downgrade mipmap: blend => enable
                    return (FilterMode.Point, true);

                // mipmap: blend
                case glFilter.LINEAR_MIPMAP_LINEAR:
                    return (FilterMode.Trilinear, true);

                // default
                case glFilter.NONE:
                    return (FilterMode.Bilinear, true);

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region WrapMode
        public static glWrap ExportWrapMode(TextureWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case TextureWrapMode.Clamp:
                    return glWrap.CLAMP_TO_EDGE;

                case TextureWrapMode.Mirror:
                case TextureWrapMode.MirrorOnce:
                    return glWrap.MIRRORED_REPEAT;

                // case (TextureWrapMode)(-1):
                // case TextureWrapMode.Repeat:
                default:
                    return glWrap.REPEAT;
            }
        }

        public static TextureWrapMode ImportWrapMode(glWrap wrap)
        {
            switch (wrap)
            {
                case glWrap.NONE: // default
                    return TextureWrapMode.Repeat;

                case glWrap.CLAMP_TO_EDGE:
                    return TextureWrapMode.Clamp;

                case glWrap.REPEAT:
                    return TextureWrapMode.Repeat;

                case glWrap.MIRRORED_REPEAT:
                    return TextureWrapMode.Mirror;

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        public static glTFTextureSampler Export(Texture2D texture)
        {
            var magFilter = ExportMagFilter(texture);
            var minFilter = ExportMinFilter(texture);
            var wrapS = ExportWrapMode(texture.wrapModeU);
            var wrapT = ExportWrapMode(texture.wrapModeV);
            return new glTFTextureSampler
            {
                magFilter = magFilter,
                minFilter = minFilter,
                wrapS = wrapS,
                wrapT = wrapT,
            };
        }

        public static SamplerParam CreateSampler(glTF gltf, int index)
        {
            var gltfTexture = gltf.textures[index];
            if (gltfTexture.sampler < 0 || gltfTexture.sampler >= gltf.samplers.Count)
            {
                // default
                return SamplerParam.Default;
            }

            var gltfSampler = gltf.samplers[gltfTexture.sampler];
            var (filterMode, enableMipMap) = ImportFilterMode(gltfSampler.minFilter);
            return new SamplerParam
            {
                WrapModesU = ImportWrapMode(gltfSampler.wrapS),
                WrapModesV = ImportWrapMode(gltfSampler.wrapT),
                FilterMode = filterMode,
                EnableMipMap = enableMipMap,
            };
        }
    }
}
