using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif


namespace UniGLTF
{
    public static class TextureIO
    {
        public static RenderTextureReadWrite GetColorSpace(glTFTextureTypes textureType)
        {
            switch (textureType)
            {
                case glTFTextureTypes.Metallic:
                case glTFTextureTypes.Normal:
                case glTFTextureTypes.Occlusion:
                    return RenderTextureReadWrite.Linear;
                case glTFTextureTypes.BaseColor:
                case glTFTextureTypes.Emissive:
                    return RenderTextureReadWrite.sRGB;
                default:
                    return RenderTextureReadWrite.sRGB;
            }
        }

        public static glTFTextureTypes GetglTFTextureType(string shaderName, string propName)
        {
            switch (propName)
            {
                case "_Color":
                    return glTFTextureTypes.BaseColor;
                case "_MetallicGlossMap":
                    return glTFTextureTypes.Metallic;
                case "_BumpMap":
                    return glTFTextureTypes.Normal;
                case "_OcclusionMap":
                    return glTFTextureTypes.Occlusion;
                case "_EmissionMap":
                    return glTFTextureTypes.Emissive;
                default:
                    return glTFTextureTypes.Unknown;
            }
        }

        public static glTFTextureTypes GetglTFTextureType(glTF glTf, int textureIndex)
        {
            foreach (var material in glTf.materials)
            {
                var textureInfo = material.GetTextures().FirstOrDefault(x => (x!=null) && x.index == textureIndex);
                if (textureInfo != null)
                {
                    return textureInfo.TextureType;
                }
            }
            return glTFTextureTypes.Unknown;
        }

#if UNITY_EDITOR
        public static void MarkTextureAssetAsNormalMap(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (null == textureImporter)
            {
                return;
            }

            //Debug.LogFormat("[MarkTextureAssetAsNormalMap] {0}", assetPath);
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.SaveAndReimport();
        }
#endif

        public struct TextureExportItem
        {
            public Texture Texture;
            public glTFTextureTypes TextureType;

            public TextureExportItem(Texture texture, glTFTextureTypes textureType)
            {
                Texture = texture;
                TextureType = textureType;
            }
        }

        public static IEnumerable<TextureExportItem> GetTextures(Material m)
        {
            var props = ShaderPropExporter.PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            if (props == null)
            {
                yield return new TextureExportItem(m.mainTexture, glTFTextureTypes.BaseColor);
            }

            foreach (var prop in props.Properties)
            {

                if (prop.ShaderPropertyType == ShaderPropExporter.ShaderPropertyType.TexEnv)
                {
                    yield return new TextureExportItem(m.GetTexture(prop.Key), GetglTFTextureType(m.shader.name, prop.Key));
                }
            }
        }


        struct BytesWithMime
        {
            public Byte[] Bytes;
            public string Mime;
        }

        static BytesWithMime GetBytesWithMime(Texture texture, glTFTextureTypes textureType)
        {
#if UNITY_EDITOR
            var path = UnityPath.FromAsset(texture);
            if (path.IsUnderAssetsFolder)
            {
                var textureImporter = (TextureImporter) AssetImporter.GetAtPath(path.Value);
                var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                if (textureImporter != null && getSizeMethod != null)
                {
                    var args = new object[2] {0, 0};
                    getSizeMethod.Invoke(textureImporter, args);
                    var originalWidth = (int) args[0];
                    var originalHeight = (int) args[1];

                    var originalSize = Mathf.Max(originalWidth, originalHeight);
                    var requiredMaxSize = textureImporter.maxTextureSize;

                    // Resized exporting if MaxSize setting value is smaller than original image size.
                    if (originalSize > requiredMaxSize)
                    {
                        return new BytesWithMime
                        {
                            Bytes = TextureItem.CopyTexture(texture, GetColorSpace(textureType), null).EncodeToPNG(),
                            Mime = "image/png",
                        };
                    }
                }
                
                if (path.Extension == ".png")
                {
                    return new BytesWithMime
                    {
                        Bytes = System.IO.File.ReadAllBytes(path.FullPath),
                        Mime = "image/png",
                    };
                }
                if (path.Extension == ".jpg")
                {
                    return new BytesWithMime
                    {
                        Bytes = System.IO.File.ReadAllBytes(path.FullPath),
                        Mime = "image/jpeg",
                    };
                }
            }
#endif

            return new BytesWithMime
            {
                Bytes = TextureItem.CopyTexture(texture, TextureIO.GetColorSpace(textureType), null).EncodeToPNG(),
                Mime = "image/png",
            };
        }

        public static int ExportTexture(glTF gltf, int bufferIndex, Texture texture, glTFTextureTypes textureType)
        {
            var bytesWithMime = GetBytesWithMime(texture, textureType); ;

            // add view
            var view = gltf.buffers[bufferIndex].Append(bytesWithMime.Bytes, glBufferTarget.NONE);
            var viewIndex = gltf.AddBufferView(view);

            // add image
            var imageIndex = gltf.images.Count;
            gltf.images.Add(new glTFImage
            {
                name = texture.name,
                bufferView = viewIndex,
                mimeType = bytesWithMime.Mime,
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
