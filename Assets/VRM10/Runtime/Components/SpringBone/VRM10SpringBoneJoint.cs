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

        SpringBoneLogic m_logic;

        public void DrawGizmo(Transform center, Color color)
        {
            if (m_logic != null)
            {
                m_logic.DrawGizmo(center, m_jointRadius, color);
            }
            else
            {
#if UNITY_EDITOR                
                // Gizmos.matrix = Transform.localToWorldMatrix;
                Gizmos.color = color;
                Gizmos.DrawSphere(transform.position, m_jointRadius);
#endif
            }
        }

        public void Process(Transform center, float deltaTime, List<SpringBoneLogic.InternalCollider> colliders, VRM10SpringBoneJoint tail)
        {
            if (m_logic == null)
            {
                // 初期化
                if (tail != null)
                {
                    var localPosition = tail.transform.localPosition;
                    var scale = tail.transform.lossyScale;
                    m_logic = new SpringBoneLogic(center, transform,
                        new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                            ));
                }
                else
                {
                    // 親からまっすぐの位置に tail を作成
                    var delta = transform.position - transform.parent.position;
                    var childPosition = transform.position + delta.normalized * 0.07f;
                    m_logic = new SpringBoneLogic(center, transform, transform.worldToLocalMatrix.MultiplyPoint(childPosition));
                }
            }

            m_logic.Update(center, m_stiffnessForce * deltaTime, m_dragForce, m_gravityDir * (m_gravityPower * deltaTime), colliders, m_jointRadius);
        }

    }
}
