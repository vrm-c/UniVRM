using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class BuiltinRPVrm10MaterialExporter : BuiltinRPGltfMaterialExporter
    {
        public override glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            if (BuiltinRPVrm10MToonMaterialExporter.TryExportMaterialAsMToon(m, textureExporter, out var dst))
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
