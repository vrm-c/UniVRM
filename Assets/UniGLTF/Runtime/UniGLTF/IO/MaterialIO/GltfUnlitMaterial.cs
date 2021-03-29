using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    public static class GltfUnlitMaterial
    {
        public const string ShaderName = "UniGLTF/UniUnlit";

        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            if (i < 0 || i >= parser.GLTF.materials.Count)
            {
                param = default;
                return false;
            }

            var src = parser.GLTF.materials[i];
            if (!glTF_KHR_materials_unlit.IsEnable(src))
            {
                param = default;
                return false;
            }
            
            param = new MaterialImportParam(GltfMaterialImporter.MaterialName(i, src), ShaderName);

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
                var textureParam = GltfTextureImporter.CreateSRGB(parser, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
                param.TextureSlots.Add("_MainTex", textureParam);
            }

            // color
            if (src.pbrMetallicRoughness.baseColorFactor != null && src.pbrMetallicRoughness.baseColorFactor.Length == 4)
            {
                var color = src.pbrMetallicRoughness.baseColorFactor;
                param.Colors.Add("_Color", (new Color(color[0], color[1], color[2], color[3])).gamma);
            }

            //renderMode
            param.Actions.Add(material =>
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
                var hasVertexColor = parser.GLTF.MaterialHasVertexColor(i);
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
