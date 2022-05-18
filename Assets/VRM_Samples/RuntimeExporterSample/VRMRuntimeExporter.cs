using System.IO;
using UnityEngine;
using VRMShaders;


namespace VRM.RuntimeExporterSample
{

    public class VRMRuntimeExporter : MonoBehaviour
    {
        [SerializeField]
        public bool UseNormalize = true;

        GameObject m_model;

        void OnGUI()
        {
            if (GUILayout.Button("Load"))
            {
                OnLoadClicked();
            }

            if (GUILayout.Button("Export"))
            {
                OnExportClicked();
            }
        }

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

            var loaded = await VrmUtility.LoadAsync(path);

            loaded.ShowMeshes();
            loaded.EnableUpdateWhenOffscreen();
            OnLoaded(loaded.gameObject);
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
    }
}
