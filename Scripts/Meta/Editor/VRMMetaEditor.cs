using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMMeta))]
    public class VRMMetaEditor : Editor
    {
        SerializedProperty m_ScriptProp;
        SerializedProperty m_VRMMetaObjectProp;

        private void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");
            m_VRMMetaObjectProp = serializedObject.FindProperty("Meta");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ScriptProp, true);
            EditorGUILayout.PropertyField(m_VRMMetaObjectProp, true);

            EditorGUILayout.Space();

            if (m_VRMMetaObjectProp.objectReferenceValue != null)
            {
                VRMMetaObjectGUI(new SerializedObject(m_VRMMetaObjectProp.objectReferenceValue));
            }

            serializedObject.ApplyModifiedProperties();
        }

        void VRMMetaObjectGUI(SerializedObject so)
        {
            so.Update();

            int i = 0;
            for (SerializedProperty iterator = so.GetIterator();
                iterator.NextVisible(true);
                ++i
                )
            {
                if (i == 0) continue;

                /*
                Debug.LogFormat("{0}: {1}({2}) = {3}",
                    iterator.depth,
                    iterator.name,
                    iterator.displayName,
                    iterator.propertyType
                    );
                    */
                EditorGUILayout.PropertyField(iterator, false);
            }

            so.ApplyModifiedProperties();
        }
    }
}
