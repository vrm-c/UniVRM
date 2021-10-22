using MToon;
using UniGLTF;
using UnityEngine;
using VRMShaders;
using RenderMode = MToon.RenderMode;

namespace VRM
{
    public static class VRMUnlitTransparentZWriteMaterialImporter
    {
        public const string UnlitTransparentZWriteShaderName = "VRM/UnlitTransparentZWrite";
        public const string UnlitTransparentZWriteMainTexturePropName = "_MainTex";

        public static bool TryCreateParam(GltfData data, glTF_VRM_extensions vrm, int materialIdx, out MaterialDescriptor matDesc)
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

            matDesc = new MaterialDescriptor(name, MToon.Utils.ShaderName);

            if (vrmMaterial.textureProperties.ContainsKey(UnlitTransparentZWriteMainTexturePropName))
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, UnlitTransparentZWriteMainTexturePropName, out var texture))
                {
                    matDesc.TextureSlots.Add(MToon.Utils.PropMainTex, texture.Item2);
                }
            }

            matDesc.Actions.Add(unityMaterial =>
            {
                var mainTexture = (Texture2D) unityMaterial.GetTexture(MToon.Utils.PropMainTex);

                // NOTE: Unlit のフォールバックなので
                // Lit/Shade 色は黒として、Alpha のために Lit にテクスチャを設定する.
                // Emissive 色は白として、テクスチャを設定する.
                // また、元のシェーダのうちユーザが設定できるプロパティは Texture のみ.
                var def = new MToonDefinition
                {
                    Meta = new MetaDefinition
                    {
                        Implementation = MToon.Utils.Implementation,
                        VersionNumber = MToon.Utils.VersionNumber,
                    },
                    Rendering = new RenderingDefinition
                    {
                        RenderMode = RenderMode.TransparentWithZWrite,
                        CullMode = CullMode.Back,
                        RenderQueueOffsetNumber = 0,
                    },
                    Color = new ColorDefinition
                    {
                        LitColor = Color.black,
                        LitMultiplyTexture = mainTexture,
                        ShadeColor = Color.black,
                        ShadeMultiplyTexture = default,
                        CutoutThresholdValue = 0.5f,
                    },
                    Lighting = new LightingDefinition
                    {
                        LitAndShadeMixing = new LitAndShadeMixingDefinition
                        {
                            ShadingShiftValue = 0,
                            ShadingToonyValue = 1,
                            ShadowReceiveMultiplierValue = 1,
                            ShadowReceiveMultiplierMultiplyTexture = default,
                            LitAndShadeMixingMultiplierValue = 1,
                            LitAndShadeMixingMultiplierMultiplyTexture = default,
                        },
                        LightingInfluence = new LightingInfluenceDefinition
                        {
                            LightColorAttenuationValue = 0,
                            GiIntensityValue = 0.1f,
                        },
                        Normal = new NormalDefinition
                        {
                            NormalTexture = default,
                            NormalScaleValue = 1,
                        },
                    },
                    Emission = new EmissionDefinition
                    {
                        EmissionColor = Color.white,
                        EmissionMultiplyTexture = mainTexture,
                    },
                    MatCap = new MatCapDefinition
                    {
                        AdditiveTexture = default,
                    },
                    Rim = new RimDefinition
                    {
                        RimColor = Color.black,
                        RimMultiplyTexture = default,
                        RimLightingMixValue = 1,
                        RimLiftValue = 0,
                        RimFresnelPowerValue = 1,
                    },
                    Outline = new OutlineDefinition
                    {
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
                        MainTextureLeftBottomOriginScale = new Vector2(1, 1),
                        MainTextureLeftBottomOriginOffset = new Vector2(0, 0),
                        UvAnimationMaskTexture = default,
                        UvAnimationScrollXSpeedValue = 0,
                        UvAnimationScrollYSpeedValue = 0,
                        UvAnimationRotationSpeedValue = 0,
                    },
                };
                MToon.Utils.SetMToonParametersToMaterial(unityMaterial, def);
            });

            Debug.LogWarning($"fallback: {UnlitTransparentZWriteShaderName} => {MToon.Utils.ShaderName}");
            return true;
        }
    }
}
