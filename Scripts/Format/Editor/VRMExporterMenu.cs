using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;

namespace VRM
{
    public class VRMExporterWizard : ScriptableWizard
    {
        const string EXTENSION = ".vrm";

        public GameObject Target;

        public bool ForceTPose;

        public bool PoseFreeze;

        public static void CreateWizard()
        {
            var wiz = ScriptableWizard.DisplayWizard<VRMExporterWizard>(
                "VRM Exporter", "Export");
            wiz.Target = Selection.activeObject as GameObject;

            // update checkbox
            var desc = wiz.Target.GetComponent<VRMHumanoidDescription>();
            if (desc == null)
            {
                wiz.ForceTPose = true;
                wiz.PoseFreeze = true;
            }
        }

        static string m_dir = Path.GetFullPath(Application.dataPath);

        void OnWizardCreate()
        {
            // save dialog
            Debug.LogFormat("OnWizardCreate: {0}", m_dir);
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    m_dir,
                    Target.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_dir = Path.GetDirectoryName(path);

            // export
            var target = Target;
            if (PoseFreeze)
            {
                Undo.RecordObjects(Target.transform.Traverse().ToArray(), "before normalize");
                var map = new Dictionary<Transform, Transform>();
                target = VRM.BoneNormalizer.Execute(Target, map, ForceTPose);
                VRMHumanoidNorimalizerMenu.CopyVRMComponents(Target, target, map);
                Undo.PerformUndo();
            }

            VRMExporter.Export(target, path);

            if (Target!=target)
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
            helpString = @"select humanoid root(require Animator with valid humanoid avatar).
";
        }
    }

    public static class VRMExporterMenu
    {
        const string CONVERT_HUMANOID_KEY = "VRM/export humanoid";

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

            var avatar = animator.avatar;
            if (avatar == null)
            {
                return false;
            }

            if (!avatar.isValid)
            {
                return false;
            }

            if (!avatar.isHuman)
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
