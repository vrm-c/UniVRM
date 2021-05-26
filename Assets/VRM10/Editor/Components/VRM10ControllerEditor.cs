using UnityEditor;
using UnityEngine;
using System;

namespace UniVRM10
{
    /// <summary>
    /// VRM10Controller CustomEditor
    /// 
    /// * Controller
    /// * Meta
    /// * Expression
    /// * LookAt
    /// * ForstPerson
    /// 
    /// </summary>
    [CustomEditor(typeof(VRM10Controller))]
    public class VRM10ControllerEditor : Editor
    {
        VRM10Controller m_target;

        enum Tabs
        {
            Controller,
            Meta,
            Expression,
            LookAt,
            FirstPerson,
        }
        Tabs _tab = Tabs.Meta;

        // for ScriptedObject
        Editor m_metaEditor;

        // expression
        VRM10ControllerEditorExpression m_expression;

        // for SerializedProperty
        PropGui m_controller;
        PropGui m_meta;
        PropGui m_lookAt;
        PropGui m_firstPerson;
        PropGui m_asset;

        void OnEnable()
        {
            m_target = (VRM10Controller)target;
            m_expression = new VRM10ControllerEditorExpression(m_target);
            if (m_target?.Meta.Meta != null)
            {
                m_metaEditor = Editor.CreateEditor(m_target.Meta.Meta);
            }
            m_controller = new PropGui(serializedObject.FindProperty(nameof(m_target.Controller)));
            m_meta = new PropGui(serializedObject.FindProperty(nameof(m_target.Meta)));
            m_lookAt = new PropGui(serializedObject.FindProperty(nameof(m_target.LookAt)));
            m_firstPerson = new PropGui(serializedObject.FindProperty(nameof(m_target.FirstPerson)));
        }

        void OnDisable()
        {
            if (m_metaEditor)
            {
                UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                m_metaEditor = null;
            }

            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledGroupScope(false))
            {
                _tab = (Tabs)EditorGUILayout.EnumPopup("Select GUI", _tab);
                EditorGUILayout.Separator();
            }

            serializedObject.Update();

            // Setup runtime function.
            m_target.Setup();

            // base.OnInspectorGUI();
            switch (_tab)
            {
                case Tabs.Meta:
                    m_meta.RecursiveProperty();
                    m_metaEditor?.OnInspectorGUI();
                    break;

                case Tabs.Controller:
                    m_controller.RecursiveProperty();
                    break;

                case Tabs.Expression:
                    m_expression.OnGUI();
                    break;

                case Tabs.LookAt:
                    m_lookAt.RecursiveProperty();
                    break;

                case Tabs.FirstPerson:
                    m_firstPerson.RecursiveProperty();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        enum VRMSceneUI
        {
            None,
            LookAt,
            SpringBone,
        }
        static VRMSceneUI s_ui = default;
        static string[] s_selection;
        static string[] Selection
        {
            get
            {
                if (s_selection == null)
                {
                    s_selection = Enum.GetNames(typeof(VRMSceneUI));
                }
                return s_selection;
            }
        }

        static VRMSceneUI SelectUI(VRMSceneUI ui)
        {
            var size = SceneView.currentDrawingSceneView.position.size;

            var rect = new Rect(0, 0, size.x, EditorGUIUtility.singleLineHeight);
            return (VRMSceneUI)GUI.SelectionGrid(rect, (int)ui, Selection, 3);
        }

        void OnSceneGUI()
        {
            Handles.BeginGUI();
            s_ui = SelectUI(s_ui);
            Handles.EndGUI();

            switch (s_ui)
            {
                case VRMSceneUI.None:
                    Tools.hidden = false;
                    break;

                case VRMSceneUI.LookAt:
                    Tools.hidden = true;
                    OnSceneGUIOffset();
                    if (!Application.isPlaying)
                    {
                        // offset
                        var p = m_target.LookAt.OffsetFromHead;
                        Handles.Label(m_target.Head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
                    }
                    else
                    {
                        m_target.LookAt.OnSceneGUILookAt(m_target.Head);
                    }
                    break;

                case VRMSceneUI.SpringBone:
                    Tools.hidden = true;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        void OnSceneGUIOffset()
        {
            var component = target as VRM10Controller;
            if (!component.LookAt.DrawGizmo)
            {
                return;
            }

            var head = component.Head;
            if (head == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var worldOffset = head.localToWorldMatrix.MultiplyPoint(component.LookAt.OffsetFromHead);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.DrawDottedLine(head.position, worldOffset, 5);
            Handles.SphereHandleCap(0, head.position, Quaternion.identity, 0.02f, Event.current.type);
            Handles.SphereHandleCap(0, worldOffset, Quaternion.identity, 0.02f, Event.current.type);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(component, "Changed FirstPerson");

                component.LookAt.OffsetFromHead = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
            }
        }
    }
}
