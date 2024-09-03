using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public class UrpVrm10MaterialExporter : IMaterialExporter
    {
        public glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            #if UNITY_EDITOR
            return new glTFMaterial{
                name = "dummyForTest",
            };
            #else
            throw new System.NotImplementedException();
            #endif
        }
    }
}
