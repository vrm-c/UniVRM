using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class BuiltInVrm10MaterialExporter : BuiltInGltfMaterialExporter
    {
        public override glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            if (BuiltInVrm10MToonMaterialExporter.TryExportMaterialAsMToon(m, textureExporter, out var dst))
            {
                return dst;
            }
            else
            {
                return base.ExportMaterial(m, textureExporter, settings);
            }
        }
    }
}
