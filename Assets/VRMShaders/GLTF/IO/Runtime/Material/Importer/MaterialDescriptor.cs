using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    public readonly struct MaterialDescriptor : IEquatable<MaterialDescriptor>
    {
        public readonly string Name;
        public readonly string ShaderName;
        public readonly int? RenderQueue;
        public readonly IReadOnlyDictionary<string, TextureDescriptor> TextureSlots;
        public readonly IReadOnlyDictionary<string, float> FloatValues;
        public readonly IReadOnlyDictionary<string, Color> Colors;
        public readonly IReadOnlyDictionary<string, Vector4> Vectors;
        public readonly IReadOnlyList<Action<Material>> Actions;

        public SubAssetKey SubAssetKey => new SubAssetKey(SubAssetKey.MaterialType, Name);

        public MaterialDescriptor(
            string name,
            string shaderName,
            int? renderQueue,
            IReadOnlyDictionary<string, TextureDescriptor> textureSlots,
            IReadOnlyDictionary<string, float> floatValues,
            IReadOnlyDictionary<string, Color> colors,
            IReadOnlyDictionary<string, Vector4> vectors,
            IReadOnlyList<Action<Material>> actions)
        {
            Name = name;
            ShaderName = shaderName;
            RenderQueue = renderQueue;
            TextureSlots = textureSlots;
            FloatValues = floatValues;
            Colors = colors;
            Vectors = vectors;
            Actions = actions;
        }

        public bool Equals(MaterialDescriptor other)
        {
            return Name == other.Name && ShaderName == other.ShaderName && RenderQueue == other.RenderQueue && Equals(TextureSlots, other.TextureSlots) && Equals(FloatValues, other.FloatValues) && Equals(Colors, other.Colors) && Equals(Vectors, other.Vectors) && Equals(Actions, other.Actions);
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialDescriptor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ShaderName != null ? ShaderName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RenderQueue.GetHashCode();
                hashCode = (hashCode * 397) ^ (TextureSlots != null ? TextureSlots.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FloatValues != null ? FloatValues.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Colors != null ? Colors.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Vectors != null ? Vectors.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Actions != null ? Actions.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}