using System.Linq;
using UnityEditor;
using UnityEngine;


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

            GUILayout.Space(16);
            var current = m_target.GetComponentsInChildren<VRMSpringBone>().Any(x => x.UseRuntimeScalingSupport);
            var newValue = GUILayout.Toggle(current, "[experimental] SpringBone scaling params");
            if (current != newValue)
            {
                foreach (var sb in m_target.GetComponentsInChildren<VRMSpringBone>())
                {
                    sb.UseRuntimeScalingSupport = newValue;
                }
            }
        }
    }
}
