using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.ShaderPropExporter;
using UnityEngine;


namespace VRM
{
    public class VRMMaterialExporter : MaterialExporter
    {
        protected override glTFMaterial CreateMaterial(Material m)
        {
            switch (m.shader.name)
            {
                case "VRM/UnlitTexture":
                    return Export_VRMUnlitTexture(m);

                case "VRM/UnlitTransparent":
                    return Export_VRMUnlitTransparent(m);

                case "VRM/UnlitCutout":
                    return Export_VRMUnlitCutout(m);

                case "VRM/UnlitTransparentZWrite":
                    return Export_VRMUnlitTransparentZWrite(m);

                case "VRM/MToon":
                    return Export_VRMMToon(m);

                default:
                    return base.CreateMaterial(m);
            }
        }

        static glTFMaterial Export_VRMUnlitTexture(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "OPAQUE";
            return material;
        }
        static glTFMaterial Export_VRMUnlitTransparent(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "BLEND";
            return material;
        }
        static glTFMaterial Export_VRMUnlitCutout(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "MASK";
            return material;
        }
        static glTFMaterial Export_VRMUnlitTransparentZWrite(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "BLEND";
            return material;
        }

        static glTFMaterial Export_VRMMToon(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();

            switch (m.GetTag("RenderType", true))
            {
                case "Transparent":
                    material.alphaMode = "BLEND";
                    break;

                case "TransparentCutout":
                    material.alphaMode = "MASK";
                    material.alphaCutoff = m.GetFloat("_Cutoff");
                    break;

                default:
                    material.alphaMode = "OPAQUE";
                    break;
            }

            switch ((int)m.GetFloat("_CullMode"))
            {
                case 0:
                    material.doubleSided = true;
                    break;

                case 1:
                    Debug.LogWarning("ignore cull front");
                    break;

                case 2:
                    // cull back
                    break;

                default:
                    throw new NotImplementedException();
            }

            return material;
        }

        #region CreateFromMaterial
        private static readonly string[] VRMExtensionShaders = new string[]
        {
            "VRM/UnlitTransparentZWrite",
            "VRM/MToon"
        };

        static readonly string[] TAGS = new string[]{
            "RenderType",
            // "Queue",
        };

        public static glTF_VRM_Material CreateFromMaterial(Material m, List<Texture> textures)
        {
            var material = new glTF_VRM_Material
            {
                name = m.name,
                shader = m.shader.name,
                renderQueue = m.renderQueue,
            };

            if (!VRMExtensionShaders.Contains(m.shader.name))
            {
                material.shader = glTF_VRM_Material.VRM_USE_GLTFSHADER;
                return material;
            }

            var prop = PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            if (prop == null)
            {
                Debug.LogWarningFormat("Fail to export shader: {0}", m.shader.name);
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
                                var value = m.GetColor(kv.Key).ToArray();
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
                                    var value = textures.IndexOf(texture);
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

            foreach (var tag in TAGS)
            {
                var value = m.GetTag(tag, false);
                if (!String.IsNullOrEmpty(value))
                {
                    material.tagMap.Add(tag, value);
                }
            }

            return material;
        }
        #endregion
    }
}
