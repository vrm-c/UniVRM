using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    public static class GltfUnlitMaterialImporter
    {
        public const string ShaderName = "UniGLTF/UniUnlit";

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

            matDesc = new MaterialDescriptor(GltfMaterialDescriptorGenerator.GetMaterialName(i, src), ShaderName);

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
                var (key, textureParam) = GltfTextureImporter.CreateSRGB(data, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
                matDesc.TextureSlots.Add("_MainTex", textureParam);
            }

            // color
            if (src.pbrMetallicRoughness.baseColorFactor != null && src.pbrMetallicRoughness.baseColorFactor.Length == 4)
            {
                matDesc.Colors.Add("_Color",
                    src.pbrMetallicRoughness.baseColorFactor.ToColor4(ColorSpace.Linear, ColorSpace.sRGB)
                );
            }

            //renderMode
            matDesc.Actions.Add(material =>
            {
                if (src.alphaMode == "OPAQUE")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
                }
                else if (src.alphaMode == "BLEND")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Transparent);
                }
                else if (src.alphaMode == "MASK")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Cutout);
                    material.SetFloat("_Cutoff", src.alphaCutoff);
                }
                else
                {
                    // default OPAQUE
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
                }

                // culling
                if (src.doubleSided)
                {
                    UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Off);
                }
                else
                {
                    UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Back);
                }

                // VColor
                var hasVertexColor = data.GLTF.MaterialHasVertexColor(i);
                if (hasVertexColor)
                {
                    UniUnlit.Utils.SetVColBlendMode(material, UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
                }

                UniUnlit.Utils.ValidateProperties(material, true);
            });

            return true;
        }
    }
}
