using UnityEditor;


namespace VRM
{
    [CustomEditor(typeof(VRMMeta))]
    public class VRMMetaEditor : Editor
    {
        Editor m_Inspector;
        SerializedProperty m_VRMMetaObjectProp;

        private void OnDestroy()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
        }

        private void OnEnable()
        {
            m_VRMMetaObjectProp = serializedObject.FindProperty(nameof(VRMMeta.Meta));
            m_Inspector = Editor.CreateEditor(m_VRMMetaObjectProp.objectReferenceValue);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_VRMMetaObjectProp);
            m_Inspector.OnInspectorGUI();
        }
    }
}
