using System;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class BuiltinRPVrmMaterialExporter : BuiltinRPGltfMaterialExporter
    {
        public static readonly string[] SupportedShaderNames =
        {
            BuiltinRPVrmMToonMaterialExporter.TargetShaderName,
            "VRM/UnlitTexture",
            "VRM/UnlitTransparent",
            "VRM/UnlitCutout",
            "VRM/UnlitTransparentZWrite",
        };

        public override glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            glTFMaterial dst = default;
            switch (src.shader.name)
            {
                case BuiltinRPVrmMToonMaterialExporter.TargetShaderName:
                    if (BuiltinRPVrmMToonMaterialExporter.TryExportMaterial(src, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTexture":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTransparent":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitCutout":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.MASK, textureExporter, out dst)) return dst;
                    break;
                case "VRM/UnlitTransparentZWrite":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(src, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
            }

            return base.ExportMaterial(src, textureExporter, settings);
        }
    }
}
