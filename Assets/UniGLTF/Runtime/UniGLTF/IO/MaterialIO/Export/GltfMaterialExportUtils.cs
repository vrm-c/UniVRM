using System;
using UnityEngine;

namespace UniGLTF
{
    public static class GltfMaterialExportUtils
    {
        public static void ExportTextureTransform(Material src, glTFTextureInfo dstTextureInfo, string targetTextureName)
        {
            if (src.HasProperty(targetTextureName))
            {
                var offset = src.GetTextureOffset(targetTextureName);
                var scale = src.GetTextureScale(targetTextureName);

                ExportTextureTransform(offset, scale, dstTextureInfo);
            }
        }

        public static void ExportTextureTransform(Vector2 offset, Vector2 scale, glTFTextureInfo dstTextureInfo)
        {
            if (dstTextureInfo != null)
            {
                (scale, offset) = TextureTransform.VerticalFlipScaleOffset(scale, offset);

                glTF_KHR_texture_transform.Serialize(dstTextureInfo, (offset.x, offset.y), (scale.x, scale.y));
            }
        }
    }
}