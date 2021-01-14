using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MeshUtility.M17N;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class VRMExporterWizard : EditorWindow
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export " + VRMVersion.VRM_VERSION;

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var window = (VRMExporterWizard)GetWindow(typeof(VRMExporterWizard));
            window.titleContent = new GUIContent("VRM Exporter");
            window.Show();
        }

        enum Tabs
        {
            Meta,
            Mesh,
            BlendShape,
            ExportSettings,
        }
        Tabs _tab;

        MeshUtility.ExporterDialogState m_state;

        VRMExportSettings m_settings;
        VRMExportMeshes m_meshes;

        VRMMetaObject m_meta;
        VRMMetaObject Meta
        {
            get { return m_meta; }
            set
            {
                if (m_meta == value)
                {
                    return;
                }
                if (m_metaEditor != null)
                {
                    UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                    m_metaEditor = null;
                }
                m_meta = value;
            }
        }

        VRMMetaObject m_tmpMeta;

        Editor m_metaEditor;
        Editor m_settingsInspector;
        Editor m_meshesInspector;

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;

            m_tmpMeta = ScriptableObject.CreateInstance<VRMMetaObject>();

            m_settings = ScriptableObject.CreateInstance<VRMExportSettings>();
            m_settingsInspector = Editor.CreateEditor(m_settings);

            m_meshes = ScriptableObject.CreateInstance<VRMExportMeshes>();
            m_meshesInspector = Editor.CreateEditor(m_meshes);

            m_state = new MeshUtility.ExporterDialogState();
            m_state.ExportRootChanged += (root) =>
            {
                // update meta
                if (root == null)
                {
                    Meta = null;
                }
                else
                {
                    var meta = root.GetComponent<VRMMeta>();
                    if (meta != null)
                    {
                        Meta = meta.Meta;
                    }
                    else
                    {
                        Meta = null;
                    }

                    // default setting
                    m_settings.PoseFreeze =
                    HumanoidValidator.HasRotationOrScale(root)
                    || m_meshes.Meshes.Any(x => x.ExportBlendShapeCount > 0 && !x.HasSkinning)
                    ;
                }

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

            // m_metaEditor
            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;
            // m_settingsInspector
            UnityEditor.Editor.DestroyImmediate(m_settingsInspector);
            m_settingsInspector = null;
            // m_meshesInspector
            UnityEditor.Editor.DestroyImmediate(m_meshesInspector);
            m_meshesInspector = null;
            // Meta
            Meta = null;
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            m_tmpMeta = null;
            // m_settings
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
            // m_meshes
            ScriptableObject.DestroyImmediate(m_meshes);
            m_meshes = null;
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
            HumanoidValidator.MeshInformations = m_meshes.Meshes;
            HumanoidValidator.EnableFreeze = m_settings.PoseFreeze;
            VRMExporterValidator.ReduceBlendshape = m_settings.ReduceBlendshape;

            yield return MeshUtility.Validators.HierarchyValidator.Validate;
            if (!m_state.ExportRoot)
            {
                yield break;
            }

            yield return HumanoidValidator.Validate;
            yield return VRMExporterValidator.Validate;
            yield return VRMSpringBoneValidator.Validate;

            var firstPerson = m_state.ExportRoot.GetComponent<VRMFirstPerson>();
            if (firstPerson != null)
            {
                yield return firstPerson.Validate;
            }

            var proxy = m_state.ExportRoot.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null)
            {
                yield return proxy.Validate;
            }

            var meta = Meta ? Meta : m_tmpMeta;
            yield return meta.Validate;
        }

        private void OnGUI()
        {
            // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
            // Validation により GUI の表示項目が変わる場合があるので、
            // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
            if (Event.current.type == EventType.Layout)
            {
                // m_settings, m_meshes.Meshes                
                m_meshes.SetRoot(m_state.ExportRoot, m_settings);
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
                        OnExportClicked(m_state.ExportRoot, Meta != null ? Meta : m_tmpMeta, m_settings, m_meshes);
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
            EditorGUILayout.HelpBox($"Mesh size: {m_meshes.ExpectedExportByteSize / 1000000.0f:0.0} MByte", MessageType.Info);

            //
            // GUI
            //
            _tab = MeshUtility.TabBar.OnGUI(_tab);
            foreach (var meshInfo in m_meshes.Meshes)
            {
                switch (meshInfo.VertexColor)
                {
                    case UniGLTF.MeshExportInfo.VertexColorState.ExistsAndMixed:
                        MeshUtility.Validation.Warning($"{meshInfo.Renderer}: Both vcolor.multiply and not multiply unlit materials exist").DrawGUI();
                        break;
                }
            }
            return DrawWizardGUI();
        }

        bool DrawWizardGUI()
        {
            if (m_tmpMeta == null)
            {
                // disabled
                return false;
            }

            // tabbar
            switch (_tab)
            {
                case Tabs.Meta:
                    if (m_metaEditor == null)
                    {
                        if (m_meta != null)
                        {
                            m_metaEditor = Editor.CreateEditor(Meta);
                        }
                        else
                        {
                            m_metaEditor = Editor.CreateEditor(m_tmpMeta);
                        }
                    }
                    m_metaEditor.OnInspectorGUI();
                    break;

                case Tabs.Mesh:
                    m_meshesInspector.OnInspectorGUI();
                    break;

                case Tabs.BlendShape:
                    if (m_state.ExportRoot)
                    {
                        OnBlendShapeGUI(m_state.ExportRoot.GetComponent<VRMBlendShapeProxy>());
                    }
                    break;

                case Tabs.ExportSettings:
                    m_settings.Root = m_state.ExportRoot;
                    m_settingsInspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        enum BlendShapeTabMessages
        {
            [LangMsg(Languages.ja, "prefab は操作できません")]
            [LangMsg(Languages.en, "cannot manipulate prefab")]
            CANNOT_MANIPULATE_PREFAB,

            [LangMsg(Languages.ja, "シーン上のExportRootにBlendShapeを適用します。Exportすると適用された状態がBakeされます。")]
            [LangMsg(Languages.en, "Apply blendshpae to ExportRoot in scene. Bake scene status if Export.")]
            SCENE_MESSAGE,

            [LangMsg(Languages.ja, "選択された BlendShapeClip を適用する")]
            [LangMsg(Languages.en, "Apply selected BlendShapeClip")]
            APPLY_BLENDSHAPECLIP_BUTTON,

            [LangMsg(Languages.ja, "BlendShape を Clear する")]
            [LangMsg(Languages.en, "Clear BlendShape")]
            CLEAR_BLENDSHAPE_BUTTON,
        }

        BlendShapeMerger m_merger;

        int m_selected = 0;
        void OnBlendShapeGUI(VRMBlendShapeProxy proxy)
        {
            if (!m_state.ExportRoot.scene.IsValid())
            {
                EditorGUILayout.HelpBox(BlendShapeTabMessages.CANNOT_MANIPULATE_PREFAB.Msg(), MessageType.Warning);
                return;
            }

            if (!proxy)
            {
                return;
            }
            var avatar = proxy.BlendShapeAvatar;
            if (!avatar)
            {
                return;
            }

            m_merger = new BlendShapeMerger(avatar.Clips, proxy.transform);


            GUILayout.Space(20);

            EditorGUILayout.HelpBox(BlendShapeTabMessages.SCENE_MESSAGE.Msg(), MessageType.Info);

            var options = avatar.Clips.Select(x => x.ToString()).ToArray();
            m_selected = EditorGUILayout.Popup("select blendshape", m_selected, options);

            if (GUILayout.Button(BlendShapeTabMessages.APPLY_BLENDSHAPECLIP_BUTTON.Msg()))
            {
                m_merger.SetValues(avatar.Clips.Select((x, i) => new KeyValuePair<BlendShapeKey, float>(x.Key, i == m_selected ? 1 : 0)));
                m_merger.Apply();
                m_settings.PoseFreeze = true;
            }

            if (GUILayout.Button(BlendShapeTabMessages.CLEAR_BLENDSHAPE_BUTTON.Msg()))
            {
                m_merger.SetValues(avatar.Clips.Select(x => new KeyValuePair<BlendShapeKey, float>(x.Key, 0)));
                m_merger.Apply();
            }
        }

        const string EXTENSION = ".vrm";
        private static string m_lastExportDir;
        static void OnExportClicked(GameObject root, VRMMetaObject meta, VRMExportSettings settings, VRMExportMeshes meshes)
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
                    root.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            // export
            VRMEditorExporter.Export(path, root, meta, settings, meshes.Meshes);
        }
    }
}
