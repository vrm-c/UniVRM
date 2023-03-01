using System;
using System.Collections.Generic;
using UniGLTF.UniUnlit;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    public static class BuiltInGltfUnlitMaterialImporter
    {
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");

        public static Shader Shader => Shader.Find(UniUnlitUtil.ShaderName);

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

            var colors = new Dictionary<string, Color>();
            var textureSlots = new Dictionary<string, TextureDescriptor>();

            // color
            var baseColorFactor = GltfMaterialImportUtils.ImportLinearBaseColorFactor(data, src);
            if (baseColorFactor.HasValue)
            {
                colors.Add("_Color", baseColorFactor.Value.gamma);
            }

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
                if (GltfTextureImporter.TryCreateSrgb(data, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale, out var key, out var desc))
                {
                    textureSlots.Add("_MainTex", desc);
                }
            }

            matDesc = new MaterialDescriptor(
                GltfMaterialImportUtils.ImportMaterialName(i, src),
                Shader,
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