using UnityEngine;

namespace UniGLTF
{
    public class BuiltInGltfMaterialExporter : IMaterialExporter
    {
        public static readonly string[] SupportedShaderNames =
        {
            BuiltInStandardMaterialExporter.TargetShaderName,
            BuiltInUniUnlitMaterialExporter.TargetShaderName,
            "Unlit/Color",
            "Unlit/Texture",
            "Unlit/Transparent",
            "Unlit/Transparent Cutout",
        };

        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            glTFMaterial dst;
            switch (m.shader.name)
            {
                case BuiltInStandardMaterialExporter.TargetShaderName:
                    if (BuiltInStandardMaterialExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;
                    break;
                case BuiltInUniUnlitMaterialExporter.TargetShaderName:
                    if (BuiltInUniUnlitMaterialExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Color":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Texture":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.OPAQUE, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Transparent":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.BLEND, textureExporter, out dst)) return dst;
                    break;
                case "Unlit/Transparent Cutout":
                    if (BuiltInGenericUnlitMaterialExporter.TryExportMaterial(m, glTFBlendMode.MASK, textureExporter, out dst)) return dst;
                    break;
            }

            Debug.Log($"Material `{m.name}` fallbacks.");
            return BuiltInFallbackMaterialExporter.ExportMaterial(m, textureExporter);
        }
    }
}
