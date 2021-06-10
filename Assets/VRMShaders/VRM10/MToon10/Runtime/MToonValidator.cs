using System;
using UnityEngine;
using UnityEngine.Rendering;
using VRMShaders.VRM10.MToon10.Runtime;

namespace VRMShaders.VRM10.MToon10.Runtime
{
    public sealed class MToonValidator
    {
        private readonly Material _material;

        public MToonValidator(Material material)
        {
            _material = material;
        }

        public void Validate()
        {
            var alphaMode = (AlphaMode) _material.GetInt(MToon10Prop.AlphaMode);
            var zWriteMode = (TransparentWithZWriteMode) _material.GetInt(MToon10Prop.TransparentWithZWrite);
            var renderQueueOffset = _material.GetInt(MToon10Prop.RenderQueueOffsetNumber);
            var doubleSidedMode = (DoubleSidedMode) _material.GetInt(MToon10Prop.DoubleSided);
            SetUnityShaderPassSettings(_material, alphaMode, zWriteMode, renderQueueOffset, doubleSidedMode);
            SetUnityShaderVariants(_material);
        }

        private static void SetUnityShaderPassSettings(Material material, AlphaMode alphaMode, TransparentWithZWriteMode zWriteMode, int renderQueueOffset, DoubleSidedMode doubleSidedMode)
        {
            material.SetInt(MToon10Prop.AlphaMode, (int) alphaMode);
            material.SetInt(MToon10Prop.TransparentWithZWrite, (int) zWriteMode);
            material.SetInt(MToon10Prop.DoubleSided, (int) doubleSidedMode);

            switch (alphaMode)
            {
                case AlphaMode.Opaque:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.OpaqueValue);
                    material.SetInt(MToon10Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(MToon10Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(MToon10Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(MToon10Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = 0;
                    material.renderQueue = (int) RenderQueue.Geometry;
                    break;
                case AlphaMode.Cutout:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentCutoutValue);
                    material.SetInt(MToon10Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(MToon10Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(MToon10Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(MToon10Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.On);

                    renderQueueOffset = 0;
                    material.renderQueue = (int) RenderQueue.AlphaTest;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.Off:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(MToon10Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(MToon10Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(MToon10Prop.UnityZWrite, (int) UnityZWriteMode.Off);
                    material.SetInt(MToon10Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = Mathf.Clamp(renderQueueOffset, -9, 0);
                    material.renderQueue = (int) RenderQueue.Transparent + renderQueueOffset;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.On:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(MToon10Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(MToon10Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(MToon10Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(MToon10Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = Mathf.Clamp(renderQueueOffset, 0, +9);
                    material.renderQueue = (int) RenderQueue.GeometryLast + 1 + renderQueueOffset; // Transparent First + N
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alphaMode), alphaMode, null);
            }

            switch (doubleSidedMode)
            {
                case DoubleSidedMode.Off:
                    material.SetInt(MToon10Prop.UnityCullMode, (int) CullMode.Back);
                    break;
                case DoubleSidedMode.On:
                    material.SetInt(MToon10Prop.UnityCullMode, (int) CullMode.Off);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doubleSidedMode), doubleSidedMode, null);
            }

            // Set after validation
            material.SetInt(MToon10Prop.RenderQueueOffsetNumber, renderQueueOffset);
        }

        private static void SetUnityCullingSettings(Material material, DoubleSidedMode doubleSidedMode)
        {

        }

        private static void SetUnityShaderVariants(Material material)
        {
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaTest,
                (AlphaMode) material.GetInt(MToon10Prop.AlphaMode) == AlphaMode.Cutout
            );
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaBlend,
                (AlphaMode) material.GetInt(MToon10Prop.AlphaMode) == AlphaMode.Transparent
            );
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaPremultiply,
                false
            );
            material.SetKeyword(
                NormalMapKeyword.On,
                material.GetTexture(MToon10Prop.NormalTexture) != null
            );
            material.SetKeyword(
                EmissiveMapKeyword.On,
                material.GetTexture(MToon10Prop.EmissiveTexture) != null
            );
            material.SetKeyword(
                RimMapKeyword.On,
                material.GetTexture(MToon10Prop.MatcapTexture) != null || // Matcap
                material.GetTexture(MToon10Prop.RimMultiplyTexture) != null // Rim
            );
            material.SetKeyword(
                ParameterMapKeyword.On,
                material.GetTexture(MToon10Prop.ShadingShiftTexture) != null || // Shading Shift (R)
                material.GetTexture(MToon10Prop.OutlineWidthMultiplyTexture) != null || // Outline Width (G)
                material.GetTexture(MToon10Prop.UvAnimationMaskTexture) != null // UV Anim Mask (B)
            );
            material.SetKeyword(
                OutlineModeKeyword.World,
                (OutlineMode) material.GetInt(MToon10Prop.OutlineWidthMode) == OutlineMode.World
            );
            material.SetKeyword(
                OutlineModeKeyword.Screen,
                (OutlineMode) material.GetInt(MToon10Prop.OutlineWidthMode) == OutlineMode.Screen
            );
        }
    }
}