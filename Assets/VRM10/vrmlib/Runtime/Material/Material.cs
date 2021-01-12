using System.Collections.Generic;
using System;
using System.Numerics;

namespace VrmLib
{
    public class Material: GltfId
    {
        public string Name;

        public virtual LinearColor BaseColorFactor
        {
            get;
            set;
        } = LinearColor.White;

        public virtual TextureInfo BaseColorTexture
        {
            get;
            set;
        }

        public virtual AlphaModeType AlphaMode
        {
            get;
            set;
        } = AlphaModeType.OPAQUE;

        public virtual float AlphaCutoff
        {
            get;
            set;
        } = 0.5f;

        public virtual bool DoubleSided
        {
            get;
            set;
        }

        public Material(string name)
        {
            Name = name;
        }

        static protected bool ImageIsEquals(Image lhs, Image rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }
            else
            {
                if (rhs is null)
                {
                    return false;
                }
            }

            if (!lhs.Bytes.Equals(rhs.Bytes))
            {
                return false;
            }

            return true;
        }

        static protected bool TextureIsEquals(TextureInfo lhs, TextureInfo rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }
            else
            {
                if (rhs is null)
                {
                    return false;
                }
            }

            if (lhs.Offset != rhs.Offset) return false;
            if (lhs.Scaling != rhs.Scaling) return false;

            if (lhs.Texture is ImageTexture lImage && rhs.Texture is ImageTexture rImage)
            {
                if (!ImageIsEquals(lImage.Image, rImage.Image))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public virtual bool CanIntegrate(Material rhs)
        {
            return false;
        }
    }
}
