using UnityEngine;

namespace UniGLTF
{
    public class OcclusionConverter : ITextureConverter
    {

        public Texture2D GetImportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Occlusion, Import, null);
            return converted;
        }

        public Texture2D GetExportTexture(Texture2D texture)
        {
            var converted = TextureConverter.Convert(texture, glTFTextureTypes.Occlusion, Export, null);
            return converted;
        }

        public Color32 Import(Color32 src)
        {
            return new Color32
            {
                r = 0,
                g = src.r,
                b = 0,
                a = 255,
            };
        }

        public Color32 Export(Color32 src)
        {
            return new Color32
            {
                r = src.g,
                g = 0,
                b = 0,
                a = 255,
            };
        }
    }
}