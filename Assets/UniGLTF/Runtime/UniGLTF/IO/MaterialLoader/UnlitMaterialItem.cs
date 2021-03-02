
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public static class UnlitMaterialItem
    {
        public const string ShaderName = "UniGLTF/UniUnlit";

        public static async Task<Material> CreateAsync(IAwaitCaller awaitCaller, glTF gltf, int i, GetTextureAsyncFunc getTexture, bool hasVertexColor)
        {
            if (getTexture == null)
            {
                getTexture = (_x, _y, _z) => Task.FromResult<Texture2D>(default);
            }

            var src = gltf.materials[i];
            var material = MaterialFactory.CreateMaterial(i, src, ShaderName);

            // texture
            if (src.pbrMetallicRoughness.baseColorTexture != null)
            {
                material.mainTexture = await getTexture(awaitCaller, gltf, GetTextureParam.Create(gltf, src.pbrMetallicRoughness.baseColorTexture.index));

                // Texture Offset and Scale
                MaterialFactory.SetTextureOffsetAndScale(material, src.pbrMetallicRoughness.baseColorTexture, "_MainTex");
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
