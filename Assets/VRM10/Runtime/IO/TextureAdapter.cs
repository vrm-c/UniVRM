using VrmLib;
using System;
using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10
{
    public static class TextureAdapter
    {
        public static ImageTexture FromGltf(this glTFTexture x, glTFTextureSampler sampler, List<Image> images, Texture.ColorSpaceTypes colorSpace, Texture.TextureTypes textureType)
        {
            var image = images[x.source];
            var name = !string.IsNullOrEmpty(x.name) ? x.name : image.Name;
            return new ImageTexture(x.name, sampler.FromGltf(), image, colorSpace, textureType);
        }

        public static TextureSampler FromGltf(this glTFTextureSampler sampler)
        {
            return new TextureSampler
            {
                WrapS = (TextureWrapType)sampler.wrapS,
                WrapT = (TextureWrapType)sampler.wrapT,
                MinFilter = (TextureMinFilterType)sampler.minFilter,
                MagFilter = (TextureMagFilterType)sampler.magFilter,
            };
        }

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