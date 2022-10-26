using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace UniVRM10
{
    public static class PreviewMaterialUtil
    {
        public static bool TryCreateForPreview(Material material, out PreviewMaterialItem item)
        {
            item = new PreviewMaterialItem(material);

            var propNames = new List<string>();
            var hasError = false;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); ++i)
            {
                var propType = ShaderUtil.GetPropertyType(material.shader, i);
                var name = ShaderUtil.GetPropertyName(material.shader, i);

                switch (propType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        // 色
                        {
                            if (!PreviewMaterialItem.TryGetBindType(name, out var bindType))
                            {
                                Debug.LogError($"{material.shader.name}.{name} is unsupported property name");
                                hasError = true;
                                continue;
                            }

                            if (!Enum.TryParse(propType.ToString(), true, out ShaderPropertyType propertyType))
                            {
                                Debug.LogError($"{material.shader.name}.{propertyType.ToString()} is unsupported property type");
                                hasError = true;
                                continue;
                            }
                            
                            item.PropMap.Add(bindType, new PropItem
                            {
                                Name = name,
                                PropertyType = propertyType,
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

            return !hasError;
        }
    }
}
#endif