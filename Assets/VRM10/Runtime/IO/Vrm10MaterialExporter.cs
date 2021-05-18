using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class Vrm10MaterialExporter : MaterialExporter
    {
        public override glTFMaterial ExportMaterial(Material m, TextureExporter textureExporter)
        {
            if (Vrm10MToonMaterialExporter.TryExportMaterialAsMToon(m, textureExporter, out var dst))
            {
                return dst;
            }
            else
            {
                return base.ExportMaterial(m, textureExporter);
            }
        }
    }
}
