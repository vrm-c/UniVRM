using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public interface ITextureConverter
    {
        Texture2D GetImportTexture(Texture2D texture);
        Texture2D GetExportTexture(Texture2D texture);
    }

    public static class TextureConverter
    {
        public delegate Color32 ColorConversion(Color32 color);

        public static Texture2D Convert(Texture2D texture, glTFTextureTypes textureType, ColorConversion colorConversion, Material convertMaterial)
        {
            var copyTexture = TextureItem.CopyTexture(texture, TextureIO.GetColorSpace(textureType), convertMaterial);
            if (colorConversion != null)
            {
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => colorConversion(x)).ToArray());
                copyTexture.Apply();
            }
            copyTexture.name = texture.name;
            return copyTexture;
        }

        public static void AppendTextureExtension(Texture texture, string extension)
        {
            if (!texture.name.EndsWith(extension))
            {
                texture.name = texture.name + extension;
            }
        }

        public static void RemoveTextureExtension(Texture texture, string extension)
        {
            if (texture.name.EndsWith(extension))
            {
                texture.name = texture.name.Replace(extension, "");
            }
        }
    }

    class MetallicRoughnessConverter : ITextureConverter
    {
        private const string m_extension = ".metallicRoughness";

        public Texture2D GetImportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Import, null);
            TextureConverter.AppendTextureExtension(converted, m_extension);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Metallic, Export, null);
            TextureConverter.RemoveTextureExtension(converted, m_extension);
            return converted;
        }

        public Color32 Import(Color32 src)
        {
            return new Color32
            {
                r = src.b,
                g = 0,
                b = 0,
                a = (byte)(255 - src.g),
            };
        }

        public Color32 Export(Color32 src)
        {
            return new Color32
            {
                r = 0,
                g = (byte)(255 - src.a),
                b = src.r,
                a = 255,
            };
        }
    }

    class NormalConverter : ITextureConverter
    {
        private const string m_extension = ".normal";

        private Material m_decoder;
        private Material GetDecoder()
        {
            if (m_decoder == null)
            {
                m_decoder = new Material(Shader.Find("UniGLTF/NormalMapDecoder"));
            }
            return m_decoder;
        }

        private Material m_encoder;
        private Material GetEncoder()
        {
            if (m_encoder == null)
            {
                m_encoder = new Material(Shader.Find("UniGLTF/NormalMapEncoder"));
            }
            return m_encoder;
        }

        public Texture2D GetImportTexture(Texture2D texture)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return texture;
#endif
            var mat = GetEncoder();
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Normal, null, mat);
            TextureConverter.AppendTextureExtension(converted, m_extension);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return texture;
#endif
            var mat = GetDecoder();
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Normal, null, mat);
            TextureConverter.RemoveTextureExtension(converted, m_extension);
            return converted;
        }
    }

    class OcclusionConverter : ITextureConverter
    {
        private const string m_extension = ".occlusion";

        public Texture2D GetImportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Occlusion, Import, null);
            TextureConverter.AppendTextureExtension(converted, m_extension);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Occlusion, Export, null);
            TextureConverter.RemoveTextureExtension(converted, m_extension);
            return converted;
        }

        public Color32 Import(Color32 src)
        {
            return new Color32
            {
                r = 0,
                g = src.r,
                b = 0,
                a = 255,
            };
        }

        public Color32 Export(Color32 src)
        {
            return new Color32
            {
                r = src.g,
                g = 0,
                b = 0,
                a = 255,
            };
        }
    }
}