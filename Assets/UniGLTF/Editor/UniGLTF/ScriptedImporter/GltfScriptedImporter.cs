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
            ScriptedImporterImpl.Import(this, ctx, m_reverseAxis);
        }
    }
}
