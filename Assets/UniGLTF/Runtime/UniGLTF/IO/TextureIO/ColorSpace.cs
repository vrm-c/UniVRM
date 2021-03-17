using System;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class ColorSpace
    {
        public static RenderTextureReadWrite GetColorSpace(this glTFTextureTypes textureType)
        {
            switch (textureType)
            {
                case glTFTextureTypes.SRGB:
                    return RenderTextureReadWrite.sRGB;
                case glTFTextureTypes.OcclusionMetallicRoughness:
                case glTFTextureTypes.Normal:
                    return RenderTextureReadWrite.Linear;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool TryGetglTFTextureType(this glTF glTf, int textureIndex, out glTFTextureTypes textureType)
        {
            foreach (var material in glTf.materials)
            {
                var textureInfo = material.GetTextures().FirstOrDefault(x => (x != null) && x.index == textureIndex);
                if (textureInfo != null)
                {
                    textureType = textureInfo.TextureType;
                    return true;
                }
            }

            // textureIndex is not used by Material.
            textureType = default;
            return false;
        }

        public static RenderTextureReadWrite GetColorSpace(this glTF gltf, int textureIndex)
        {
            if (TryGetglTFTextureType(gltf, textureIndex, out glTFTextureTypes textureType))
            {
                return GetColorSpace(textureType);
            }
            else
            {
                return RenderTextureReadWrite.sRGB;
            }
        }
    }
}
