using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10
{
    public static class MToonLoader
    {
        public delegate Texture2D GetTextureFunc(VrmLib.TextureInfo texture);

        public static MToon.MToonDefinition ToUnity(this VrmLib.MToon.MToonDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.MToonDefinition
            {
                Color = src.Color.ToUnity(textures),
                Emission = src.Emission.ToUnity(textures),
                Lighting = src.Lighting.ToUnity(textures),
                MatCap = src.MatCap.ToUnity(textures),
                Meta = src.Meta.ToUnity(),
                Outline = src.Outline.ToUnity(textures),
                Rendering = src.Rendering.ToUnity(),
                Rim = src.Rim.ToUnity(textures),
                TextureOption = src.TextureOption.ToUnity(textures),
            };
        }

        static Vector2 ToUnity(this System.Numerics.Vector2 src)
        {
            return new Vector2(src.X, src.Y);
        }

        static MToon.ColorDefinition ToUnity(this VrmLib.MToon.ColorDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.ColorDefinition
            {
                CutoutThresholdValue = src.CutoutThresholdValue,
                LitColor = src.LitColor.ToUnitySRGB(),
                LitMultiplyTexture = textures.GetOrDefault(src.LitMultiplyTexture?.Texture),
                ShadeColor = src.ShadeColor.ToUnitySRGB(),
                ShadeMultiplyTexture = textures.GetOrDefault(src.ShadeMultiplyTexture?.Texture),
            };
        }

        static MToon.EmissionDefinition ToUnity(this VrmLib.MToon.EmissionDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.EmissionDefinition
            {
                EmissionColor = src.EmissionColor.ToUnityLinear(),
                EmissionMultiplyTexture = textures.GetOrDefault(src.EmissionMultiplyTexture?.Texture),
            };
        }

        static MToon.LightingDefinition ToUnity(this VrmLib.MToon.LightingDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.LightingDefinition
            {
                LightingInfluence = src.LightingInfluence.ToUnity(),
                LitAndShadeMixing = src.LitAndShadeMixing.ToUnity(textures),
                Normal = src.Normal.ToUnity(textures),
            };
        }

        static MToon.LightingInfluenceDefinition ToUnity(this VrmLib.MToon.LightingInfluenceDefinition src)
        {
            if (src == null) return null;
            return new MToon.LightingInfluenceDefinition
            {
                GiIntensityValue = src.GiIntensityValue,
                LightColorAttenuationValue = src.LightColorAttenuationValue,
            };
        }

        static MToon.LitAndShadeMixingDefinition ToUnity(this VrmLib.MToon.LitAndShadeMixingDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.LitAndShadeMixingDefinition
            {
                ShadingShiftValue = src.ShadingShiftValue,
                ShadingToonyValue = src.ShadingToonyValue,
            };
        }

        static MToon.NormalDefinition ToUnity(this VrmLib.MToon.NormalDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.NormalDefinition
            {
                NormalScaleValue = src.NormalScaleValue,
                NormalTexture = textures.GetOrDefault(src.NormalTexture?.Texture),
            };
        }

        static MToon.MatCapDefinition ToUnity(this VrmLib.MToon.MatCapDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.MatCapDefinition
            {
                AdditiveTexture = textures.GetOrDefault(src.AdditiveTexture?.Texture),
            };
        }

        static MToon.MetaDefinition ToUnity(this VrmLib.MToon.MetaDefinition src)
        {
            if (src == null) return null;
            return new MToon.MetaDefinition
            {
                Implementation = src.Implementation,
                VersionNumber = src.VersionNumber,
            };
        }

        static MToon.OutlineDefinition ToUnity(this VrmLib.MToon.OutlineDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.OutlineDefinition
            {
                OutlineColor = src.OutlineColor.ToUnitySRGB(),
                OutlineColorMode = (MToon.OutlineColorMode)src.OutlineColorMode,
                OutlineLightingMixValue = src.OutlineLightingMixValue,
                OutlineScaledMaxDistanceValue = src.OutlineScaledMaxDistanceValue,
                OutlineWidthMode = (MToon.OutlineWidthMode)src.OutlineWidthMode,
                OutlineWidthMultiplyTexture = textures.GetOrDefault(src.OutlineWidthMultiplyTexture?.Texture),
                OutlineWidthValue = src.OutlineWidthValue,
            };
        }

        static MToon.RenderingDefinition ToUnity(this VrmLib.MToon.RenderingDefinition src)
        {
            if (src == null) return null;
            return new MToon.RenderingDefinition
            {
                CullMode = (MToon.CullMode)src.CullMode,
                RenderMode = (MToon.RenderMode)src.RenderMode,
                RenderQueueOffsetNumber = src.RenderQueueOffsetNumber,
            };
        }

        static MToon.RimDefinition ToUnity(this VrmLib.MToon.RimDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.RimDefinition
            {
                RimColor = src.RimColor.ToUnityLinear(),
                RimFresnelPowerValue = src.RimFresnelPowerValue,
                RimLiftValue = src.RimLiftValue,
                RimLightingMixValue = src.RimLightingMixValue,
                RimMultiplyTexture = textures.GetOrDefault(src.RimMultiplyTexture?.Texture),
            };
        }

        static MToon.TextureUvCoordsDefinition ToUnity(this VrmLib.MToon.TextureUvCoordsDefinition src, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src == null) return null;
            return new MToon.TextureUvCoordsDefinition
            {
                MainTextureLeftBottomOriginOffset = src.MainTextureLeftBottomOriginOffset.ToUnity(),
                MainTextureLeftBottomOriginScale = src.MainTextureLeftBottomOriginScale.ToUnity(),
                UvAnimationMaskTexture = textures.GetOrDefault(src.UvAnimationMaskTexture?.Texture),
                UvAnimationRotationSpeedValue = src.UvAnimationRotationSpeedValue,
                UvAnimationScrollXSpeedValue = src.UvAnimationScrollXSpeedValue,
                UvAnimationScrollYSpeedValue = src.UvAnimationScrollYSpeedValue,
            };
        }
    }
}
