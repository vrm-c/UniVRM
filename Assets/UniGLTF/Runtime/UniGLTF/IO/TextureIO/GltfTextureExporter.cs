using System;
using UnityEngine;
using VRMShaders;

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
        public static int PushGltfTexture(this glTF gltf, int bufferIndex, Texture2D texture, Func<Texture2D, (byte[] bytes, string mime)> getTextureBytes)
        {
            var bytesWithMime = getTextureBytes(texture);

            // add view
            var view = gltf.buffers[bufferIndex].Append(bytesWithMime.bytes, glBufferTarget.NONE);
            var viewIndex = gltf.AddBufferView(view);

            // add image
            var imageIndex = gltf.images.Count;
            gltf.images.Add(new glTFImage
            {
                name = TextureImportName.RemoveSuffix(texture.name),
                bufferView = viewIndex,
                mimeType = bytesWithMime.mime,
            });

            // add sampler
            var samplerIndex = gltf.samplers.Count;
            var sampler = TextureSamplerUtil.Export(texture);
            gltf.samplers.Add(sampler);

            // add texture
            var textureIndex = gltf.textures.Count;
            gltf.textures.Add(new glTFTexture
            {
                sampler = samplerIndex,
                source = imageIndex,
            });

            return textureIndex;
        }
    }
}
