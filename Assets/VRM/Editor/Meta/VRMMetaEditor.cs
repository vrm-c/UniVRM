using UnityEditor;


namespace VRM
{
    [CustomEditor(typeof(VRMMeta))]
    public class VRMMetaEditor : Editor
    {
        VRMMeta m_target;
        Editor m_Inspector;
        SerializedProperty m_VRMMetaObjectProp;

        void DestroyEditor()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
        }

        private void OnDestroy()
        {
            DestroyEditor();
        }

        private void OnEnable()
        {
            m_target = target as VRMMeta;
            m_VRMMetaObjectProp = serializedObject.FindProperty(nameof(VRMMeta.Meta));
            if (m_target.Meta != null)
            {
                m_Inspector = Editor.CreateEditor(m_VRMMetaObjectProp.objectReferenceValue);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var old = m_VRMMetaObjectProp.objectReferenceValue;
            EditorGUILayout.PropertyField(m_VRMMetaObjectProp);
            if (m_VRMMetaObjectProp.objectReferenceValue != old)
            {
                // updated
                serializedObject.ApplyModifiedProperties();
                DestroyEditor();
                m_Inspector = Editor.CreateEditor(m_VRMMetaObjectProp.objectReferenceValue);
            }

            if (m_Inspector != null)
            {
                m_Inspector.OnInspectorGUI();
            }
        }
    }
}
