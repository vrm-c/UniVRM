using System;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10Object))]
    public class VRM10ObjectEditor : Editor
    {
        VRM10Object m_target;

        enum Tabs
        {
            Meta,
            Expression,
            LookAt,
            FirstPerson,
        }
        static Tabs _tab = Tabs.Meta;

        // for SerializedProperty
        PropGui m_expression;
        PropGui m_meta;
        PropGui m_lookAt;
        PropGui m_firstPerson;
        PropGui m_asset;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10Object)target;

            m_expression = new PropGui(serializedObject.FindProperty(nameof(m_target.Expression)));
            m_meta = new PropGui(serializedObject.FindProperty(nameof(m_target.Meta)));
            m_lookAt = new PropGui(serializedObject.FindProperty(nameof(m_target.LookAt)));
            m_firstPerson = new PropGui(serializedObject.FindProperty(nameof(m_target.FirstPerson)));
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
                    break;

                case Tabs.Expression:
                    m_expression.RecursiveProperty();
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
    }
}
