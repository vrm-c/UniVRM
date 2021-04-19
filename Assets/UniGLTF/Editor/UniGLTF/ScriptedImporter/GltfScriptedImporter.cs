using UnityEngine;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    [ScriptedImporter(1, "gltf")]
    public class GltfScriptedImporter : ScriptedImporter
    {
        [SerializeField]
        Axises m_reverseAxis = default;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ctx.assetPath);
            if (asset == null)
            {
                // first time. set default setting
                m_reverseAxis = UniGLTFPreference.GltfIOAxis;
            }
            ScriptedImporterImpl.Import(this, ctx, m_reverseAxis);
        }
    }
}
