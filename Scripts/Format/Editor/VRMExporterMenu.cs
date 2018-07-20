using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;
using System.Text;


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
            var target = m_settings.Source;
            if (m_settings.PoseFreeze)
            {
                Undo.RecordObjects(m_settings.Source.transform.Traverse().ToArray(), "before normalize");
                var map = new Dictionary<Transform, Transform>();
                target = VRM.BoneNormalizer.Execute(m_settings.Source.gameObject, map, m_settings.ForceTPose);
                VRMHumanoidNorimalizerMenu.CopyVRMComponents(m_settings.Source.gameObject, target, map);
                Undo.PerformUndo();
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var vrm = VRMExporter.Export(target);
            vrm.extensions.VRM.meta.title = m_settings.Title;
            vrm.extensions.VRM.meta.author = m_settings.Author;

            var bytes = vrm.ToGlbBytes();
            File.WriteAllBytes(path, bytes);

            Debug.LogFormat("Export elapsed {0}", sw.Elapsed);

            if (m_settings.Source.gameObject != target)
            {
                GameObject.DestroyImmediate(target);
            }

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
            }
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
        const string CONVERT_HUMANOID_KEY = VRMVersion.VRM_VERSION + "/export humanoid";

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
