using System;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        }

        public Dictionary<UniGLTF.Extensions.VRMC_vrm.MaterialColorType, PropItem> PropMap = new Dictionary<UniGLTF.Extensions.VRMC_vrm.MaterialColorType, PropItem>();

        public string[] PropNames
        {
            get;
            private set;
        }

        public void RestoreInitialValues()
        {
            foreach (var prop in PropMap)
            {
                Material.SetColor(prop.Value.Name, prop.Value.DefaultValues);
            }
        }

#if UNITY_EDITOR
        public const string UV_PROPERTY = "_MainTex_ST";
        public const string COLOR_PROPERTY = "_Color";
        public const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
        public const string RIM_COLOR_PROPERTY = "_RimColor";
        public const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
        public const string SHADE_COLOR_PROPERTY = "_ShadeColor";
        public static MaterialColorType GetBindType(string property)
        {
            switch (property)
            {
                case COLOR_PROPERTY:
                    return MaterialColorType.color;

                case EMISSION_COLOR_PROPERTY:
                    return MaterialColorType.emissionColor;

                case RIM_COLOR_PROPERTY:
                    return MaterialColorType.rimColor;

                case SHADE_COLOR_PROPERTY:
                    return MaterialColorType.shadeColor;

                case OUTLINE_COLOR_PROPERTY:
                    return MaterialColorType.outlineColor;
            }

            throw new NotImplementedException();
        }

        public static PreviewMaterialItem CreateForPreview(Material material)
        {
            var item = new PreviewMaterialItem(material);

            var propNames = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); ++i)
            {
                var propType = ShaderUtil.GetPropertyType(material.shader, i);
                var name = ShaderUtil.GetPropertyName(material.shader, i);

                switch (propType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        // 色
                        {
                            var bindType = GetBindType(name);
                            item.PropMap.Add(bindType, new PropItem
                            {
                                Name = name,
                                PropertyType = (UniVRM10.ShaderPropertyType)Enum.Parse(typeof(UniVRM10.ShaderPropertyType), propType.ToString(), true),
                                DefaultValues = material.GetColor(name),
                            });
                            propNames.Add(name);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        // テクスチャ
                        // {
                        //     name += "_ST";
                        //     item.PropMap.Add(name, new PropItem
                        //     {
                        //         PropertyType = propType,
                        //         DefaultValues = material.GetVector(name),
                        //     });
                        //     propNames.Add(name);
                        // }
                        // // 縦横分離用
                        // {
                        //     var st_name = name + "_S";
                        //     item.PropMap.Add(st_name, new PropItem
                        //     {
                        //         PropertyType = propType,
                        //         DefaultValues = material.GetVector(name),
                        //     });
                        //     propNames.Add(st_name);
                        // }
                        // {
                        //     var st_name = name + "_T";
                        //     item.PropMap.Add(st_name, new PropItem
                        //     {
                        //         PropertyType = propType,
                        //         DefaultValues = material.GetVector(name),
                        //     });
                        //     propNames.Add(st_name);
                        // }
                        break;
                }
            }
            item.PropNames = propNames.ToArray();
            return item;
        }
#endif
    }
}
