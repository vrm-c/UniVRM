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

        // GLTF data to Unity texture
        public static Texture2D Import(Texture2D texture)
        {
            return texture;
        }

        // Unity texture to GLTF data
        public static Texture2D Export(Texture texture)
        {
            return TextureConverter.CopyTexture(texture, ColorSpace.Linear, false, Decoder);
        }
    }
}
