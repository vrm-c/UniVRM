
using UnityEngine;
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
        Axises m_reverseAxis = default;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            try
            {
                ScriptedImporterImpl.Import(this, ctx, m_reverseAxis);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
