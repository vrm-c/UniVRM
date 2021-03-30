using VrmLib;
using System;
using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10
{
    public static class TextureAdapter
    {
        public static glTFTextureSampler ToGltf(this TextureSampler src)
        {
            return new glTFTextureSampler
            {
                wrapS = (glWrap)src.WrapS,
                wrapT = (glWrap)src.WrapT,
                minFilter = (glFilter)src.MinFilter,
                magFilter = (glFilter)src.MagFilter,
            };
        }
    }
}
