using UnityEngine;

namespace UniGLTF
{
    public interface ITextureConverter
    {
        Texture2D GetImportTexture(Texture2D texture);
        Texture2D GetExportTexture(Texture2D texture);
    }
}
