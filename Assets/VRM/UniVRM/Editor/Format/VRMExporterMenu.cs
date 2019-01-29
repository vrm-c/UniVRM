using System.Text;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public class VRMExporterWizard : ScriptableWizard
    {
        const string EXTENSION = ".vrm";

        VRMMeta m_meta;

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

        void OnWizardCreate()
        {
            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    null,
                    m_settings.Source.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // export
            m_settings.Export(path);
        }

        void OnWizardUpdate()
        {
            isValid = true;
            var helpBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            foreach(var msg in m_settings.CanExport())
            {
                isValid = false;
                errorBuilder.Append(msg);
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
