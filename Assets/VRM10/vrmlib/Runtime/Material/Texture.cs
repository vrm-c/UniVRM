using System;
using System.Numerics;

namespace VrmLib
{
    public enum TextureMagFilterType : int
    {
        NEAREST = 9728,
        LINEAR = 9729
    }

    public enum TextureMinFilterType : int
    {
        NEAREST = 9728,
        LINEAR = 9729,

        NEAREST_MIPMAP_NEAREST = 9984,
        LINEAR_MIPMAP_NEAREST = 9985,
        NEAREST_MIPMAP_LINEAR = 9986,
        LINEAR_MIPMAP_LINEAR = 9987,
    }

    public enum TextureWrapType : int
    {
        REPEAT = 10497,
        CLAMP_TO_EDGE = 33071,
        MIRRORED_REPEAT = 33648,
    }

    public class TextureSampler
    {
        public TextureWrapType WrapS;
        public TextureWrapType WrapT;

        public TextureMinFilterType MinFilter;
        public TextureMagFilterType MagFilter;
    }

    public abstract class Texture: GltfId
    {
        public enum TextureTypes
        {
            Default,
            NormalMap,
            MetallicRoughness,
            Emissive,
            Occlusion
        };

        public enum ColorSpaceTypes
        {
            Srgb,
            Linear,
        };

        public string Name;
        public TextureSampler Sampler;
        public ColorSpaceTypes ColorSpace;
        public TextureTypes TextureType;

        protected Texture(string name, TextureSampler sampler, ColorSpaceTypes colorSpace, TextureTypes textureType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            Sampler = sampler;
            ColorSpace = colorSpace;
            TextureType = textureType;
        }
    }

    public class ImageTexture : Texture, IEquatable<ImageTexture>
    {
        public Image Image;

        public ImageTexture(string name, TextureSampler sampler, Image image, ColorSpaceTypes colorSpace, TextureTypes textureType = TextureTypes.Default) : base(name, sampler, colorSpace, textureType)
        {
            Image = image;
        }

        public override string ToString()
        {
            return $"{Name}({Image.Name}: {Image.MimeType})";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageTexture rhs)
            {
                return Equals(rhs);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ImageTexture other)
        {
            if (Name != other.Name) return false;
            // if (Offset != other.Offset) return false;
            // if (Scaling != other.Scaling) return false;
            if (!Image.Bytes.Equals(other.Image.Bytes)) return false;
            return true;
        }
    }

    public class MetallicRoughnessImageTexture : ImageTexture
    {
        public float RoughnessFactor;

        public MetallicRoughnessImageTexture(string name, TextureSampler sampler, Image image, float roughnessFactor, ColorSpaceTypes colorSpace, TextureTypes textureType = TextureTypes.Default)
            : base(name, sampler, image, colorSpace, textureType)
        {
            Image = image;
            RoughnessFactor = roughnessFactor;
        }
    }

    /// <summary>
    /// 単色の 2x2 テクスチャ
    /// </summary>
    public class SolidTexture : Texture
    {
        public readonly Vector4 Color;

        public SolidTexture(string name, TextureSampler sampler, Vector4 color, ColorSpaceTypes colorSpace, TextureTypes textureType) : base(name, sampler, colorSpace, textureType)
        {
            Color = color;
        }

        public static readonly SolidTexture White = new SolidTexture("white", new TextureSampler(), Vector4.One, ColorSpaceTypes.Srgb, TextureTypes.Default);
    }

    /// <summary>
    /// レンダーターゲットの元になる
    /// </summary>
    public class RenderTexture : Texture
    {
        public int Width;
        public int Height;

        public RenderTexture(string name, TextureSampler sampler, ColorSpaceTypes colorSpace, TextureTypes textureType, int width = 256, int height = 256) : base(name, sampler, colorSpace, textureType)
        {
            Width = width;
            Height = height;
        }

        public void Resize(int w, int h)
        {
            Width = w;
            Height = h;
        }
    }
}
