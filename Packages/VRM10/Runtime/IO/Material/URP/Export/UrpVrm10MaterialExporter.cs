using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public class UrpVrm10MaterialExporter : IMaterialExporter
    {
        public UrpGltfMaterialExporter GltfExporter { get; set; } = new();
        public UrpVrm10MToonMaterialExporter MToonExporter { get; set; } = new();

        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            if (MToonExporter.TryExportMaterial(m, textureExporter, out var dst))
            {
                return dst;
            }
            else
            {
                return GltfExporter.ExportMaterial(m, textureExporter, settings);
            }
        }
    }
}
