using UnityEngine;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    [ScriptedImporter(1, "glb")]
    public class GlbScriptedImporter : ScriptedImporter
    {
        [SerializeField]
        public ScriptedImporterAxes m_reverseAxis;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ScriptedImporterImpl.Import(this, ctx, m_reverseAxis.ToAxes());
        }
    }
}
