using System;
using System.Collections.Generic;
using UnityEngine;


namespace VRMShaders
{
    public class MaterialDescriptor
    {
        public readonly string Name;
        public readonly string ShaderName;
        public readonly Dictionary<string, TextureDescriptor> TextureSlots = new Dictionary<string, TextureDescriptor>();
        public readonly Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        public readonly Dictionary<string, Vector4> Vectors = new Dictionary<string, Vector4>();
        public int? RenderQueue;
        public readonly List<Action<Material>> Actions = new List<Action<Material>>();

        public SubAssetKey SubAssetKey => new SubAssetKey(SubAssetKey.MaterialType, Name);

        public MaterialDescriptor(string name, string shaderName)
        {
            Name = name;
            ShaderName = shaderName;
        }
    }
}
