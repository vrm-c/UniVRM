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

        public GameObject Target;
        VRMMeta m_meta;

        public string Title;

        public string Author;

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

            var meta = wiz.Target.GetComponent<VRMMeta>();
            if (meta != null && meta.Meta != null)
            {
                wiz.Title = meta.Meta.Title;
                wiz.Author = meta.Meta.Author;
            }
            else
            {
                wiz.Title = wiz.Target.name;
            }

            wiz.OnWizardUpdate();
        }

        string m_dir;
        string Dir
        {
            get
            {
                if (m_dir == null)
                {
                    m_dir = Path.GetFullPath(Application.dataPath); ;
                }
                return m_dir;
            }
            set
            {
                m_dir = value;
            }
        }

        void OnWizardCreate()
        {
            // save dialog
            Debug.LogFormat("OnWizardCreate: {0}", Dir);
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    Dir,
                    Target.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            Dir = Path.GetDirectoryName(path);

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

            VRMExporter.Export(target, path, gltf => {

                gltf.extensions.VRM.meta.title = Title;
                gltf.extensions.VRM.meta.author = Author;

            });

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
            isValid = true;
            var helpBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            helpBuilder.Append("select humanoid root(require Animator with valid humanoid avatar).\n");

            if (string.IsNullOrEmpty(Title))
            {
                isValid = false;
                errorBuilder.Append("require Title\n");
            }

            if (string.IsNullOrEmpty(Author))
            {
                isValid = false;
                errorBuilder.Append("require Author\n");
            }

            helpString = helpBuilder.ToString();
            errorString = errorBuilder.ToString();
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
