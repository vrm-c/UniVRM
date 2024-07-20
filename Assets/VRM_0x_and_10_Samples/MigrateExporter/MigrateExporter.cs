using System.IO;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.Sample
{
    [DisallowMultipleComponent]
    public class MigrateExporter : MonoBehaviour
    {
        [SerializeField]
        UniVRM10.VRM10ObjectMeta _meta = new UniVRM10.VRM10ObjectMeta();

        async void OnGUI()
        {
            // validate
            foreach (var validation in _meta.Validate())
            {
                GUILayout.Label("meta validation: " + validation.Message);
                if (!validation.CanExport)
                {
                    GUI.enabled = false;
                }
            }

            GUILayout.Label("ライセンスを変更する権利のある vrm-0.x モデルをロードしてください");
            if (GUILayout.Button("migrate"))
            {
#if UNITY_EDITOR
                var path = UnityEditor.EditorUtility.OpenFilePanel("load vrm-0.x", null, "vrm");
#else
                Debug.LogWarning("no OpenFilePanel for runtime");
                string path = null;
#endif
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                Debug.Log(path);
                var bytes = File.ReadAllBytes(path);

                // load
                var vrm0Instance = await VRM.VrmUtility.LoadBytesAsync(path, bytes);

                // export設定
                var exportConfig = new UniGLTF.GltfExportSettings
                {
                };
                // export vrm0
                var vrm0 = VRM.VRMExporter.Export(exportConfig,
                    vrm0Instance.gameObject, new RuntimeTextureSerializer());
                var vrm0bytes = vrm0.ToGlbBytes();

                // migrate to vrm1
                var vrm1Bytes = UniVRM10.Migrator.Migrate(vrm0bytes, _meta, gltf =>
                {
                    gltf.asset.generator = "MigrateExporter sample";
                });
                var pathObj = PathObject.FromFullPath(path);
                var newPath = pathObj.Parent.Child(pathObj.Stem + ".10.vrm");
                newPath.WriteAllBytes(vrm1Bytes);
                Debug.Log($"export to: {newPath}");
            }
        }
    }
}
