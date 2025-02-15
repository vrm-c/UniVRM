using System;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
using VRM10.MToon10;


namespace UniVRM10
{
    public enum ShaderPropertyType
    {
        //
        // 概要:
        //     Color Property.
        Color = 0,
        //
        // 概要:
        //     Vector Property.
        Vector = 1,
        //
        // 概要:
        //     Float Property.
        Float = 2,
        //
        // 概要:
        //     Range Property.
        Range = 3,
        //
        // 概要:
        //     Texture Property.
        TexEnv = 4
    }

    [Serializable]
    public struct PropItem
    {
        public string Name;
        public ShaderPropertyType PropertyType;
        public Vector4 DefaultValues;
    }

    /// <summary>
    /// Material 一つ分のプロパティを蓄えている
    ///
    /// * PreviewSceneManager で使う
    /// * MaterialValueBindingMerger で使う
    ///
    /// </summary>
    [Serializable]
    public sealed class PreviewMaterialItem
    {
        public readonly Material Material;

        public PreviewMaterialItem(Material material)
        {
            Material = material;

            // uv default value
            var s = material.mainTextureScale;
            var o = material.mainTextureOffset;
            DefaultUVScaleOffset = new(s.x, s.y, o.x, o.y);
        }

        public Dictionary<UniGLTF.Extensions.VRMC_vrm.MaterialColorType, PropItem> PropMap = new Dictionary<UniGLTF.Extensions.VRMC_vrm.MaterialColorType, PropItem>();

        public Vector4 DefaultUVScaleOffset = new Vector4(1, 1, 0, 0);

        public string[] PropNames
        {
            get;
            set;
        }

        public void RestoreInitialValues()
        {
            foreach (var prop in PropMap)
            {
                Material.SetColor(prop.Value.Name, prop.Value.DefaultValues);
            }
        }

        public static readonly string COLOR_PROPERTY = MToon10Prop.BaseColorFactor.ToUnityShaderLabName();
        public static readonly string EMISSION_COLOR_PROPERTY = MToon10Prop.EmissiveFactor.ToUnityShaderLabName();
        public static readonly string RIM_COLOR_PROPERTY = MToon10Prop.ParametricRimColorFactor.ToUnityShaderLabName();
        public static readonly string OUTLINE_COLOR_PROPERTY = MToon10Prop.OutlineColorFactor.ToUnityShaderLabName();
        public static readonly string SHADE_COLOR_PROPERTY = MToon10Prop.ShadeColorFactor.ToUnityShaderLabName();
        public static readonly string MATCAP_COLOR_PROPERTY = MToon10Prop.MatcapColorFactor.ToUnityShaderLabName();

        public static bool TryGetBindType(string property, out MaterialColorType type)
        {
            if (property == COLOR_PROPERTY)
            {
                type = MaterialColorType.color;
            }
            else if (property == EMISSION_COLOR_PROPERTY)
            {
                type = MaterialColorType.emissionColor;
            }
            else if (property == RIM_COLOR_PROPERTY)
            {
                type = MaterialColorType.rimColor;
            }
            else if (property == OUTLINE_COLOR_PROPERTY)
            {
                type = MaterialColorType.outlineColor;
            }
            else if (property == SHADE_COLOR_PROPERTY)
            {
                type = MaterialColorType.shadeColor;
            }
            else if (property == MATCAP_COLOR_PROPERTY)
            {
                type = MaterialColorType.matcapColor;
            }
            else
            {
                type = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// [Preview] 積算する前の初期値にクリアする
        /// </summary>
        public void Clear()
        {
            // clear Color
            foreach (var _kv in PropMap)
            {
                Material.SetColor(_kv.Value.Name, _kv.Value.DefaultValues);
            }

            // clear UV
            Material.mainTextureScale = new(DefaultUVScaleOffset.x, DefaultUVScaleOffset.y);
            Material.mainTextureOffset = new(DefaultUVScaleOffset.z, DefaultUVScaleOffset.w);
        }

        /// <summary>
        /// [Preview] scaleOffset を weight で重みを付けて加える
        /// </summary>
        /// <param name="scaleOffset"></param>
        /// <param name="weight"></param>
        public void AddScaleOffset(Vector4 scaleOffset, float weight)
        {
            var s = Material.mainTextureScale;
            var o = Material.mainTextureOffset;
            var value = new Vector4(s.x, s.y, o.x, o.y);
            value += (scaleOffset - DefaultUVScaleOffset) * weight;
            Material.mainTextureOffset = new(value.z, value.w);
            Material.mainTextureScale = new(value.x, value.y);
        }
    }
}
