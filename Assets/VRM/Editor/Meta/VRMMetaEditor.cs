using UnityEditor;


namespace VRM
{
    [CustomEditor(typeof(VRMMeta))]
    public class VRMMetaEditor : Editor
    {
        VRMMeta m_target;
        Editor m_Inspector;
        SerializedProperty m_VRMMetaObjectProp;

        private void OnDestroy()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
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
            EditorGUILayout.PropertyField(m_VRMMetaObjectProp);
            if (m_Inspector != null)
            {
                m_Inspector.OnInspectorGUI();
            }
        }
    }
}
