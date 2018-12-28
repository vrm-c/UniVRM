using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public class TextureExportManager
    {
        List<Texture> m_textures;
        public List<Texture> Textures
        {
            get { return m_textures; }
        }

        List<Texture> m_exportTextures;
        public Texture GetExportTexture(int index)
        {
            if (index < 0 || index >= m_exportTextures.Count)
            {
                return null;
            }
            if (m_exportTextures[index] != null)
            {
                // コピー変換済み
                return m_exportTextures[index];
            }

            // オリジナル
            return m_textures[index];
        }

        public TextureExportManager(IEnumerable<Texture> textures)
        {
            /*
            if (textures == null)
            {
                throw new System.ArgumentNullException();
            }
			*/
            m_textures = textures.ToList();
            m_exportTextures = new List<Texture>(Enumerable.Repeat<Texture>(null, m_textures.Count));
        }

        public int CopyAndGetIndex(Texture texture, RenderTextureReadWrite readWrite)
        {
            if (texture == null)
            {
                return -1;
            }

            var index = m_textures.IndexOf(texture);
            if (index == -1)
            {
                // ありえない？
                return -1;
            }

            // ToDo: may already exists
            m_exportTextures[index] = TextureItem.CopyTexture(texture, readWrite, null);

            return index;
        }

        public int ConvertAndGetIndex(Texture texture, ITextureConverter converter)
        {
            if (texture == null)
            {
                return -1;
            }

            var index = m_textures.IndexOf(texture);
            if (index == -1)
            {
                // ありえない？
                return -1;
            }

            m_exportTextures[index] = converter.GetExportTexture(texture as Texture2D);

            return index;
        }
    }
}
