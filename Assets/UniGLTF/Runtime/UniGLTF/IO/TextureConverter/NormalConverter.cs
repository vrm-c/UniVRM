using UnityEngine;

namespace UniGLTF
{
    public class NormalConverter : ITextureConverter
    {
        private Material m_decoder;
        private Material GetDecoder()
        {
            if (m_decoder == null)
            {
                m_decoder = new Material(Shader.Find("UniGLTF/NormalMapDecoder"));
            }
            return m_decoder;
        }

        private Material m_encoder;
        private Material GetEncoder()
        {
            if (m_encoder == null)
            {
                m_encoder = new Material(Shader.Find("UniGLTF/NormalMapEncoder"));
            }
            return m_encoder;
        }

        // GLTF data to Unity texture
        // ConvertToNormalValueFromRawColorWhenCompressionIsRequired
        public Texture2D GetImportTexture(Texture2D texture)
        {
            var mat = GetEncoder();
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Normal, null, mat);
            return converted;
        }

        // Unity texture to GLTF data
        // ConvertToRawColorWhenNormalValueIsCompressed
        public Texture2D GetExportTexture(Texture2D texture)
        {
            var mat = GetDecoder();
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Normal, null, mat);
            return converted;
        }
    }

}