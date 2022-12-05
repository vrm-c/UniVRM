using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    public sealed class MaterialDescriptor
    {
        public readonly string Name;
        public readonly Shader Shader;
        public readonly int? RenderQueue;
        public readonly IReadOnlyDictionary<string, TextureDescriptor> TextureSlots;
        public readonly IReadOnlyDictionary<string, float> FloatValues;
        public readonly IReadOnlyDictionary<string, Color> Colors;
        public readonly IReadOnlyDictionary<string, Vector4> Vectors;
        public readonly IReadOnlyList<Action<Material>> Actions;

        public SubAssetKey SubAssetKey => new SubAssetKey(SubAssetKey.MaterialType, Name);

        public MaterialDescriptor(
            string name,
            Shader shader,
            int? renderQueue,
            IReadOnlyDictionary<string, TextureDescriptor> textureSlots,
            IReadOnlyDictionary<string, float> floatValues,
            IReadOnlyDictionary<string, Color> colors,
            IReadOnlyDictionary<string, Vector4> vectors,
            IReadOnlyList<Action<Material>> actions)
        {
            Name = name;
            Shader = shader;
            RenderQueue = renderQueue;
            TextureSlots = textureSlots;
            FloatValues = floatValues;
            Colors = colors;
            Vectors = vectors;
            Actions = actions;
        }
    }
}