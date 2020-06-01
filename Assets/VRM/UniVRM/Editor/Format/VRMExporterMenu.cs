using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public class VRMExporterWizard : ScriptableWizard
    {
        const string EXTENSION = ".vrm";

        VRMMeta m_meta;
       
        private static string m_lastExportDir;

        public VRMExportSettings m_settings = new VRMExportSettings();

        public static void CreateWizard()
        {
            var wiz = ScriptableWizard.DisplayWizard<VRMExporterWizard>(
                "VRM Exporter", "Export");
            var go = Selection.activeObject as GameObject;

            // update checkbox
            wiz.m_settings.InitializeFrom(go);

            wiz.OnWizardUpdate();
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += OnWizardUpdate;
        }

        void OnDisable()
        {
            // Debug.Log("OnDisable");
            Undo.willFlushUndoRecord -= OnWizardUpdate;
        }

        void OnWizardCreate()
        {
            string directory;
            if (string.IsNullOrEmpty(m_lastExportDir))
                directory = Directory.GetParent(Application.dataPath).ToString();
            else
                directory = m_lastExportDir;

            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    directory,
                    m_settings.Source.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            // export
            m_settings.Export(path);
        }

        void OnWizardUpdate()
        {
            isValid = true;
            var helpBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            foreach (var validation in m_settings.CanExport())
            {
                if (!validation.CanExport)
                {
                    isValid = false;
                    errorBuilder.Append(validation.Message);
                }
                else
                {
                    helpBuilder.AppendLine(validation.Message);
                }
            }

            helpString = helpBuilder.ToString();
            errorString = errorBuilder.ToString();
        }
    }

    public static class VRMExporterMenu
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export humanoid";

        [MenuItem(CONVERT_HUMANOID_KEY, true, 1)]
        private static bool ExportValidate()
        {
            var root = Selection.activeObject as GameObject;
            if (root == null)
            {
                return false;
            }

            var animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                return false;
            }

            return true;
        }

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            VRMExporterWizard.CreateWizard();
        }
    }
}
