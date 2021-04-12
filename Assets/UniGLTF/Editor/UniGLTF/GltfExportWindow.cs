using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UniGLTF.Animation;
using UnityEditor;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class GltfExportWindow : EditorWindow
    {
        const string MENU_KEY = UniGLTFVersion.MENU + "/Export " + UniGLTFVersion.UNIGLTF_VERSION;

        [MenuItem(MENU_KEY, false, 0)]
        private static void ExportFromMenu()
        {
            var window = (GltfExportWindow)GetWindow(typeof(GltfExportWindow));
            window.titleContent = new GUIContent("Gltf Exporter");
            window.Show();
        }

        private static void Export(GameObject go, string path, MeshExportSettings settings, Axises inverseAxis)
        {
            var ext = Path.GetExtension(path).ToLower();
            var isGlb = false;
            switch (ext)
            {
                case ".glb": isGlb = true; break;
                case ".gltf": isGlb = false; break;
                default: throw new System.Exception();
            }

            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf, inverseAxis))
            {
                exporter.Prepare(go);
                exporter.Export(settings, AssetTextureUtil.IsTextureEditorAsset, AssetTextureUtil.GetTextureBytesWithMime);
            }

            if (isGlb)
            {
                var bytes = gltf.ToGlbBytes();
                File.WriteAllBytes(path, bytes);
            }
            else
            {
                var (json, buffers) = gltf.ToGltf(path);
                // without BOM
                var encoding = new System.Text.UTF8Encoding(false);
                File.WriteAllText(path, json, encoding);
                // write to local folder
                var dir = Path.GetDirectoryName(path);
                foreach (var b in buffers)
                {
                    var bufferPath = Path.Combine(dir, b.uri);
                    File.WriteAllBytes(bufferPath, b.GetBytes().ToArray());
                }
            }

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                AssetDatabase.Refresh();
            }
        }

        MeshUtility.ExporterDialogState m_state;
        GltfExportSettings m_settings;
        Editor m_settingsInspector;

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;

            m_settings = ScriptableObject.CreateInstance<GltfExportSettings>();
            m_settingsInspector = Editor.CreateEditor(m_settings);

            m_state = new MeshUtility.ExporterDialogState();
            m_state.ExportRootChanged += (root) =>
            {
                Repaint();
            };
            m_state.ExportRoot = Selection.activeObject as GameObject;
        }

        void OnDisable()
        {
            m_state.Dispose();

            // Debug.Log("OnDisable");
            Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;

            // m_settingsInspector
            UnityEditor.Editor.DestroyImmediate(m_settingsInspector);
            m_settingsInspector = null;
            // m_settings
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
        }

        public delegate Vector2 BeginVerticalScrollViewFunc(Vector2 scrollPosition, bool alwaysShowVertical, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options);
        static BeginVerticalScrollViewFunc s_func;
        static BeginVerticalScrollViewFunc BeginVerticalScrollView
        {
            get
            {
                if (s_func == null)
                {
                    var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "BeginVerticalScrollView").ToArray();
                    var method = methods.First(x => x.GetParameters()[1].ParameterType == typeof(bool));
                    s_func = (BeginVerticalScrollViewFunc)method.CreateDelegate(typeof(BeginVerticalScrollViewFunc));
                }
                return s_func;
            }
        }
        private Vector2 m_ScrollPosition;

        IEnumerable<MeshUtility.Validator> ValidatorFactory()
        {
            yield return MeshUtility.Validators.HierarchyValidator.ValidateRoot;
            yield return AnimationValidator.Validate;
            if (!m_state.ExportRoot)
            {
                yield break;
            }
        }

        private void OnGUI()
        {
            // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
            // Validation により GUI の表示項目が変わる場合があるので、
            // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
            if (Event.current.type == EventType.Layout)
            {
                m_state.Validate(ValidatorFactory());
            }

            EditorGUIUtility.labelWidth = 150;

            // lang
            MeshUtility.M17N.Getter.OnGuiSelectLang();

            EditorGUILayout.LabelField("ExportRoot");
            {
                m_state.ExportRoot = (GameObject)EditorGUILayout.ObjectField(m_state.ExportRoot, typeof(GameObject), true);
            }

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);

            bool modified = ScrollArea();

            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            {
                // errors            
                GUILayout.BeginVertical();
                // GUILayout.FlexibleSpace();

                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = m_state.Validations.All(x => x.CanExport);

                    if (GUILayout.Button("Export", GUILayout.MinWidth(100)))
                    {
                        OnExportClicked(m_state.ExportRoot, m_settings);
                        Close();
                        GUIUtility.ExitGUI();
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(8);

            if (modified)
            {
                m_state.Invalidate();
            }
        }

        bool ScrollArea()
        {
            //
            // Validation
            //
            foreach (var v in m_state.Validations)
            {
                v.DrawGUI();
                if (v.ErrorLevel == MeshUtility.ErrorLevels.Critical)
                {
                    // Export UI を表示しない
                    return false;
                }
            }

            //
            // GUI
            //
            m_settings.Root = m_state.ExportRoot;
            m_settingsInspector.OnInspectorGUI();
            return true;
        }

        private static string m_lastExportDir;
        static void OnExportClicked(GameObject root, GltfExportSettings settings)
        {
            string directory;
            if (string.IsNullOrEmpty(m_lastExportDir))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }
            else
            {
                directory = m_lastExportDir;
            }

            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    directory,
                    root.name + ".glb",
                    "glb,gltf");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            // export
            Export(root, path, new MeshExportSettings
            {
                ExportOnlyBlendShapePosition = settings.DropNormal,
                UseSparseAccessorForMorphTarget = settings.Sparse,
                DivideVertexBuffer = settings.DivideVertexBuffer,
            }, settings.InverseAxis);
        }
    }
}
