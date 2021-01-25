using MeshUtility;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    [CustomEditor(typeof(VRMFirstPerson))]
    class VRMFirstPersonEditor : Editor
    {
        VRMFirstPerson m_target;

        void OnEnable()
        {
            m_target = target as VRMFirstPerson;
        }

        void OnDisable()
        {

        }

        /// <summary>
        /// SceneView gizmo
        /// </summary>
        void OnSceneGUI()
        {
            var head = m_target.FirstPersonBone;
            if (head == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var worldOffset = head.localToWorldMatrix.MultiplyPoint(m_target.FirstPersonOffset);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.Label(worldOffset, "FirstPersonOffset");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_target, "Changed FirstPerson");

                m_target.FirstPersonOffset = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
            }
        }

        public static void Separator(int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * 15);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            VRMFirstPersonValidator.Hierarchy = m_target.transform.GetComponentsInChildren<Transform>(true);

            // show vaildation
            bool isValid = true;
            for (int i = 0; i < m_target.Renderers.Count; ++i)
            {
                if (VRMFirstPersonValidator.IsValid(m_target.Renderers[i], $"Renderers[{i}]", out Validation v))
                {
                    continue;
                }
                if (isValid)
                {
                    EditorGUILayout.LabelField("Validation Errors");
                }
                v.DrawGUI();
                isValid = false;
            }

            base.OnInspectorGUI();
        }
    }
}
