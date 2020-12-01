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

        public static IEnumerable<KeyValuePair<TextureWrapType, TextureWrapMode>> GetUnityWrapMode(glTFTextureSampler sampler)
        {
#if UNITY_2017_1_OR_NEWER
            if (sampler.wrapS == sampler.wrapT)
            {
                switch (sampler.wrapS)
                {
                    case glWrap.NONE: // default
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (sampler.wrapS)
                {
                    case glWrap.NONE: // default
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return TypeWithMode(TextureWrapType.U, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                switch (sampler.wrapT)
                {
                    case glWrap.NONE: // default
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return TypeWithMode(TextureWrapType.V, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
#else
            // Unity2017.1より前
            // * wrapSとwrapTの区別が無くてwrapしかない
            // * Mirrorが無い

            switch (sampler.wrapS)
            {
                case glWrap.NONE: // default
                    yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                    break;

                case glWrap.CLAMP_TO_EDGE:
                case glWrap.MIRRORED_REPEAT:
                    yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Clamp);
                    break;

                case glWrap.REPEAT:
                    yield return TypeWithMode(TextureWrapType.All, TextureWrapMode.Repeat);
                    break;

                default:
                    throw new NotImplementedException();
#endif
            }
        }
        #endregion

        public static FilterMode ImportFilterMode(glFilter filterMode)
        {
            switch (filterMode)
            {
                case glFilter.NEAREST:
                case glFilter.NEAREST_MIPMAP_LINEAR:
                case glFilter.NEAREST_MIPMAP_NEAREST:
                    return FilterMode.Point;

                case glFilter.NONE:
                case glFilter.LINEAR:
                case glFilter.LINEAR_MIPMAP_NEAREST:
                    return FilterMode.Bilinear;

                case glFilter.LINEAR_MIPMAP_LINEAR:
                    return FilterMode.Trilinear;

                default:
                    throw new NotImplementedException();

            }
        }

        public static void SetSampler(Texture2D texture, glTFTextureSampler sampler)
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

            texture.filterMode = ImportFilterMode(sampler.minFilter);
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
