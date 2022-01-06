using System;
using System.Collections.Generic;
using UniGLTF.UniUnlit;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    public static class GltfUnlitMaterialImporter
    {
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");

        public static bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (i < 0 || i >= data.GLTF.materials.Count)
            {
                matDesc = default;
                return false;
            }

            var src = data.GLTF.materials[i];
            if (!glTF_KHR_materials_unlit.IsEnable(src))
            {
                matDesc = default;
                return false;
            }

            var textureSlots = new Dictionary<string, TextureDescriptor>();
            var colors =
                src.pbrMetallicRoughness.baseColorFactor != null &&
                src.pbrMetallicRoughness.baseColorFactor.Length == 4
                    ? new Dictionary<string, Color>
                    {
                        {
                            "_Color",
                            src.pbrMetallicRoughness.baseColorFactor.ToColor4(ColorSpace.Linear, ColorSpace.sRGB)
                        }
                    }
                    : new Dictionary<string, Color>();

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var (offset, scale) =
                    GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
                var (key, textureParam) = GltfTextureImporter.CreateSrgb(data,
                    src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
                textureSlots.Add("_MainTex", textureParam);
            }

            matDesc = new MaterialDescriptor(
                GltfMaterialDescriptorGenerator.GetMaterialName(i, src),
                UniUnlitUtil.ShaderName,
                null,
                textureSlots,
                new Dictionary<string, float>(),
                colors,
                new Dictionary<string, Vector4>(),
                new Action<Material>[]
                {
                    //renderMode
                    material =>
                    {
                        switch (src.alphaMode)
                        {
                            case "OPAQUE":
                                UniUnlitUtil.SetRenderMode(material, UniUnlitRenderMode.Opaque);
                                break;
                            case "BLEND":
                                UniUnlitUtil.SetRenderMode(material, UniUnlitRenderMode.Transparent);
                                break;
                            case "MASK":
                                UniUnlitUtil.SetRenderMode(material, UniUnlitRenderMode.Cutout);
                                material.SetFloat(Cutoff, src.alphaCutoff);
                                break;
                            default:
                                // default OPAQUE
                                UniUnlitUtil.SetRenderMode(material, UniUnlitRenderMode.Opaque);
                                break;
                        }

                        // culling
                        if (src.doubleSided)
                        {
                            UniUnlitUtil.SetCullMode(material, UniUnlitCullMode.Off);
                        }
                        else
                        {
                            UniUnlitUtil.SetCullMode(material, UniUnlitCullMode.Back);
                        }

                        // VColor
                        var hasVertexColor = data.MaterialHasVertexColor(i);
                        if (hasVertexColor)
                        {
                            UniUnlitUtil.SetVColBlendMode(material, UniUnlitVertexColorBlendOp.Multiply);
                        }

                        UniUnlitUtil.ValidateProperties(material, true);
                    }
                }
            );

            return true;
        }
    }
}