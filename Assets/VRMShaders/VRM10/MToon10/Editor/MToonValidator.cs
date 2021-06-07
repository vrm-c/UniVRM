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
            SetUnityRenderSettings(_material, alphaMode, zWriteMode);
        }

        private static void SetUnityRenderSettings(Material material, AlphaMode alphaMode, TransparentWithZWriteMode zWriteMode)
        {
            material.SetInt(Prop.AlphaMode, (int) alphaMode);
            material.SetInt(Prop.TransparentWithZWrite, (int) zWriteMode);

            switch (alphaMode)
            {
                case AlphaMode.Opaque:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.OpaqueValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaTest, false);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaBlend, false);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaPremultiply, false);
                    material.renderQueue = (int) RenderQueue.Geometry;
                    break;
                case AlphaMode.Cutout:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentCutoutValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.One);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.On);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaTest, true);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaBlend, false);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaPremultiply, false);
                    material.renderQueue = (int) RenderQueue.AlphaTest;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.Off:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.Off);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaTest, false);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaBlend, true);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaPremultiply, false);
                    material.renderQueue = (int) RenderQueue.Transparent;
                    break;
                case AlphaMode.Transparent when zWriteMode == TransparentWithZWriteMode.On:
                    material.SetOverrideTag(UnityRenderTag.Key, UnityRenderTag.TransparentValue);
                    material.SetInt(Prop.UnitySrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Prop.UnityDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Prop.UnityZWrite, (int) UnityZWriteMode.On);
                    material.SetInt(Prop.UnityAlphaToMask, (int) UnityAlphaToMaskMode.Off);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaTest, false);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaBlend, true);
                    material.SetKeyword(UnityAlphaModeKeyword.AlphaPremultiply, false);
                    material.renderQueue = (int) RenderQueue.GeometryLast + 1; // Transparent First
                    break;
                default:
                    SetUnityRenderSettings(material, AlphaMode.Opaque, TransparentWithZWriteMode.Off);
                    break;
            }
        }
    }
}