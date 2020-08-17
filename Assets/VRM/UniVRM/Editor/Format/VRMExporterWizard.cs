using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using uei = UnityEngine.Internal;

namespace VRM
{
    /// <summary>
    /// エクスポートダイアログ
    /// </summary>
    public class VRMExporterWizard : EditorWindow
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export humanoid";

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            VRMExporterWizard.CreateWizard();
        }

        GameObject ExportRoot;

        VRMExportSettings m_settings;

        VRMMetaObject m_meta;
        VRMMetaObject Meta
        {
            get { return m_meta; }
            set
            {
                if (m_meta == value) return;
                if (m_metaEditor != null)
                {
                    UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                    m_metaEditor = null;
                }
                m_meta = value;
            }
        }

        bool MetaHasError
        {
            get
            {
                if (Meta != null)
                {
                    return Meta.Validate().Any();
                }
                else
                {
                    return m_tmpMeta.Validate().Any();
                }
            }
        }

        void UpdateRoot(GameObject root)
        {
            if (root == ExportRoot)
            {
                return;
            }
            ExportRoot = root;
            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;

            if (ExportRoot == null)
            {
                Meta = null;
            }
            else
            {
                // default setting
                m_settings.PoseFreeze = HasRotationOrScale(ExportRoot);

                var meta = ExportRoot.GetComponent<VRMMeta>();
                if (meta != null)
                {
                    Meta = meta.Meta;
                }
                else
                {
                    Meta = null;
                }
            }
        }

        /// <summary>
        /// ボーン名の重複を確認
        /// </summary>
        /// <returns></returns>
        bool DuplicateBoneNameExists()
        {
            if (ExportRoot == null)
            {
                return false;
            }
            var bones = ExportRoot.transform.GetComponentsInChildren<Transform>();
            var duplicates = bones
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            return (duplicates.Any());
        }

        public static bool IsFileNameLengthTooLong(string fileName)
        {
            return fileName.Length > 64;
        }

        public static bool HasRotationOrScale(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                if (t.localRotation != Quaternion.identity)
                {
                    return true;
                }
                if (t.localScale != Vector3.one)
                {
                    return true;
                }
            }

            return false;
        }

        static Vector3 GetForward(Transform l, Transform r)
        {
            if (l is null || r is null)
            {
                return Vector3.zero;
            }
            var lr = (r.position - l.position).normalized;
            return Vector3.Cross(lr, Vector3.up);
        }

        static bool EnableRenderer(Renderer renderer)
        {
            if (renderer.transform.Ancestor().Any(x => !x.gameObject.activeSelf))
            {
                // 自分か祖先に !activeSelf がいる
                return false;
            }
            if (!renderer.enabled)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// エクスポート可能か検証する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Validation> Validate()
        {
            if (ExportRoot == null)
            {
                yield break;
            }

            if (DuplicateBoneNameExists())
            {
                yield return Validation.Warning(Msg.DUPLICATE_BONE_NAME_EXISTS);
            }

            if (m_settings.ReduceBlendshape && ExportRoot.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return Validation.Error(Msg.NEEDS_VRM_BLENDSHAPE_PROXY);
            }

            var vertexColor = ExportRoot.GetComponentsInChildren<SkinnedMeshRenderer>().Any(x => x.sharedMesh.colors.Length > 0);
            if (vertexColor)
            {
                yield return Validation.Warning(Msg.VERTEX_COLOR_IS_INCLUDED);
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            var materials = renderers.SelectMany(x => x.sharedMaterials).Distinct();
            foreach (var material in materials)
            {
                if (material.shader.name == "Standard")
                {
                    // standard
                    continue;
                }

                if (VRMMaterialExporter.UseUnlit(material.shader.name))
                {
                    // unlit
                    continue;
                }

                if (VRMMaterialExporter.VRMExtensionShaders.Contains(material.shader.name))
                {
                    // VRM supported
                    continue;
                }

                yield return Validation.Warning($"Material: {material.name}. Unknown Shader: \"{material.shader.name}\" is used. {Msg.UNKNOWN_SHADER}");
            }

            foreach (var material in materials)
            {
                if (IsFileNameLengthTooLong(material.name))
                    yield return Validation.Error(Msg.FILENAME_TOO_LONG + material.name);
            }

            var textureNameList = new List<string>();
            foreach (var material in materials)
            {
                var shader = material.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        if ((material.GetTexture(ShaderUtil.GetPropertyName(shader, i)) != null))
                        {
                            var textureName = material.GetTexture(ShaderUtil.GetPropertyName(shader, i)).name;
                            if (!textureNameList.Contains(textureName))
                                textureNameList.Add(textureName);
                        }
                    }
                }
            }

            foreach (var textureName in textureNameList)
            {
                if (IsFileNameLengthTooLong(textureName))
                    yield return Validation.Error(Msg.FILENAME_TOO_LONG + textureName);
            }

            var vrmMeta = ExportRoot.GetComponent<VRMMeta>();
            if (vrmMeta != null && vrmMeta.Meta != null && vrmMeta.Meta.Thumbnail != null)
            {
                var thumbnailName = vrmMeta.Meta.Thumbnail.name;
                if (IsFileNameLengthTooLong(thumbnailName))
                    yield return Validation.Error(Msg.FILENAME_TOO_LONG + thumbnailName);
            }

            var meshFilters = ExportRoot.GetComponentsInChildren<MeshFilter>();
            var meshesName = meshFilters.Select(x => x.sharedMesh.name).Distinct();
            foreach (var meshName in meshesName)
            {
                if (IsFileNameLengthTooLong(meshName))
                    yield return Validation.Error(Msg.FILENAME_TOO_LONG + meshName);
            }

            var skinnedmeshRenderers = ExportRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinnedmeshesName = skinnedmeshRenderers.Select(x => x.sharedMesh.name).Distinct();
            foreach (var skinnedmeshName in skinnedmeshesName)
            {
                if (IsFileNameLengthTooLong(skinnedmeshName))
                    yield return Validation.Error(Msg.FILENAME_TOO_LONG + skinnedmeshName);
            }
        }

        VRMMetaObject m_tmpMeta;

        Editor m_metaEditor;
        Editor m_Inspector;

        private bool m_IsValid = true;

        List<Validation> m_validations = new List<Validation>();

        private Vector2 m_ScrollPosition;
        private string m_CreateButton = "Create";
        private string m_OtherButton = "";

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += OnWizardUpdate;
            Selection.selectionChanged += OnWizardUpdate;

            m_tmpMeta = ScriptableObject.CreateInstance<VRMMetaObject>();

            if (m_settings == null)
            {
                m_settings = ScriptableObject.CreateInstance<VRMExportSettings>();
            }
            if (m_Inspector == null)
            {
                m_Inspector = Editor.CreateEditor(m_settings);
            }

            m_lang = EnumUtil.TryParseOrDefault<VRMExporterWizardMessages.Languages>(EditorPrefs.GetString(LANG_KEY, default(VRMExporterWizardMessages.Languages).ToString()));
        }
        const string LANG_KEY = "VRM_LANG";

        void OnDisable()
        {
            ExportRoot = null;

            // Debug.Log("OnDisable");
            Selection.selectionChanged -= OnWizardUpdate;
            Undo.willFlushUndoRecord -= OnWizardUpdate;

            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
            m_Inspector = null;
            Meta = null;
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            m_tmpMeta = null;
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
        }

        private void InvokeWizardUpdate()
        {
            const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            MethodInfo method = GetType().GetMethod("OnWizardUpdate", kInstanceInvokeFlags);
            if (method != null)
                method.Invoke(this, null);
        }

        private class Styles
        {
            public static string errorText = "Wizard Error";
            public static string box = "Wizard Box";
        }

        public delegate Vector2 BeginVerticalScrollViewFunc(Vector2 scrollPosition, bool alwaysShowVertical, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options);
        static BeginVerticalScrollViewFunc s_func;
        static BeginVerticalScrollViewFunc BeginVerticalScrollView
        {
            get
            {
                if (s_func is null)
                {
                    var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "BeginVerticalScrollView").ToArray();
                    var method = methods.First(x => x.GetParameters()[1].ParameterType == typeof(bool));
                    s_func = (BeginVerticalScrollViewFunc)method.CreateDelegate(typeof(BeginVerticalScrollViewFunc));
                }
                return s_func;
            }
        }

        VRMExporterWizardMessages.Languages m_lang;
        VRMExporterWizardMessages.LangMessages Msg => VRMExporterWizardMessages.M17N[m_lang];

        //@TODO: Force repaint if scripts recompile

        private void OnGUI()
        {
            if (m_tmpMeta == null)
            {
                // OnDisable
                return;
            }

            EditorGUIUtility.labelWidth = 150;

            // lang
            var lang = (VRMExporterWizardMessages.Languages)EditorGUILayout.EnumPopup("lang", m_lang);
            if (lang != m_lang)
            {
                m_lang = lang;
                EditorPrefs.SetString(LANG_KEY, m_lang.ToString());
            }

            EditorGUILayout.LabelField("ExportRoot");
            {
                var root = (GameObject)EditorGUILayout.ObjectField(ExportRoot, typeof(GameObject), true);
                UpdateRoot(root);
            }

            //
            // ここでも validate している。ここで失敗して return した場合は Export UI を表示しない
            //

            //
            // root
            //
            if (ExportRoot == null)
            {
                Validation.Error(Msg.ROOT_EXISTS).DrawGUI();
                return;
            }
            if (ExportRoot.transform.parent != null)
            {
                Validation.Error(Msg.NO_PARENT).DrawGUI();
                return;
            }
            if (ExportRoot.transform.localRotation != Quaternion.identity || ExportRoot.transform.localScale != Vector3.one)
            {
                Validation.Error(Msg.ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED).DrawGUI();
                return;
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            if (renderers.All(x => !EnableRenderer(x)))
            {
                Validation.Error(Msg.NO_ACTIVE_MESH).DrawGUI();
                return;
            }

            if (HasRotationOrScale(ExportRoot))
            {
                if (m_settings.PoseFreeze)
                {
                    EditorGUILayout.HelpBox("Root OK", MessageType.Info);
                }
                else
                {
                    Validation.Warning(Msg.ROTATION_OR_SCALEING_INCLUDED_IN_NODE).DrawGUI();
                }
            }
            else
            {
                if (m_settings.PoseFreeze)
                {
                    Validation.Warning(Msg.IS_POSE_FREEZE_DONE).DrawGUI();
                }
                else
                {
                    EditorGUILayout.HelpBox("Root OK", MessageType.Info);
                }
            }

            //
            // animator
            //
            var animator = ExportRoot.GetComponent<Animator>();
            if (animator == null)
            {
                Validation.Error(Msg.NO_ANIMATOR).DrawGUI();
                return;
            }

            var l = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var r = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            var f = GetForward(l, r);
            if (Vector3.Dot(f, Vector3.forward) < 0.8f)
            {
                Validation.Error(Msg.FACE_Z_POSITIVE_DIRECTION).DrawGUI();
                return;
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                Validation.Error(Msg.NO_AVATAR_IN_ANIMATOR).DrawGUI();
                return;
            }
            if (!avatar.isValid)
            {
                Validation.Error(Msg.AVATAR_IS_NOT_VALID).DrawGUI();
                return;
            }
            if (!avatar.isHuman)
            {
                Validation.Error(Msg.AVATAR_IS_NOT_HUMANOID).DrawGUI();
                return;
            }
            var jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (jaw != null)
            {
                Validation.Warning(Msg.JAW_BONE_IS_INCLUDED).DrawGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Animator OK", MessageType.Info);
            }

            // validation
            foreach (var v in m_validations)
            {
                v.DrawGUI();
            }

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);
            bool modified = DrawWizardGUI();
            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            {
                // errors            
                GUILayout.BeginVertical();
                // GUILayout.FlexibleSpace();

                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = m_IsValid;

                    const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                    if (m_OtherButton != "" && GUILayout.Button(m_OtherButton, GUILayout.MinWidth(100)))
                    {
                        MethodInfo method = GetType().GetMethod("OnWizardOtherButton", kInstanceInvokeFlags);
                        if (method != null)
                        {
                            method.Invoke(this, null);
                            GUIUtility.ExitGUI();
                        }
                        else
                            Debug.LogError("OnWizardOtherButton has not been implemented in script");
                    }

                    if (m_CreateButton != "" && GUILayout.Button(m_CreateButton, GUILayout.MinWidth(100)))
                    {
                        MethodInfo method = GetType().GetMethod("OnWizardCreate", kInstanceInvokeFlags);
                        if (method != null)
                            method.Invoke(this, null);
                        else
                            Debug.LogError("OnWizardCreate has not been implemented in script");
                        Close();
                        GUIUtility.ExitGUI();
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            if (modified)
                InvokeWizardUpdate();

            GUILayout.Space(8);
        }

        enum Tabs
        {
            Meta,
            ExportSettings,
        }
        Tabs _tab;

        GUIStyle TabButtonStyle => "LargeButton";

        // GUI.ToolbarButtonSize.FitToContentsも設定できる
        GUI.ToolbarButtonSize TabButtonSize => GUI.ToolbarButtonSize.Fixed;

        bool DrawWizardGUI()
        {
            if (m_tmpMeta == null)
            {
                // disabled
                return false;
            }

            // tabbar
            _tab = TabBar.OnGUI(_tab, TabButtonStyle, TabButtonSize);
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

                case Tabs.ExportSettings:
                    m_Inspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        // Creates a wizard.
        public static T DisplayWizard<T>(string title) where T : VRMExporterWizard
        {
            return DisplayWizard<T>(title, "Create", "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName) where T : VRMExporterWizard
        {
            return DisplayWizard<T>(title, createButtonName, "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName, string otherButtonName) where T : VRMExporterWizard
        {
            return (T)DisplayWizard(title, typeof(T), createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static VRMExporterWizard DisplayWizard(string title, Type klass, string createButtonName)
        {
            string otherButtonName = "";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static VRMExporterWizard DisplayWizard(string title, Type klass)
        {
            string otherButtonName = "";
            string createButtonName = "Create";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        // Creates a wizard.
        public static VRMExporterWizard DisplayWizard(string title, Type klass, [uei.DefaultValueAttribute("\"Create\"")] string createButtonName, [uei.DefaultValueAttribute("\"\"")] string otherButtonName)
        {
            VRMExporterWizard wizard = CreateInstance(klass) as VRMExporterWizard;
            wizard.m_CreateButton = createButtonName;
            wizard.m_OtherButton = otherButtonName;
            wizard.titleContent = new GUIContent(title);
            if (wizard != null)
            {
                wizard.InvokeWizardUpdate();
                wizard.ShowUtility();
            }
            return wizard;
        }

        // Allows you to set the create button text of the wizard.
        public string createButtonName
        {
            get { return m_CreateButton; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_CreateButton != newString)
                {
                    m_CreateButton = newString;
                    Repaint();
                }
            }
        }

        // Allows you to set the other button text of the wizard.
        public string otherButtonName
        {
            get { return m_OtherButton; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_OtherButton != newString)
                {
                    m_OtherButton = newString;
                    Repaint();
                }
            }
        }

        // Allows you to enable and disable the wizard create button, so that the user can not click it.
        public bool isValid
        {
            get { return m_IsValid; }
            set { m_IsValid = value; }
        }

        const string EXTENSION = ".vrm";

        private static string m_lastExportDir;

        public static void CreateWizard()
        {
            var wiz = VRMExporterWizard.DisplayWizard<VRMExporterWizard>(
                "VRM Exporter", "Export");
            var go = Selection.activeObject as GameObject;

            // update checkbox
            wiz.UpdateRoot(go);

            if (go != null)
            {
                wiz.m_settings.PoseFreeze = HasRotationOrScale(go);
            }

            wiz.OnWizardUpdate();
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
                    ExportRoot.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            // export
            if (Meta == null)
            {
                var metaB = ExportRoot.GetComponent<VRMMeta>();
                if (metaB == null)
                {
                    metaB = ExportRoot.AddComponent<VRMMeta>();
                }
                metaB.Meta = m_tmpMeta;
            }
            VRMEditorExporter.Export(path, ExportRoot, m_settings);
            if (Meta == null)
            {
                UnityEngine.GameObject.DestroyImmediate(ExportRoot.GetComponent<VRMMeta>());
            }
        }

        void OnWizardUpdate()
        {
            UpdateRoot(ExportRoot);

            m_validations.Clear();
            m_validations.AddRange(Validate());
            m_validations.AddRange(VRMSpringBoneValidator.Validate(ExportRoot));
            var hasError = m_validations.Any(x => !x.CanExport);
            m_IsValid = !hasError && !MetaHasError;

            Repaint();
        }
    }
}

