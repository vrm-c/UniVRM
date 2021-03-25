
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public static class UnlitMaterialItem
    {
        public const string ShaderName = "UniGLTF/UniUnlit";

        public static async Task<Material> CreateAsync(IAwaitCaller awaitCaller, GltfParser parser, int i, GetTextureAsyncFunc getTexture, bool hasVertexColor)
        {
            if (getTexture == null)
            {
                getTexture = (_) => Task.FromResult<Texture2D>(default);
            }

            var src = parser.GLTF.materials[i];
            var material = new Material(Shader.Find(ShaderName));
            material.name = MaterialFactory.MaterialName(i, src);

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
                material.mainTexture = await getTexture(GltfTextureImporter.CreateSRGB(parser, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale));

                // Texture Offset and Scale
                MaterialFactory.SetTextureOffsetAndScale(material, "_MainTex", offset, scale);
            }

            // color
            if (src.pbrMetallicRoughness.baseColorFactor != null && src.pbrMetallicRoughness.baseColorFactor.Length == 4)
            {
                var color = src.pbrMetallicRoughness.baseColorFactor;
                material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
            }

            //renderMode
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
            if (hasVertexColor)
            {
                UniUnlit.Utils.SetVColBlendMode(material, UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
            }

            UniUnlit.Utils.ValidateProperties(material, true);

            return material;
        }
    }
}
