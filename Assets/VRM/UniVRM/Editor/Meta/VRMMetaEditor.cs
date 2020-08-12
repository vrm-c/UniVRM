using UnityEditor;


namespace VRM
{
    [CustomEditor(typeof(VRMMeta))]
    public class VRMMetaEditor : Editor
    {
        Editor m_Inspector;
        SerializedProperty m_VRMMetaObjectProp;

        private void OnDisable()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
            m_Inspector = null;
        }

        private void OnEnable()
        {
            m_VRMMetaObjectProp = serializedObject.FindProperty("Meta");
            m_Inspector = Editor.CreateEditor(m_VRMMetaObjectProp.objectReferenceValue);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_VRMMetaObjectProp);
            if (m_Inspector != null)
            {
                m_Inspector.OnInspectorGUI();
            }
        }
    }
}
