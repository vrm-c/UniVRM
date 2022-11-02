using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class BuiltinRPGltfMaterialExporter : IMaterialExporter
    {
        public static readonly string[] SupportedShaderNames =
        {
            BuiltinRPStandardMaterialExporter.TargetShaderName,
            BuiltinRPUniUnlitMaterialExporter.TargetShaderName,
            "Unlit/Color",
            "Unlit/Texture",
            "Unlit/Transparent",
            "Unlit/Transparent Cutout",
        };

        public virtual glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            glTFMaterial dst;
            switch (m.shader.name)
            {
                case BuiltinRPStandardMaterialExporter.TargetShaderName:
                    if (BuiltinRPStandardMaterialExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;
                    break;
                case BuiltinRPUniUnlitMaterialExporter.TargetShaderName:
                    if (BuiltinRPUniUnlitMaterialExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Color":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Texture":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Transparent":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Transparent Cutout":
                    if (BuiltinRPGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.MASK, textureExporter, out dst)) return dst;
                    break;
            }

            return BuiltinRPFallbackMaterialExporter.ExportMaterial(m, textureExporter);
        }
    }
}
