using System;
using System.Numerics;

namespace VrmLib
{
    public class TextureTransformBind : IEquatable<TextureTransformBind>
    {
        public readonly Material Material;
        public readonly Vector2 Scale; // default = [1, 1]
        public readonly Vector2 Offset; // default = [0, 0]

        public TextureTransformBind(Material material, Vector2 scale, Vector2 offset)
        {
            Material = material;
            Scale = scale;
            Offset = offset;
        }

        public override int GetHashCode()
        {
            return Material.GetHashCode();
        }

        public bool Equals(TextureTransformBind other)
        {
            return Material == other.Material && Scale == other.Scale && Offset == other.Offset;
        }

        /// <summary>
        /// Scaleは平均。Offsetは足す
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public TextureTransformBind Merge(TextureTransformBind rhs)
        {
            if (Material != rhs.Material)
            {
                throw new System.Exception();
            }
            return new TextureTransformBind(Material, (Scale + rhs.Scale) / 2, Offset + rhs.Offset);
        }
    }
}
