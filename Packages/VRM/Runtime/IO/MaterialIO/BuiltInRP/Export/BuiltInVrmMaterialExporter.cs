using System;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public class BuiltInVrmMaterialExporter : IMaterialExporter
    {
        public static readonly string[] SupportedShaderNames =
        {
            BuiltInVrmMToonMaterialExporter.TargetShaderName,
        };

        private readonly BuiltInGltfMaterialExporter _gltfExporter = new BuiltInGltfMaterialExporter();

        public glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            switch (src.shader.name)
            {
                case BuiltInVrmMToonMaterialExporter.TargetShaderName:
                    if (BuiltInVrmMToonMaterialExporter.TryExportMaterial(src, textureExporter, out var dst)) return dst;
                    break;
            }

            return _gltfExporter.ExportMaterial(src, textureExporter, settings);
        }
    }
}
