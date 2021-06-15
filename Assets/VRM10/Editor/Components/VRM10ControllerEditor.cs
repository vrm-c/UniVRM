using UniGLTF;
using UnityEditor;
using UnityEngine;

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
            SpringBone,
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
        PropGui m_springBone;
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
            m_springBone = new PropGui(serializedObject.FindProperty(nameof(m_target.SpringBone)));
            m_firstPerson = new PropGui(serializedObject.FindProperty(nameof(m_target.FirstPerson)));
        }

        void OnDisable()
        {
            if (m_metaEditor)
            {
                UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                m_metaEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            // select sub editor
            using (new EnabledScope())
            {
                _tab = (Tabs)EditorGUILayout.EnumPopup("Select GUI", _tab);
            }
            EditorGUILayout.Separator();

            serializedObject.Update();
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

                case Tabs.SpringBone:
                    m_springBone.RecursiveProperty();
                    break;

                case Tabs.FirstPerson:
                    m_firstPerson.RecursiveProperty();
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
