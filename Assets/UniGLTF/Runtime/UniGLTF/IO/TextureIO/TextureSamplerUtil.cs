using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    public static class TextureSamplerUtil
    {
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
