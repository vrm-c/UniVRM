using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRMShaders.VRM10.MToon10.Editor
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
            var alphaMode = (AlphaMode) _material.GetInt(Prop.AlphaMode);
            var zWriteMode = (TransparentWithZWriteMode) _material.GetInt(Prop.TransparentWithZWrite);
            var renderQueueOffset = _material.GetInt(Prop.RenderQueueOffsetNumber);
            var doubleSidedMode = (DoubleSidedMode) _material.GetInt(Prop.DoubleSided);
            SetUnityShaderPassSettings(_material, alphaMode, zWriteMode, renderQueueOffset, doubleSidedMode);
            SetUnityShaderVariants(_material);
        }

        private static void SetUnityShaderPassSettings(Material material, AlphaMode alphaMode, TransparentWithZWriteMode zWriteMode, int renderQueueOffset, DoubleSidedMode doubleSidedMode)
        {
            material.SetInt(Prop.AlphaMode, (int) alphaMode);
            material.SetInt(Prop.TransparentWithZWrite, (int) zWriteMode);
            material.SetInt(Prop.DoubleSided, (int) doubleSidedMode);

            switch (alphaMode)
            {
                case AlphaMode.Opaque:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.OpaqueValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = 0;
                    material.renderQueue = (int) RenderQueue.Geometry;
                    break;
                case AlphaMode.Cutout:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentCutoutValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.On);

                    renderQueueOffset = 0;
                    material.renderQueue = (int) RenderQueue.AlphaTest;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.Off:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.Off);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = Mathf.Clamp(renderQueueOffset, -9, 0);
                    material.renderQueue = (int) RenderQueue.Transparent + renderQueueOffset;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.On:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);

                    renderQueueOffset = Mathf.Clamp(renderQueueOffset, 0, +9);
                    material.renderQueue = (int) RenderQueue.GeometryLast + 1 + renderQueueOffset; // Transparent First + N
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alphaMode), alphaMode, null);
            }

            switch (doubleSidedMode)
            {
                case DoubleSidedMode.Off:
                    material.SetInt(Prop.UnityCullMode, (int) CullMode.Back);
                    break;
                case DoubleSidedMode.On:
                    material.SetInt(Prop.UnityCullMode, (int) CullMode.Off);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doubleSidedMode), doubleSidedMode, null);
            }

            // Set after validation
            material.SetInt(Prop.RenderQueueOffsetNumber, renderQueueOffset);
        }

        private static void SetUnityCullingSettings(Material material, DoubleSidedMode doubleSidedMode)
        {

        }

        private static void SetUnityShaderVariants(Material material)
        {
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaTest,
                (AlphaMode) material.GetInt(Prop.AlphaMode) == AlphaMode.Cutout
            );
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaBlend,
                (AlphaMode) material.GetInt(Prop.AlphaMode) == AlphaMode.Transparent
            );
            material.SetKeyword(
                UnityAlphaModeKeyword.AlphaPremultiply,
                false
            );
            material.SetKeyword(
                NormalMapKeyword.On,
                material.GetTexture(Prop.NormalTexture) != null
            );
            material.SetKeyword(
                EmissiveMapKeyword.On,
                material.GetTexture(Prop.EmissiveTexture) != null
            );
            material.SetKeyword(
                RimMapKeyword.On,
                material.GetTexture(Prop.MatcapTexture) != null || // Matcap
                material.GetTexture(Prop.RimMultiplyTexture) != null // Rim
            );
            material.SetKeyword(
                ParameterMapKeyword.On,
                material.GetTexture(Prop.ShadingShiftTexture) != null || // Shading Shift (R)
                material.GetTexture(Prop.OutlineWidthMultiplyTexture) != null || // Outline Width (G)
                material.GetTexture(Prop.UvAnimationMaskTexture) != null // UV Anim Mask (B)
            );
            material.SetKeyword(
                OutlineModeKeyword.World,
                (OutlineMode) material.GetInt(Prop.OutlineWidthMode) == OutlineMode.World
            );
            material.SetKeyword(
                OutlineModeKeyword.Screen,
                (OutlineMode) material.GetInt(Prop.OutlineWidthMode) == OutlineMode.Screen
            );
        }
    }
}