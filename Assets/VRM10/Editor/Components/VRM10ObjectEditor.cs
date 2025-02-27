using System.Linq;
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
        SerializedPropertyEditor m_expression;
        SerializedPropertyEditor m_meta;
        VRM10ObjectLookAtEditor m_lookAt;
        SerializedPropertyEditor m_firstPerson;
        SerializedProperty m_prefab;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10Object)target;

            m_expression = SerializedPropertyEditor.Create(serializedObject, nameof(m_target.Expression));
            m_meta = VRM10MetaEditor.Create(serializedObject);
            m_lookAt = new(serializedObject);
            m_firstPerson = SerializedPropertyEditor.Create(serializedObject, nameof(m_target.FirstPerson));

            m_prefab = serializedObject.FindProperty("m_prefab");
        }

        public override void OnInspectorGUI()
        {
            // prefab
            if (_tab == Tabs.FirstPerson && m_prefab.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("required !", MessageType.Error);
            }
            serializedObject.Update();
            EditorGUILayout.ObjectField(m_prefab);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();

            // select sub editor
            using (new EnabledScope())
            {
                _tab = (Tabs)EditorGUILayout.EnumPopup("Select GUI", _tab);
            }
            EditorGUILayout.Separator();

            switch (_tab)
            {
                case Tabs.Meta:
                    m_meta.OnInspectorGUI();
                    break;

                case Tabs.Expression:
                    m_expression.OnInspectorGUI();
                    break;

                case Tabs.LookAt:
                    m_lookAt.OnInspectorGUI();
                    break;

                case Tabs.FirstPerson:
                    using (new EditorGUI.DisabledScope(m_target.Prefab == null))
                    {
                        if (GUILayout.Button("set default"))
                        {
                            m_target.FirstPerson.SetDefault(m_target.Prefab.transform);
                        }
                        EditorGUILayout.HelpBox("Clear Renderers and add all renderers (Auto)", MessageType.Info);
                    }
                    m_firstPerson.OnInspectorGUI();
                    break;
            }
        }
    }
}
