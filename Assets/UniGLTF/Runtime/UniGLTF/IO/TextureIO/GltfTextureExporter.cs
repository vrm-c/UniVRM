using System;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    public static class GltfTextureExporter
    {
        /// <summary>
        /// gltf に texture を足す
        ///
        /// * textures
        /// * samplers
        /// * images
        /// * bufferViews
        ///
        /// を更新し、textures の index を返す
        ///
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="bufferIndex"></param>
        /// <param name="texture"></param>
        /// <returns>gltf texture index</returns>
        public static int PushGltfTexture(GltfBufferWriter w, Texture2D texture, ColorSpace textureColorSpace, ITextureSerializer textureSerializer)
        {
            var bytesWithMime = textureSerializer.ExportBytesWithMime(texture, textureColorSpace);

            // add view
            var viewIndex = w.ExtendBufferAndGetViewIndex(bytesWithMime.bytes);

            // add image
            var imageIndex = w.GLTF.images.Count;
            w.GLTF.images.Add(new glTFImage
            {
                name = TextureImportName.RemoveSuffix(texture.name),
                bufferView = viewIndex,
                mimeType = bytesWithMime.mime,
            });

            // add sampler
            var samplerIndex = w.GLTF.samplers.Count;
            var sampler = TextureSamplerUtil.Export(texture);
            w.GLTF.samplers.Add(sampler);

            // add texture
            var textureIndex = w.GLTF.textures.Count;
            w.GLTF.textures.Add(new glTFTexture
            {
                sampler = samplerIndex,
                source = imageIndex,
            });

            return textureIndex;
        }
    }
}
