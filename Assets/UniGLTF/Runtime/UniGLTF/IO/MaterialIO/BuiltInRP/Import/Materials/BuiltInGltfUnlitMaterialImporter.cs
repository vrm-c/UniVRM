using System;
using System.Collections.Generic;
using UniGLTF.UniUnlit;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// A class that generates MaterialDescriptor for "UnGLTF/UniUnlit" shader based on glTF Extension "KHR_materials_unlit".
    ///
    /// https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_unlit
    /// </summary>
    public class BuiltInGltfUnlitMaterialImporter
    {
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");

        /// <summary>
        /// Can be replaced with custom shaders that are compatible with "UniGLTF/UniUnlit" properties and keywords.
        /// </summary>
        public Shader Shader { get; set; }

        public BuiltInGltfUnlitMaterialImporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find(UniUnlitUtil.ShaderName);
        }

        public bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
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