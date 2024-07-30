using UnityEngine;

namespace UniGLTF
{
    public class UrpGltfMaterialExporter : IMaterialExporter
    {
        private readonly UrpLitMaterialExporter _litExporter = new();

        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            glTFMaterial dst;

            if (_litExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;

            return dst;
        }
    }
}