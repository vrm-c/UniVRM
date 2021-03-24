using UnityEngine;


namespace VRMShaders
{
    public static class NormalConverter
    {
        private static Material m_decoder;
        private static Material Decoder
        {
            get
            {
                if (m_decoder == null)
                {
                    m_decoder = new Material(Shader.Find("UniGLTF/NormalMapDecoder"));
                }
                return m_decoder;
            }
        }

        private static Material m_encoder;
        private static Material Encoder
        {
            get
            {
                if (m_encoder == null)
                {
                    m_encoder = new Material(Shader.Find("UniGLTF/NormalMapEncoder"));
                }
                return m_encoder;
            }
        }

        // GLTF data to Unity texture
        // ConvertToNormalValueFromRawColorWhenCompressionIsRequired
        public static Texture2D Import(Texture2D texture)
        {
            return TextureConverter.Convert(texture, TextureImportTypes.NormalMap, null, Encoder);
        }

        // Unity texture to GLTF data
        // ConvertToRawColorWhenNormalValueIsCompressed
        public static Texture2D Export(Texture texture)
        {
            return TextureConverter.Convert(texture, TextureImportTypes.NormalMap, null, Decoder);
        }
    }
}
