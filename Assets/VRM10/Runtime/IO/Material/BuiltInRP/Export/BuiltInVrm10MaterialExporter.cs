using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public class BuiltInVrm10MaterialExporter : IMaterialExporter
    {
        private readonly BuiltInGltfMaterialExporter _gltfExporter = new BuiltInGltfMaterialExporter();

        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            if (BuiltInVrm10MToonMaterialExporter.TryExportMaterialAsMToon(m, textureExporter, out var dst))
            {
                return dst;
            }
            else
            {
                return _gltfExporter.ExportMaterial(m, textureExporter, settings);
            }
        }
    }
}
