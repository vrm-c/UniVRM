using System;
using UniGLTF;
using UniGLTF.ShaderPropExporter;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace VRM
{
    /// <summary>
    /// VRM/MToon のマテリアル情報をエクスポートする。
    /// VRM extension 内の materialProperties に記録するデータを用意する。
    /// </summary>
    public static class BuiltInVrmExtensionMaterialPropertyExporter
    {
        private static readonly string[] ExportingTags =
        {
            "RenderType",
            // "Queue",
        };

        public static glTF_VRM_Material ExportMaterial(Material m, ITextureExporter textureExporter)
        {
            var material = new glTF_VRM_Material
            {
                name = m.name,
                shader = m.shader.name,
                renderQueue = m.renderQueue,
            };

            if (m.shader.name != MToon.Utils.ShaderName)
            {
                material.shader = glTF_VRM_Material.VRM_USE_GLTFSHADER;
                return material;
            }

            var prop = PreShaderPropExporter.GetPropsForMToon();
            if (prop == null)
            {
                throw new Exception("arienai");
            }
            else
            {
                foreach (var keyword in m.shaderKeywords)
                {
                    material.keywordMap.Add(keyword, m.IsKeywordEnabled(keyword));
                }

                // get properties
                //material.SetProp(prop);
                foreach (var kv in prop.Properties)
                {
                    switch (kv.ShaderPropertyType)
                    {
                        case ShaderPropertyType.Color:
                        {
                            // No color conversion. Because color property is serialized to raw float array.
                            var value = m.GetColor(kv.Key).ToFloat4(ColorSpace.Linear, ColorSpace.Linear);
                            material.vectorProperties.Add(kv.Key, value);
                        }
                            break;

                        case ShaderPropertyType.Range:
                        case ShaderPropertyType.Float:
                        {
                            var value = m.GetFloat(kv.Key);
                            material.floatProperties.Add(kv.Key, value);
                        }
                            break;

                        case ShaderPropertyType.TexEnv:
                        {
                            var texture = m.GetTexture(kv.Key);
                            if (texture != null)
                            {
                                var value = -1;
                                var isNormalMap = kv.Key == "_BumpMap";
                                if (isNormalMap)
                                {
                                    value = textureExporter.RegisterExportingAsNormal(texture);
                                }
                                else
                                {
                                    var needsAlpha = kv.Key == "_MainTex";
                                    value = textureExporter.RegisterExportingAsSRgb(texture, needsAlpha);
                                }
                                if (value == -1)
                                {
                                    Debug.LogFormat("not found {0}", texture.name);
                                }
                                else
                                {
                                    material.textureProperties.Add(kv.Key, value);
                                }
                            }

                            // offset & scaling
                            var offset = m.GetTextureOffset(kv.Key);
                            var scaling = m.GetTextureScale(kv.Key);
                            material.vectorProperties.Add(kv.Key,
                                new float[] { offset.x, offset.y, scaling.x, scaling.y });
                        }
                            break;

                        case ShaderPropertyType.Vector:
                        {
                            var value = m.GetVector(kv.Key).ToArray();
                            material.vectorProperties.Add(kv.Key, value);
                        }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            foreach (var tag in ExportingTags)
            {
                var value = m.GetTag(tag, false);
                if (!String.IsNullOrEmpty(value))
                {
                    material.tagMap.Add(tag, value);
                }
            }

            return material;
        }
    }
}