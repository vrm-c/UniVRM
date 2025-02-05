using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MToon;
using UniGLTF;
using UnityEngine;
using RenderMode = MToon.RenderMode;

namespace VRM
{
    public static class BuiltInVrmUnlitTransparentZWriteMaterialImporter
    {
        public const string UnlitTransparentZWriteShaderName = "VRM/UnlitTransparentZWrite";
        public const string UnlitTransparentZWriteMainTexturePropName = "_MainTex";

        public static bool TryCreateParam(GltfData data, glTF_VRM_extensions vrm, int materialIdx,
            out MaterialDescriptor matDesc)
        {
            if (vrm?.materialProperties == null || vrm.materialProperties.Count == 0)
            {
                matDesc = default;
                return false;
            }

            if (materialIdx < 0 || materialIdx >= vrm.materialProperties.Count)
            {
                matDesc = default;
                return false;
            }

            var vrmMaterial = vrm.materialProperties[materialIdx];
            if (vrmMaterial.shader != UnlitTransparentZWriteShaderName)
            {
                matDesc = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            var name = data.GLTF.materials[materialIdx].name;

            var textureSlots = new Dictionary<string, TextureDescriptor>();
            var floatValues = new Dictionary<string, float>();
            var colors = new Dictionary<string, Color>();
            var vectors = new Dictionary<string, Vector4>();
            var actions = new Collection<Action<Material>>();

            if (vrmMaterial.textureProperties.ContainsKey(UnlitTransparentZWriteMainTexturePropName))
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial,
                    UnlitTransparentZWriteMainTexturePropName, out var key, out var desc))
                {
                    textureSlots.Add(MToon.Utils.PropMainTex, desc);
                }
            }

            actions.Add(unityMaterial =>
            {
                var mainTexture = (Texture2D)unityMaterial.GetTexture(MToon.Utils.PropMainTex);

                // NOTE: Unlit のフォールバックなので
                // Lit/Shade 色は黒として、Alpha のために Lit にテクスチャを設定する.
                // Emissive 色は白として、テクスチャを設定する.
                // また、元のシェーダのうちユーザが設定できるプロパティは Texture のみ.
                MToon.Utils.SetMToonParametersToMaterial(unityMaterial, new MToonDefinition
                {
                    Meta = new MetaDefinition
                    {
                        Implementation = MToon.Utils.Implementation,
                        VersionNumber = MToon.Utils.VersionNumber,
                    },
                    Rendering = new RenderingDefinition
                    {
                        // NOTE: Transparent ZWrite
                        RenderMode = RenderMode.TransparentWithZWrite,
                        CullMode = CullMode.Back,
                        RenderQueueOffsetNumber = 0,
                    },
                    Color = new ColorDefinition
                    {
                        // NOTE: Unlit なので、RGB 値は黒とする。
                        // NOTE: Alpha は使うので、テクスチャを設定する.
                        LitColor = new Color(0, 0, 0, 1),
                        LitMultiplyTexture = mainTexture,
                        ShadeColor = new Color(0, 0, 0, 1),
                        ShadeMultiplyTexture = default,
                        CutoutThresholdValue = 0.5f,
                    },
                    Lighting = new LightingDefinition
                    {
                        LitAndShadeMixing = new LitAndShadeMixingDefinition
                        {
                            // NOTE: default value
                            ShadingShiftValue = 0,
                            ShadingToonyValue = 1,
                            ShadowReceiveMultiplierValue = 1,
                            ShadowReceiveMultiplierMultiplyTexture = default,
                            LitAndShadeMixingMultiplierValue = 1,
                            LitAndShadeMixingMultiplierMultiplyTexture = default,
                        },
                        LightingInfluence = new LightingInfluenceDefinition
                        {
                            // NOTE: default value
                            LightColorAttenuationValue = 0,
                            GiIntensityValue = 0.1f,
                        },
                        Normal = new NormalDefinition
                        {
                            // NOTE: default value
                            NormalTexture = default,
                            NormalScaleValue = 1,
                        },
                    },
                    Emission = new EmissionDefinition
                    {
                        // NOTE: Unlit なので Emission にテクスチャを設定する.
                        EmissionColor = Color.white,
                        EmissionMultiplyTexture = mainTexture,
                    },
                    MatCap = new MatCapDefinition
                    {
                        // NOTE: default value
                        AdditiveTexture = default,
                    },
                    Rim = new RimDefinition
                    {
                        // NOTE: default value
                        RimColor = Color.black,
                        RimMultiplyTexture = default,
                        RimLightingMixValue = 1,
                        RimLiftValue = 0,
                        RimFresnelPowerValue = 1,
                    },
                    Outline = new OutlineDefinition
                    {
                        // NOTE: default value
                        OutlineWidthMode = OutlineWidthMode.None,
                        OutlineWidthValue = 0,
                        OutlineWidthMultiplyTexture = default,
                        OutlineScaledMaxDistanceValue = 1,
                        OutlineColorMode = OutlineColorMode.FixedColor,
                        OutlineColor = Color.black,
                        OutlineLightingMixValue = 1,
                    },
                    TextureOption = new TextureUvCoordsDefinition
                    {
                        // NOTE: default value
                        MainTextureLeftBottomOriginScale = new Vector2(1, 1),
                        MainTextureLeftBottomOriginOffset = new Vector2(0, 0),
                        UvAnimationMaskTexture = default,
                        UvAnimationScrollXSpeedValue = 0,
                        UvAnimationScrollYSpeedValue = 0,
                        UvAnimationRotationSpeedValue = 0,
                    },
                });

                // NOTE: MToon として正しくはないが、やむをえず renderQueue を元の値で復帰する.
                unityMaterial.renderQueue = vrmMaterial.renderQueue;
            });

            matDesc = new MaterialDescriptor(
                name,
                Shader.Find(Utils.ShaderName),
                null,
                textureSlots,
                floatValues,
                colors,
                vectors,
                actions);

            UniGLTFLogger.Warning($"fallback: {UnlitTransparentZWriteShaderName} => {MToon.Utils.ShaderName}");
            return true;
        }
    }
}