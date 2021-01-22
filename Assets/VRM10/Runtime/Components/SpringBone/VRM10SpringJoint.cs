using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10SpringJoint
    {
        [SerializeField]
        public Transform Transform;

        public VRM10SpringJoint(Transform t)
        {
            Transform = t;
        }

        [SerializeField, Range(0, 4), Header("Settings")]
        public float m_stiffnessForce = 1.0f;

        [SerializeField, Range(0, 2)]
        public float m_gravityPower = 0;

        [SerializeField]
        public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField, Range(0, 1)]
        public float m_dragForce = 0.4f;

        [SerializeField, Range(0, 0.5f), Header("Collision")]
        public float m_hitRadius = 0.02f;

        SpringBoneLogic m_logic;

        public void DrawGizmo(Transform center, Color color)
        {
            if (m_logic != null)
            {
                m_logic.DrawGizmo(center, m_hitRadius, color);
            }
            else
            {
                // TODO
            }
        }

        public void Update(Transform center, float deltaTime, List<SpringBoneLogic.InternalCollider> colliders, VRM10SpringJoint tail)
        {
            if (m_logic == null)
            {
                // Setup(joints, m_center);
                // SetupRecursive(joint, m_center);
                if (tail == null)
                {
                    // 末端に追加のスプリングを付加する
                    var delta = Transform.position - Transform.parent.position;
                    var childPosition = Transform.position + delta.normalized * 0.07f;
                    m_logic = new SpringBoneLogic(center, Transform, Transform.worldToLocalMatrix.MultiplyPoint(childPosition));
                }
                else
                {
                    // 最初の子ボーンを尻尾としてスプリングを付加する
                    var localPosition = tail.Transform.localPosition;
                    var scale = tail.Transform.lossyScale;
                    m_logic = new SpringBoneLogic(center, Transform,
                        new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                            ));
                }
            }

            m_logic.Update(center, m_stiffnessForce * deltaTime, m_dragForce, m_gravityDir * (m_gravityPower * deltaTime), colliders, m_hitRadius);
        }
    }
}
