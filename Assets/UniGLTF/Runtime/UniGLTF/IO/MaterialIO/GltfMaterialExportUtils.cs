using System;
using UnityEngine;

namespace UniGLTF
{
    public static class GltfMaterialExportUtils
    {
        public static void ExportTextureTransform(Material src, glTFTextureInfo dstTextureInfo, string targetTextureName)
        {
            if (dstTextureInfo != null && src.HasProperty(targetTextureName))
            {
                var offset = src.GetTextureOffset(targetTextureName);
                var scale = src.GetTextureScale(targetTextureName);
                (scale, offset) = TextureTransform.VerticalFlipScaleOffset(scale, offset);

                glTF_KHR_texture_transform.Serialize(dstTextureInfo, (offset.x, offset.y), (scale.x, scale.y));
            }
        }
    }
}