using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public enum ExpressionOverrideType
    {
        None,
        Block,
        Blend,
    }

    public class Expression
    {
        public readonly ExpressionPreset Preset;
        public readonly string Name;

        public bool IsBinary;

        public ExpressionOverrideType OverrideBlink;
        public ExpressionOverrideType OverrideLookAt;
        public ExpressionOverrideType OverrideMouth;

        public readonly List<MorphTargetBind> MorphTargetBinds = new List<MorphTargetBind>();

        public readonly List<MaterialColorBind> MaterialColorBinds = new List<MaterialColorBind>();

        public readonly List<TextureTransformBind> TextureTransformBinds = new List<TextureTransformBind>();

        public void CleanupUVScaleOffset()
        {
            // ST_S, ST_T を統合する
            var count = TextureTransformBinds.Count;
            var map = new Dictionary<Material, TextureTransformBind>();
            foreach (var uv in TextureTransformBinds.OrderBy(uv => uv.Material.Name).Distinct())
            {
                if (!map.TryGetValue(uv.Material, out TextureTransformBind value))
                {
                    value = new TextureTransformBind(uv.Material, Vector2.One, Vector2.Zero);
                }
                map[uv.Material] = value.Merge(uv);
            }
            TextureTransformBinds.Clear();
            foreach (var kv in map)
            {
                TextureTransformBinds.Add(new TextureTransformBind(kv.Key,
                    kv.Value.Scale,
                    kv.Value.Offset));
            }
            // Console.WriteLine($"MergeUVScaleOffset: {count} => {UVScaleOffsetValues.Count}");
        }

        public Expression(ExpressionPreset preset, string name, bool isBinary)
        {
            Preset = preset;
            Name = name;
            IsBinary = isBinary;
        }

        public override string ToString()
        {
            return Preset.ToString();
        }
    }
}
