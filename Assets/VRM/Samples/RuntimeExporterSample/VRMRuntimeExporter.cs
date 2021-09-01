using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using VRMShaders;

namespace VRM.RuntimeExporterSample
{

    public class VRMRuntimeExporter : MonoBehaviour
    {
        [SerializeField] Button m_loadButton = default;

        [SerializeField] Button m_exportButton = default;

        [SerializeField]
        public bool UseNormalize = true;

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

            // GLB形式でJSONを取得しParseします
            var data = new GlbFileParser(path).Parse();
            // VRM extension を parse します
            var vrm = new VRMData(data);
            using (var context = new VRMImporterContext(vrm))
            {

                // metaを取得(todo: thumbnailテクスチャのロード)
                var meta = await context.ReadMetaAsync();
                Debug.LogFormat("meta: title:{0}", meta.Title);

                // ParseしたJSONをシーンオブジェクトに変換していく
                var loaded = await context.LoadAsync();

                loaded.ShowMeshes();
                loaded.EnableUpdateWhenOffscreen();

                OnLoaded(loaded.gameObject);
            }
        }

        void OnLoaded(GameObject go)
        {
            if (m_model != null)
            {
                GameObject.Destroy(m_model.gameObject);
            }

            m_model = go;
            m_model.transform.rotation = Quaternion.Euler(0, 180, 0);
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

            var bytes = UseNormalize ? ExportCustom(m_model) : ExportSimple(m_model);

            File.WriteAllBytes(path, bytes);
            Debug.LogFormat("export to {0}", path);
        }

        static byte[] ExportSimple(GameObject model)
        {
            var vrm = VRMExporter.Export(new UniGLTF.GltfExportSettings(), model, new RuntimeTextureSerializer());
            var bytes = vrm.ToGlbBytes();
            return bytes;
        }

        static byte[] ExportCustom(GameObject exportRoot, bool forceTPose = false)
        {
            // normalize
            var target = VRMBoneNormalizer.Execute(exportRoot, forceTPose);

            try
            {
                return ExportSimple(target);
            }
            finally
            {
                // cleanup
                GameObject.Destroy(target);
            }
        }

        void OnExported(UniGLTF.glTF vrm)
        {
            Debug.LogFormat("exported");
        }

        #endregion
    }
}
