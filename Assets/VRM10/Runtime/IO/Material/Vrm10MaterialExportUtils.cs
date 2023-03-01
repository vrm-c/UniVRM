using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;

namespace UniVRM10
{
    public static class Vrm10MaterialExportUtils
    {
        public static void ExportTextureTransform(glTFTextureInfo textureInfo, Vector2 unityScale, Vector2 unityOffset)
        {
            if (textureInfo == null)
            {
                return;
            }
            var scale = unityScale;
            var offset = new Vector2(unityOffset.x, 1.0f - unityOffset.y - unityScale.y);

            glTF_KHR_texture_transform.Serialize(textureInfo, (offset.x, offset.y), (scale.x, scale.y));
        }

        public static void ExportTextureTransform(TextureInfo textureInfo, Vector2 unityScale, Vector2 unityOffset)
        {
            if (textureInfo == null)
            {
                return;
            }
            // Generate extension to empty holder.
            var gltfTextureInfo = new EmptyGltfTextureInfo();
            ExportTextureTransform(gltfTextureInfo, unityScale, unityOffset);

            // Copy extension from empty holder.
            textureInfo.Extensions = gltfTextureInfo.extensions;
        }

        public static void ExportTextureTransform(ShadingShiftTextureInfo textureInfo, Vector2 unityScale, Vector2 unityOffset)
        {
            if (textureInfo == null)
            {
                return;
            }
            // Generate extension to empty holder.
            var gltfTextureInfo = new EmptyGltfTextureInfo();
            ExportTextureTransform(gltfTextureInfo, unityScale, unityOffset);

            // Copy extension from empty holder.
            textureInfo.Extensions = gltfTextureInfo.extensions;
        }

        private sealed class EmptyGltfTextureInfo : glTFTextureInfo
        {

        }
    }
}