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

    public class TextureIO : ITextureExporter
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

        public static glTFTextureTypes GetglTFTextureType(string shaderName, string propName)
        {
            switch (propName)
            {
                case "_MetallicGlossMap":
                case "_OcclusionMap":
                    return glTFTextureTypes.OcclusionMetallicRoughness;
                case "_BumpMap":
                    return glTFTextureTypes.Normal;
                case "_Color":
                case "_EmissionMap":
                    return glTFTextureTypes.SRGB;
                default:
                    Debug.LogWarning($"unknown texture property: {propName} as sRGB");
                    return glTFTextureTypes.SRGB;
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

        public virtual IEnumerable<(Texture texture, glTFTextureTypes textureType)> GetTextures(Material m)
        {
            var props = ShaderPropExporter.PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            if (props == null)
            {
                // unknown shader
                yield return (m.mainTexture, glTFTextureTypes.SRGB);
            }

            foreach (var prop in props.Properties)
            {

                if (prop.ShaderPropertyType == ShaderPropExporter.ShaderPropertyType.TexEnv)
                {
                    yield return (m.GetTexture(prop.Key), GetglTFTextureType(m.shader.name, prop.Key));
                }
            }
        }

        public virtual (Byte[] bytes, string mine) GetBytesWithMime(Texture texture, glTFTextureTypes textureType)
        {
#if UNITY_EDITOR
            var path = UnityPath.FromAsset(texture);
            if (path.IsUnderAssetsFolder)
            {
                var textureImporter = AssetImporter.GetAtPath(path.Value) as TextureImporter;
                var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                if (textureImporter != null && getSizeMethod != null)
                {
                    var args = new object[2] { 0, 0 };
                    getSizeMethod.Invoke(textureImporter, args);
                    var originalWidth = (int)args[0];
                    var originalHeight = (int)args[1];

                    var originalSize = Mathf.Max(originalWidth, originalHeight);
                    var requiredMaxSize = textureImporter.maxTextureSize;

                    // Resized exporting if MaxSize setting value is smaller than original image size.
                    if (originalSize > requiredMaxSize)
                    {
                        return
                        (
                            TextureConverter.CopyTexture(texture, GetColorSpace(textureType), null).EncodeToPNG(),
                            "image/png"
                        );
                    }
                }

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
                TextureConverter.CopyTexture(texture, TextureIO.GetColorSpace(textureType), null).EncodeToPNG(),
                "image/png"
            );
        }

        public int ExportTexture(glTF gltf, int bufferIndex, Texture texture, glTFTextureTypes textureType)
        {
            var bytesWithMime = GetBytesWithMime(texture, textureType); ;

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
