using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public class UnlitMaterialItem : MaterialItemBase
    {
        public const string ShaderName = "UniGLTF/UniUnlit";

        bool m_hasVertexColor;

        public UnlitMaterialItem(int i, glTFMaterial src, bool hasVertexColor) : base(i, src)
        {
            m_hasVertexColor = hasVertexColor;
        }

        public override async Task<Material> GetOrCreateAsync(GetTextureAsyncFunc getTexture)
        {
            if (getTexture == null)
            {
                getTexture = _ => Task.FromResult<Texture2D>(null);
            }

            var material = CreateMaterial(ShaderName);

            // texture
            if (m_src.pbrMetallicRoughness.baseColorTexture != null)
            {
                material.mainTexture = await getTexture(GetTextureParam.Create(m_src.pbrMetallicRoughness.baseColorTexture.index));

                // Texture Offset and Scale
                SetTextureOffsetAndScale(material, m_src.pbrMetallicRoughness.baseColorTexture, "_MainTex");
            }

            // color
            if (m_src.pbrMetallicRoughness.baseColorFactor != null && m_src.pbrMetallicRoughness.baseColorFactor.Length == 4)
            {
                var color = m_src.pbrMetallicRoughness.baseColorFactor;
                material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
            }

            //renderMode
            if (m_src.alphaMode == "OPAQUE")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
            }
            else if (m_src.alphaMode == "BLEND")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Transparent);
            }
            else if (m_src.alphaMode == "MASK")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Cutout);
                material.SetFloat("_Cutoff", m_src.alphaCutoff);
            }
            else
            {
                // default OPAQUE
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
            }

            // culling
            if (m_src.doubleSided)
            {
                UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Off);
            }
            else
            {
                UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Back);
            }

            // VColor
            if (m_hasVertexColor)
            {
                UniUnlit.Utils.SetVColBlendMode(material, UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
            }

            UniUnlit.Utils.ValidateProperties(material, true);

            return material;
        }
    }
}
