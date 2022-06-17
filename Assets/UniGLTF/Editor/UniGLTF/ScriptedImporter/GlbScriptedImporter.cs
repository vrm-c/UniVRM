#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
#if UNITY_2020_2_OR_NEWER
#if UNIGLTF_DISABLE_DEFAULT_GLB_IMPORTER
    [ScriptedImporter(1, null, overrideExts: new[] { "glb" })]
#else
    [ScriptedImporter(1, new[] { "glb" })]
#endif
#else
	[ScriptedImporter(1, new[] { "glb" })]
#endif
    public class GlbScriptedImporter : GltfScriptedImporterBase
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Import(this, ctx, m_reverseAxis.ToAxes(), m_renderPipeline);
        }
    }
}
