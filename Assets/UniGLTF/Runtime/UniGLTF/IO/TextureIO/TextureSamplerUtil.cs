using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    public static class TextureSamplerUtil
    {
        #region Export
        // MagFilter は ２種類だけ
        public static glFilter ExportMagFilter(Texture texture)
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

        public static glFilter ExportMinFilter(Texture texture)
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

        public static glTFTextureSampler Export(Texture texture)
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
        #endregion
    }
}
