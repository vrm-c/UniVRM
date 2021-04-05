using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace VRM.Samples
{

    public class VRMRuntimeExporter : MonoBehaviour
    {
        [SerializeField] Button m_loadButton = default;

        [SerializeField] Button m_exportButton = default;

        GameObject m_model;

        private void Awake()
        {
            m_loadButton.onClick.AddListener(OnLoadClicked);

            m_exportButton.onClick.AddListener(OnExportClicked);
        }

        private void Update()
        {
            m_exportButton.interactable = (m_model != null);
        }

        #region Load

        async void OnLoadClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
        var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            // GLB形式でJSONを取得しParseします
            var parser = new GltfParser();
            parser.ParseGlb(bytes);

            using (var context = new VRMImporterContext(parser))
            {

                // metaを取得(todo: thumbnailテクスチャのロード)
                var meta = await context.ReadMetaAsync();
                Debug.LogFormat("meta: title:{0}", meta.Title);

                // ParseしたJSONをシーンオブジェクトに変換していく
                await context.LoadAsync();

                OnLoaded(context);
            }
        }

        void OnLoaded(VRMImporterContext context)
        {
            if (m_model != null)
            {
                GameObject.Destroy(m_model.gameObject);
            }

            m_model = context.Root;
            m_model.transform.rotation = Quaternion.Euler(0, 180, 0);

            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();
        }

        #endregion

        #region Export

        void OnExportClicked()
        {
            //#if UNITY_STANDALONE_WIN
#if false
        var path = FileDialogForWindows.SaveDialog("save VRM", Application.dataPath + "/export.vrm");
#else
            var path = Application.dataPath + "/../export.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var vrm = VRMExporter.Export(UniGLTF.MeshExportSettings.Default, m_model, _ => false);
            var bytes = vrm.ToGlbBytes();
            File.WriteAllBytes(path, bytes);
            Debug.LogFormat("export to {0}", path);
        }

        void OnExported(UniGLTF.glTF vrm)
        {
            Debug.LogFormat("exported");
        }

        #endregion
    }
}
