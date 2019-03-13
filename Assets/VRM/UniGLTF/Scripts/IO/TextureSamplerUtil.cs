using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    public static class TextureSamplerUtil
    {
        #region WrapMode
        public enum TextureWrapType
        {
            All,
#if UNITY_2017_1_OR_NEWER
            U,
            V,
            W,
#endif
        }

        public static KeyValuePair<TextureWrapType, TextureWrapMode> TypeWithMode(TextureWrapType type, TextureWrapMode mode)
        {
            return new KeyValuePair<TextureWrapType, TextureWrapMode>(type, mode);
        }

        public static IEnumerable<KeyValuePair<TextureWrapType, TextureWrapMode>> GetUnityWrapMode(VGltf.Types.Sampler sampler)
        {
#if UNITY_2017_1_OR_NEWER
            if (sampler.WrapS == sampler.WrapT)
            {
                switch (sampler.WrapS)
                {
                    case VGltf.Types.Sampler.WrapEnum.ClampToEdge:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Clamp);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.MirroredRepeat:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Mirror);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.Repeat:
                    default:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                        break;
                }
            }
            else
            {
                switch (sampler.WrapS)
                {
                    case VGltf.Types.Sampler.WrapEnum.ClampToEdge:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Clamp);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.MirroredRepeat:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Mirror);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.Repeat:
                    default:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Repeat);
                        break;
                }
                switch (sampler.WrapT)
                {
                    case VGltf.Types.Sampler.WrapEnum.ClampToEdge:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Clamp);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.MirroredRepeat:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Mirror);
                        break;

                    case VGltf.Types.Sampler.WrapEnum.Repeat:
                    default:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Repeat);
                        break;
                }
#else
            // Unity2017.1より前
            // * wrapSとwrapTの区別が無くてwrapしかない
            // * Mirrorが無い

            switch (sampler.WrapS)
            {
                case VGltf.Types.Sampler.WrapEnum.ClampToEdge:
                case VGltf.Types.Sampler.WrapEnum.MirroredRepeat:
                    yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Clamp);
                    break;

                case VGltf.Types.Sampler.WrapEnum.Repeat:
                default:
                    yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                    break;
#endif
            }
        }
        #endregion

        public static FilterMode ImportFilterMode(VGltf.Types.Sampler.MinFilterEnum filterMode)
        {
            switch (filterMode)
            {
                case VGltf.Types.Sampler.MinFilterEnum.NEAREST:
                case VGltf.Types.Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
                case VGltf.Types.Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
                    return FilterMode.Point;


                case VGltf.Types.Sampler.MinFilterEnum.LINEAR:
                case VGltf.Types.Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
                    return FilterMode.Bilinear;

                case VGltf.Types.Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
                    return FilterMode.Trilinear;

                default:
                    throw new NotImplementedException();
            }
        }

        public static void SetSampler(Texture2D texture, VGltf.Types.Sampler sampler)
        {
            if (texture == null)
            {
                return;
            }

            foreach (var kv in GetUnityWrapMode(sampler))
            {
                switch (kv.Key)
                {
                    case TextureWrapType.All:
                        texture.wrapMode = kv.Value;
                        break;

#if UNITY_2017_1_OR_NEWER
                    case TextureWrapType.U:
                        texture.wrapModeU = kv.Value;
                        break;

                    case TextureWrapType.V:
                        texture.wrapModeV = kv.Value;
                        break;

                    case TextureWrapType.W:
                        texture.wrapModeW = kv.Value;
                        break;
#endif

                    default:
                        throw new NotImplementedException();
                }
            }

            texture.filterMode = ImportFilterMode(sampler.MinFilter);
        }

        #region Export
        public static glFilter ExportFilterMode(Texture texture)
        {
            switch (texture.filterMode)
            {
                case FilterMode.Point:
                    return glFilter.NEAREST;

                case FilterMode.Bilinear:
                    return glFilter.LINEAR;

                case FilterMode.Trilinear:
                    return glFilter.LINEAR_MIPMAP_LINEAR;

                default:
                    throw new NotImplementedException();
            }
        }

        public static TextureWrapMode GetWrapS(Texture texture)
        {
#if UNITY_2017_1_OR_NEWER
            return texture.wrapModeU;
#else
            return texture.wrapMode;
#endif
        }

        public static TextureWrapMode GetWrapT(Texture texture)
        {
#if UNITY_2017_1_OR_NEWER
            return texture.wrapModeV;
#else
            return texture.wrapMode;
#endif
        }

        public static glWrap ExportWrapMode(TextureWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case TextureWrapMode.Clamp:
                    return glWrap.CLAMP_TO_EDGE;

                case (TextureWrapMode)(-1):
                case TextureWrapMode.Repeat:
                    return glWrap.REPEAT;

#if UNITY_2017_1_OR_NEWER
                case TextureWrapMode.Mirror:
                case TextureWrapMode.MirrorOnce:
                    return glWrap.MIRRORED_REPEAT;
#endif

                default:
                    throw new NotImplementedException();
            }
        }

        public static glTFTextureSampler Export(Texture texture)
        {
            var filter = ExportFilterMode(texture);
            var wrapS = ExportWrapMode(GetWrapS(texture));
            var wrapT = ExportWrapMode(GetWrapT(texture));
            return new glTFTextureSampler
            {
                magFilter = filter,
                minFilter = filter,
                wrapS = wrapS,
                wrapT = wrapT,
            };
        }
        #endregion
    }
}
