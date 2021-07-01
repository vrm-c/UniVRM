#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    [ScriptedImporter(1, "gltf")]
    public class GltfScriptedImporter : GltfScriptedImporterBase
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Import(this, ctx, m_reverseAxis.ToAxes());
        }
    }
}
