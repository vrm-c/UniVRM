using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public class TextureExportManager
    {
        public int GetTextureIndex(Texture src, glTFTextureTypes textureType)
        {
            throw new NotImplementedException();
        }

        public int ExportSRGB(Texture src)
        {
            throw new NotImplementedException();
        }

        public int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture != null && occlusionTexture != null)
            {
                if (metallicSmoothTexture != occlusionTexture)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (metallicSmoothTexture)
            {
                throw new NotImplementedException();
            }
            else if (occlusionTexture)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public int ExportNormal(Texture normalTexture)
        {
            throw new NotImplementedException();
        }

        //         List<Texture> m_exportTextures;
        //         public Texture GetExportTexture(int index)
        //         {
        //             if (index < 0 || index >= m_exportTextures.Count)
        //             {
        //                 return null;
        //             }
        //             if (m_exportTextures[index] != null)
        //             {
        //                 // コピー変換済み
        //                 return m_exportTextures[index];
        //             }

        //             // オリジナル
        //             return m_textures[index];
        //         }

        //         public TextureExportManager(IEnumerable<Texture> textures)
        //         {
        //             if (textures == null)
        //             {
        //                 // empty list for UnitTest
        //                 textures = new Texture[] { };
        //             }
        //             m_textures = textures.ToList();
        //             m_exportTextures = new List<Texture>(Enumerable.Repeat<Texture>(null, m_textures.Count));
        //         }

        //         public int CopyAndGetIndex(Texture texture, glTFTextureTypes readWrite)
        //         {
        //             if (texture == null)
        //             {
        //                 return -1;
        //             }

        //             var index = m_textures.IndexOf(texture);
        //             if (index == -1)
        //             {
        //                 // ありえない？
        //                 return -1;
        //             }

        // #if UNITY_EDITOR
        //             if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(texture)))
        //             {
        //                 m_exportTextures[index] = texture;
        //                 return index;
        //             }
        // #endif

        //             // ToDo: may already exists
        //             m_exportTextures[index] = TextureConverter.CopyTexture(texture, readWrite, null);

        //             return index;
        //         }

        //         public int ConvertAndGetIndex(Texture texture, Func<Texture, Texture2D> converter)
        //         {
        //             if (texture == null)
        //             {
        //                 return -1;
        //             }

        //             var index = m_textures.IndexOf(texture);
        //             if (index == -1)
        //             {
        //                 // ありえない？
        //                 return -1;
        //             }

        //             m_exportTextures[index] = converter(texture);

        //             return index;
        //         }
    }
}
