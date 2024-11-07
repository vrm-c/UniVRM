using UniGLTF;
using UnityEngine;
using VRM10.MToon10;

namespace UniVRM10
{
    public class UrpVrm10MToonMaterialExporter
    {
        private readonly BuiltInVrm10MToonMaterialExporter _builtInExporter;

        public Shader Shader
        {
            get => _builtInExporter.Shader;
            set => _builtInExporter.Shader = value;
        }

        public UrpVrm10MToonMaterialExporter(Shader shader = null)
        {
            _builtInExporter = new BuiltInVrm10MToonMaterialExporter(
                shader != null ? shader : Shader.Find(MToon10Meta.UnityUrpShaderName)
            );
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            return _builtInExporter.TryExportMaterial(src, textureExporter, out dst);
        }
    }
}