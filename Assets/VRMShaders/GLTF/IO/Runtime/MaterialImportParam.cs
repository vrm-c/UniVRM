using System;
using System.Collections.Generic;
using UnityEngine;


namespace VRMShaders
{
    public class MaterialImportParam
    {
        public readonly string Name;
        public readonly string ShaderName;
        public readonly Dictionary<string, TextureImportParam> TextureSlots = new Dictionary<string, TextureImportParam>();
        public readonly Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        public readonly Dictionary<string, Vector4> Vectors = new Dictionary<string, Vector4>();
        public readonly List<Action<Material>> Actions = new List<Action<Material>>();

        public MaterialImportParam(string name, string shaderName)
        {
            Name = name;
            ShaderName = shaderName;
        }
    }
}
