using UniGLTF;
using UnityEditor;

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
        SerializedPropertyEditor m_lookAt;
        SerializedPropertyEditor m_firstPerson;
        SerializedPropertyEditor m_asset;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10Object)target;

            m_expression = SerializedPropertyEditor.Create(serializedObject, nameof(m_target.Expression));
            m_meta = VRM10MetaEditor.Create(serializedObject);
            m_lookAt = SerializedPropertyEditor.Create(serializedObject, nameof(m_target.LookAt));
            m_firstPerson = SerializedPropertyEditor.Create(serializedObject, nameof(m_target.FirstPerson));
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
                    m_meta.OnGUI();
                    break;

                case Tabs.Expression:
                    m_expression.OnGUI();
                    break;

                case Tabs.LookAt:
                    m_lookAt.OnGUI();
                    break;

                case Tabs.FirstPerson:
                    m_firstPerson.OnGUI();
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
