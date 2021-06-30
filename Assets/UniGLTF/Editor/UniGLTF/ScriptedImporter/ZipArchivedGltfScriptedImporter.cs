using UnityEngine;
using UnityEditor;
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
    public class ZipArchivedGltfScriptedImporter : ScriptedImporter
    {
        [SerializeField]
        public ScriptedImporterAxes m_reverseAxis = default;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ScriptedImporterImpl.Import(this, ctx, m_reverseAxis.ToAxes());
        }
    }
}
