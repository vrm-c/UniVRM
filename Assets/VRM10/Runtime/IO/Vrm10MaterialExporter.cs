using UniGLTF;
using UnityEngine;
using VRMShaders;
using ColorSpace = UniGLTF.ColorSpace;

namespace UniVRM10
{
    public class Vrm10MaterialExporter : MaterialExporter
    {
        public override glTFMaterial ExportMaterial(Material m, TextureExporter textureExporter)
        {
            if (m.shader.name != MToon.Utils.ShaderName)
            {
                return base.ExportMaterial(m, textureExporter);
            }

            // convert MToon intermediate value from UnityEngine.Material
            var def = MToon.Utils.GetMToonParametersFromMaterial(m);

            // gltfMaterial
            var material = new glTFMaterial
            {
                name = m.name,

                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = def.Color.LitColor.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear),
                    baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = textureExporter.ExportSRGB(def.Color.LitMultiplyTexture),
                    },
                },

                emissiveFactor = def.Emission.EmissionColor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear),
            };

            // VRMC_materials_mtoon
            var mtoon = new UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon
            {
                Version = "",

                TransparentWithZWrite = false,

                RenderQueueOffsetNumber = 0,

                ShadeColorFactor = new float[] { 0, 0, 0 },

                ShadeMultiplyTexture = new UniGLTF.Extensions.VRMC_materials_mtoon.TextureInfo
                {
                    Index = textureExporter.ExportSRGB(def.Color.ShadeMultiplyTexture),
                },

                // Lighting
                ShadingShiftFactor = 0,

                ShadingToonyFactor = 0,

                GiIntensityFactor = 0,

                // MatCap
                // AdditiveTexture;

                // Rim
                ParametricRimColorFactor = new float[] { 0, 0, 0 },

                // public int? RimMultiplyTexture;

                RimLightingMixFactor = 0,

                ParametricRimFresnelPowerFactor = 0,

                ParametricRimLiftFactor = 0,

                // Outline
                OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.none,

                OutlineWidthFactor = 0,

                // public int? OutlineWidthMultiplyTexture;

                OutlineColorFactor = new float[] { 0, 0, 0 },

                OutlineLightingMixFactor = 0,

                // public int? UvAnimationMaskTexture;

                UvAnimationScrollXSpeedFactor = 0,

                UvAnimationScrollYSpeedFactor = 0,

                UvAnimationRotationSpeedFactor = 0,
            };

            UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref material.extensions, mtoon);

            return material;
        }
    }
}
