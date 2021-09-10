using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace UniVRM10
{
    public static class PreviewMaterialUtil
    {
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
                            var bindType = PreviewMaterialItem.GetBindType(name);
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
                        break;
                }
            }
            item.PropNames = propNames.ToArray();
            return item;
        }
    }
}
#endif