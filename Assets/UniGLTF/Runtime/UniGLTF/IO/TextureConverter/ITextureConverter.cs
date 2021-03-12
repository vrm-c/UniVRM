using UnityEngine;

namespace UniGLTF
{
    public interface ITextureConverter
    {
        Texture2D GetExportTexture(Texture2D texture);
    }
}
