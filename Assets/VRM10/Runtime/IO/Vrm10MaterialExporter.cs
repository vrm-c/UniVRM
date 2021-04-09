using System;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class Vrm10MaterialExporter : MaterialExporter
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

        static readonly string[] TAGS = new string[]{
            "RenderType",
            // "Queue",
        };

        public override glTFMaterial ExportMaterial(Material m, TextureExporter textureExporter)
        {
            if (m.shader.name != MToon.Utils.ShaderName)
            {
                return base.ExportMaterial(m, textureExporter);
            }

            var material = new glTFMaterial
            {
                name = m.name,

                emissiveFactor = new float[] { 0, 0, 0 },
            };

            // default values
            var mtoon = new UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon
            {
                Version = "",

                TransparentWithZWrite = false,

                RenderQueueOffsetNumber = 0,

                ShadeFactor = new float[] { 0, 0, 0 },

                // ShadeMultiplyTexture;

                // Lighting
                ShadingShiftFactor = 0,

                ShadingToonyFactor = 0,

                LightColorAttenuationFactor = 0,

                GiIntensityFactor = 0,

                // MatCap
                // AdditiveTexture;

                // Rim
                RimFactor = new float[] { 0, 0, 0 },

                // public int? RimMultiplyTexture;

                RimLightingMixFactor = 0,

                RimFresnelPowerFactor = 0,

                RimLiftFactor = 0,

                // Outline
                OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.none,

                OutlineWidthFactor = 0,

                // public int? OutlineWidthMultiplyTexture;

                OutlineScaledMaxDistanceFactor = 0,

                OutlineColorMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineColorMode.fixedColor,

                OutlineFactor = new float[] { 0, 0, 0 },

                OutlineLightingMixFactor = 0,

                // public int? UvAnimationMaskTexture;

                UvAnimationScrollXSpeedFactor = 0,

                UvAnimationScrollYSpeedFactor = 0,

                UvAnimationRotationSpeedFactor = 0,
            };

            // var prop = PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            // if (prop == null)
            // {
            //     Debug.LogWarningFormat("Fail to export shader: {0}", m.shader.name);
            // }
            // else
            // {
            //     foreach (var keyword in m.shaderKeywords)
            //     {
            //         material.keywordMap.Add(keyword, m.IsKeywordEnabled(keyword));
            //     }

            //     // get properties
            //     //material.SetProp(prop);
            //     foreach (var kv in prop.Properties)
            //     {
            //         switch (kv.ShaderPropertyType)
            //         {
            //             case ShaderPropertyType.Color:
            //                 {
            //                     var value = m.GetColor(kv.Key).ToArray();
            //                     material.vectorProperties.Add(kv.Key, value);
            //                 }
            //                 break;

            //             case ShaderPropertyType.Range:
            //             case ShaderPropertyType.Float:
            //                 {
            //                     var value = m.GetFloat(kv.Key);
            //                     material.floatProperties.Add(kv.Key, value);
            //                 }
            //                 break;

            //             case ShaderPropertyType.TexEnv:
            //                 {
            //                     var texture = m.GetTexture(kv.Key);
            //                     if (texture != null)
            //                     {
            //                         var value = kv.Key == "_BumpMap"
            //                             ? textureExporter.ExportNormal(texture)
            //                             : textureExporter.ExportSRGB(texture)
            //                             ;
            //                         if (value == -1)
            //                         {
            //                             Debug.LogFormat("not found {0}", texture.name);
            //                         }
            //                         else
            //                         {
            //                             material.textureProperties.Add(kv.Key, value);
            //                         }
            //                     }

            //                     // offset & scaling
            //                     var offset = m.GetTextureOffset(kv.Key);
            //                     var scaling = m.GetTextureScale(kv.Key);
            //                     material.vectorProperties.Add(kv.Key,
            //                         new float[] { offset.x, offset.y, scaling.x, scaling.y });
            //                 }
            //                 break;

            //             case ShaderPropertyType.Vector:
            //                 {
            //                     var value = m.GetVector(kv.Key).ToArray();
            //                     material.vectorProperties.Add(kv.Key, value);
            //                 }
            //                 break;

            //             default:
            //                 throw new NotImplementedException();
            //         }
            //     }
            // }

            // foreach (var tag in TAGS)
            // {
            //     var value = m.GetTag(tag, false);
            //     if (!String.IsNullOrEmpty(value))
            //     {
            //         material.tagMap.Add(tag, value);
            //     }
            // }

            UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref material.extensions, mtoon);

            return material;
        }
        #endregion
    }
}
