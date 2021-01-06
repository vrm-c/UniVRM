using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
#if UNITY_EDITOR
    [Serializable]
    public struct PropItem
    {
        public string Name;
        public ShaderUtil.ShaderPropertyType PropertyType;
        public Vector4 DefaultValues;
    }
#endif

    /// <summary>
    /// Material 一つ分のプロパティを蓄えている
    ///
    /// * PreviewSceneManager で使う
    /// * MaterialValueBindingMerger で使う
    ///
    /// </summary>
    [Serializable]
    public class PreviewMaterialItem
    {
        public readonly Material Material;

        public PreviewMaterialItem(Material material)
        {
            Material = material;
        }

        public Dictionary<VrmLib.MaterialBindType, PropItem> PropMap = new Dictionary<VrmLib.MaterialBindType, PropItem>();

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
                            var bindType = VrmLib.MaterialBindTypeExtensions.GetBindType(name);
                            item.PropMap.Add(bindType, new PropItem
                            {
                                Name = name,
                                PropertyType = propType,
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
