using System;
using UnityEngine;

namespace VRMShaders
{
    internal readonly struct TextureExportKey : IEquatable<TextureExportKey>
    {
        public readonly Texture Src;
        public readonly TextureExportTypes TextureType;

        public TextureExportKey(Texture src, TextureExportTypes type)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }
            Src = src;
            TextureType = type;
        }

        public bool Equals(TextureExportKey other)
        {
            return Equals(Src, other.Src) && TextureType == other.TextureType;
        }

        public override bool Equals(object obj)
        {
            return obj is TextureExportKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Src != null ? Src.GetHashCode() : 0) * 397) ^ (int) TextureType;
            }
        }
    }
}