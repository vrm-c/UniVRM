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
        /// もっとも根本の Exporter クラスのみが呼び出すべきである。
        /// 他の拡張機能などが呼び出すべきではない。
        ///
        /// </summary>
        /// <returns>gltf texture index</returns>
        public static int PushGltfTexture(ExportingGltfData data, Texture2D texture, ColorSpace textureColorSpace, ITextureSerializer textureSerializer)
        {
            var bytesWithMime = textureSerializer.ExportBytesWithMime(texture, textureColorSpace);

            // add view
            var viewIndex = data.ExtendBufferAndGetViewIndex(bytesWithMime.bytes);

            // add image
            var imageIndex = data.GLTF.images.Count;
            data.GLTF.images.Add(new glTFImage
            {
                name = TextureImportName.RemoveSuffix(texture.name),
                bufferView = viewIndex,
                mimeType = bytesWithMime.mime,
            });

            // add sampler
            var samplerIndex = data.GLTF.samplers.Count;
            var sampler = TextureSamplerUtil.Export(texture);
            data.GLTF.samplers.Add(sampler);

            // add texture
            var textureIndex = data.GLTF.textures.Count;
            data.GLTF.textures.Add(new glTFTexture
            {
                sampler = samplerIndex,
                source = imageIndex,
            });

            return textureIndex;
        }
    }
}
