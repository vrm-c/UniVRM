using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    public class VRM10SpringBoneJoint : MonoBehaviour
    {
        [SerializeField, Range(0, 4), Header("Settings")]
        public float m_stiffnessForce = 1.0f;

        [SerializeField, Range(0, 2)]
        public float m_gravityPower = 0;

        [SerializeField]
        public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField, Range(0, 1)]
        public float m_dragForce = 0.4f;

        [SerializeField]
        public bool m_exclude;

        [SerializeField, Range(0, 0.5f), Header("Collision")]
        public float m_jointRadius = 0.02f;

        public void DrawGizmo(Transform center, Color color)
        {
#if UNITY_EDITOR                
            // Gizmos.matrix = Transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, m_jointRadius);
#endif
        }
    }
}
