using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace UniGLTF
{
    [ScriptedImporter(1, "gltf")]
    public class GltfScriptedImporter : ScriptedImporter
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
