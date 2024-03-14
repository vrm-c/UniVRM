using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// ヒエラルキーをエクスポートするダイアログ。
    /// 
    /// * Root管理(m_state)
    /// * Validation管理(Exportボタンを押せるか否か)
    /// 
    /// </summary>
    public abstract class ExportDialogBase : EditorWindow
    {
        ExporterDialogState m_state;
        protected ExporterDialogState State => m_state;

        protected virtual void OnEnable()
        {
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;

            m_state = new ExporterDialogState();

            Initialize();

            m_state.ExportRootChanged += (root) =>
            {
                Repaint();
            };
            m_state.ExportRoot = Selection.activeObject as GameObject;
        }

        protected abstract void Initialize();

        void OnDisable()
        {
            Clear();

            m_state.Dispose();

            Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;
        }

        protected abstract void Clear();

        //
        // scroll
        //
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

        //
        // validation
        //
        protected abstract IEnumerable<Validator> ValidatorFactory();

        void OnGUI()
        {
            var modified = false;
            var isValid = BeginGUI();
            modified = DoGUI(isValid);
            EndGUI();

            if (modified)
            {
                State.Invalidate();
            }
        }

        protected abstract bool DoGUI(bool isValid);

        protected virtual void OnLayout()
        {
        }

        bool BeginGUI()
        {
            // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
            // Validation により GUI の表示項目が変わる場合があるので、
            // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
            if (Event.current.type == EventType.Layout)
            {
                OnLayout();
                State.Validate(ValidatorFactory());
            }

            EditorGUIUtility.labelWidth = 150;

            // lang
            LanguageGetter.OnGuiSelectLang();

            EditorGUILayout.LabelField("ExportRoot");
            {
                State.ExportRoot = (GameObject)EditorGUILayout.ObjectField(State.ExportRoot, typeof(GameObject), true);
            }

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);

            // validation
            foreach (var v in State.Validations)
            {
                v.DrawGUI();
                if (v.ErrorLevel == ErrorLevels.Critical)
                {
                    // Export UI を表示しない
                    return false;
                }
            }
            return true;
        }

        protected abstract string SaveTitle { get; }
        protected abstract string SaveName { get; }
        protected abstract string[] SaveExtensions { get; }

        void EndGUI()
        {
            EditorGUILayout.EndScrollView();

            //
            // export button
            //
            // Create and Other Buttons
            {
                // errors
                using (new GUILayout.VerticalScope())
                {
                    // GUILayout.FlexibleSpace();
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            GUI.enabled = State.Validations.All(x => x.CanExport);

                            if (GUILayout.Button("Export", GUILayout.MinWidth(100)))
                            {
                                var path = SaveFileDialog.GetPath(SaveTitle, SaveName, SaveExtensions);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    ExportPath(path);
                                    // close
                                    Close();
                                    GUIUtility.ExitGUI();
                                }
                            }
                            GUI.enabled = true;
                        }
                    }
                }
            }
            GUILayout.Space(8);
        }

        protected abstract void ExportPath(string path);
    }
}
