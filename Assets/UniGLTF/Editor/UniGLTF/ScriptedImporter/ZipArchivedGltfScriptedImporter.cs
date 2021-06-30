#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
#if UNIGLTF_ENABLE_ZIPARCHVIE_IMPORTER
    [ScriptedImporter(1, "zip")]
#endif
    public class ZipArchivedGltfScriptedImporter : GltfScriptedImporterBase
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Import(this, ctx, m_reverseAxis.ToAxes());
        }
    }
}
