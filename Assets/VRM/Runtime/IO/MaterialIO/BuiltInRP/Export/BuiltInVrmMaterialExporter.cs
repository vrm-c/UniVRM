using System;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class BuiltInVrmMaterialExporter : IMaterialExporter
    {
        public static readonly string[] SupportedShaderNames =
        {
            BuiltInVrmMToonMaterialExporter.TargetShaderName,
            "VRM/UnlitTexture",
            "VRM/UnlitTransparent",
            "VRM/UnlitCutout",
            "VRM/UnlitTransparentZWrite",
        };

        private readonly BuiltInGltfMaterialExporter _gltfExporter = new BuiltInGltfMaterialExporter();

        public glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            glTFMaterial dst = default;
            switch (src.shader.name)
            {
                case BuiltInVrmMToonMaterialExporter.TargetShaderName:
                    if (BuiltInVrmMToonMaterialExporter.TryExportMaterial(src, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTexture":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTransparent":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitCutout":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.MASK, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTransparentZWrite":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
            }

            return _gltfExporter.ExportMaterial(src, textureExporter, settings);
        }
    }
}
