using System;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{

    public class TextureIO
    {
        public static RenderTextureReadWrite GetColorSpace(glTFTextureTypes textureType)
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

        public static RenderTextureReadWrite GetColorSpace(glTF gltf, int textureIndex)
        {
            if (TextureIO.TryGetglTFTextureType(gltf, textureIndex, out glTFTextureTypes textureType))
            {
                return GetColorSpace(textureType);
            }
            else
            {
                return RenderTextureReadWrite.sRGB;
            }
        }

        public static bool TryGetglTFTextureType(glTF glTf, int textureIndex, out glTFTextureTypes textureType)
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

        static (Byte[] bytes, string mine) GetBytesWithMime(Texture2D texture)
        {
#if UNITY_EDITOR
            var path = UnityPath.FromAsset(texture);
            if (path.IsUnderAssetsFolder)
            {
                if (path.Extension == ".png")
                {
                    return
                    (
                        System.IO.File.ReadAllBytes(path.FullPath),
                        "image/png"
                    );
                }
                if (path.Extension == ".jpg")
                {
                    return
                    (
                        System.IO.File.ReadAllBytes(path.FullPath),
                        "image/jpeg"
                    );
                }
            }
#endif

            return
            (
                texture.EncodeToPNG(),
                "image/png"
            );
        }

        static public int ExportTexture(glTF gltf, int bufferIndex, Texture2D texture)
        {
            var bytesWithMime = GetBytesWithMime(texture);

            // add view
            var view = gltf.buffers[bufferIndex].Append(bytesWithMime.bytes, glBufferTarget.NONE);
            var viewIndex = gltf.AddBufferView(view);

            // add image
            var imageIndex = gltf.images.Count;
            gltf.images.Add(new glTFImage
            {
                name = GetTextureParam.RemoveSuffix(texture.name),
                bufferView = viewIndex,
                mimeType = bytesWithMime.mine,
            });

            // add sampler
            var samplerIndex = gltf.samplers.Count;
            var sampler = TextureSamplerUtil.Export(texture);
            gltf.samplers.Add(sampler);

            // add texture
            gltf.textures.Add(new glTFTexture
            {
                sampler = samplerIndex,
                source = imageIndex,
            });

            return imageIndex;
        }
    }
}
