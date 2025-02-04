using UnityEngine;

namespace UniGLTF
{
    public class UrpGltfMaterialExporter : IMaterialExporter
    {
        public UrpLitMaterialExporter UrpLitExporter { get; set; } = new();
        public UrpUnlitMaterialExporter UrpUnlitExporter { get; set; } = new();
        public UrpUniUnlitMaterialExporter UrpUniUnlitExporter { get; set; } = new();
        public UrpFallbackMaterialExporter FallbackExporter { get; set; } = new();

        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            if (UrpLitExporter.TryExportMaterial(m, textureExporter, out var dst)) return dst;
            if (UrpUnlitExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;
            if (UrpUniUnlitExporter.TryExportMaterial(m, textureExporter, out dst)) return dst;

            UniGLTFLogger.Log($"Material `{m.name}` fallbacks.");
            return FallbackExporter.ExportMaterial(m, textureExporter);
        }
    }
}