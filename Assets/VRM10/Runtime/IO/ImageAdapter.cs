using System;
using UniGLTF;

namespace UniVRM10
{
    public static class ImageAdapter
    {
        public static VrmLib.Image FromGltf(this glTFImage x, Vrm10Storage storage)
        {
            if (x.bufferView == -1)
            {
                // 外部参照？
                throw new Exception();
            }

            var view = storage.Gltf.bufferViews[x.bufferView];

            var buffer = storage.Gltf.buffers[view.buffer];

            // テクスチャの用途を調べる
            var usage = default(VrmLib.ImageUsage);
            foreach (var material in storage.Gltf.materials)
            {
                var colorImage = GetColorImage(storage, material);
                if (colorImage == x)
                {
                    usage |= VrmLib.ImageUsage.Color;
                }

                var normalImage = GetNormalImage(storage, material);
                if (normalImage == x)
                {
                    usage |= VrmLib.ImageUsage.Normal;
                }
            }

            var memory = storage.GetBufferBytes(buffer);
            return new VrmLib.Image(x.name,
                x.mimeType,
                usage,
                memory.Slice(view.byteOffset, view.byteLength));
        }

        static glTFImage GetTexture(Vrm10Storage storage, int index)
        {
            if (index < 0 || index >= storage.Gltf.textures.Count)
            {
                return null;
            }
            var texture = storage.Gltf.textures[index];
            if (texture.source < 0 || texture.source >= storage.Gltf.images.Count)
            {
                return null;
            }
            return storage.Gltf.images[texture.source];
        }

        static glTFImage GetColorImage(Vrm10Storage storage, glTFMaterial m)
        {
            if (m.pbrMetallicRoughness == null)
            {
                return null;
            }
            if (m.pbrMetallicRoughness.baseColorTexture == null)
            {
                return null;
            }
            if (!m.pbrMetallicRoughness.baseColorTexture.index.TryGetValidIndex(storage.TextureCount, out int index))
            {
                return null;
            }
            return GetTexture(storage, index);
        }

        static glTFImage GetNormalImage(Vrm10Storage storage, glTFMaterial m)
        {
            if (m.normalTexture == null)
            {
                return null;
            }
            if (!m.normalTexture.index.TryGetValidIndex(storage.TextureCount, out int index))
            {
                return null;
            }
            return GetTexture(storage, index);
        }

        public static glTFImage ToGltf(this VrmLib.Image src, Vrm10Storage storage)
        {
            var viewIndex = storage.AppendToBuffer(0, src.Bytes, 1);
            var gltf = storage.Gltf;
            return new glTFImage
            {
                name = src.Name,
                mimeType = src.MimeType,
                bufferView = viewIndex,
            };
        }
    }
}