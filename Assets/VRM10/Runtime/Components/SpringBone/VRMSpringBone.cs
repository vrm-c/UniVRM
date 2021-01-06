using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// The base algorithm is http://rocketjump.skr.jp/unity3d/109/ of @ricopin416
    /// DefaultExecutionOrder(11000) means calculate springbone after FinalIK( VRIK )
    /// </summary>
    [AddComponentMenu("VRM/VRMSpringBone")]
#if UNITY_5_5_OR_NEWER
    [DefaultExecutionOrder(11000)]
#endif
    public class VRMSpringBone : MonoBehaviour
    {
        [SerializeField]
        public string m_comment;

        [SerializeField, Header("Gizmo")]
        bool m_drawGizmo = default;

        [SerializeField]
        Color m_gizmoColor = Color.yellow;

        [SerializeField, Range(0, 4), Header("Settings")]
        public float m_stiffnessForce = 1.0f;

        [SerializeField, Range(0, 2)]
        public float m_gravityPower = 0;

        [SerializeField]
        public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField, Range(0, 1)]
        public float m_dragForce = 0.4f;

        [SerializeField]
        public Transform m_center;

        [SerializeField]
        public List<Transform> RootBones = new List<Transform>();

        [SerializeField, Range(0, 0.5f), Header("Collision")]
        public float m_hitRadius = 0.02f;

        [SerializeField]
        public VRMSpringBoneColliderGroup[] ColliderGroups;

        SpringBoneProcessor m_processor = new SpringBoneProcessor();

        [ContextMenu("Reset bones")]
        public void ResetSpringBone()
        {
            m_processor.ResetSpringBone();
        }

        List<SpringBoneLogic.InternalCollider> m_colliderList = new List<SpringBoneLogic.InternalCollider>();
        public void Process()
        {
            if (RootBones == null)
            {
                return;
            }

            // gather colliders
            m_colliderList.Clear();
            if (ColliderGroups != null)
            {
                foreach (var group in ColliderGroups)
                {
                    if (group != null)
                    {
                        foreach (var collider in group.Colliders)
                        {
                            switch (collider.ColliderType)
                            {
                                case SpringBoneColliderTypes.Sphere:
                                    m_colliderList.Add(new SpringBoneLogic.InternalCollider
                                    {
                                        ColliderTypes = SpringBoneColliderTypes.Sphere,
                                        WorldPosition = group.transform.TransformPoint(collider.Offset),
                                        Radius = collider.Radius,

                                    });
                                    break;

                                case SpringBoneColliderTypes.Capsule:
                                    m_colliderList.Add(new SpringBoneLogic.InternalCollider
                                    {
                                        ColliderTypes = SpringBoneColliderTypes.Capsule,
                                        WorldPosition = group.transform.TransformPoint(collider.Offset),
                                        Radius = collider.Radius,
                                        WorldTail = group.transform.TransformPoint(collider.Tail)
                                    });
                                    break;
                            }
                        }
                    }
                }
            }

            var stiffness = m_stiffnessForce * Time.deltaTime;
            var external = m_gravityDir * (m_gravityPower * Time.deltaTime);

            m_processor.Update(RootBones, m_colliderList,
                        stiffness, m_dragForce, external,
                        m_hitRadius, m_center);
        }

        private void OnDrawGizmos()
        {
            if (m_drawGizmo)
            {
                m_processor.DrawGizmos(m_center, m_hitRadius, m_gizmoColor);
            }
        }
    }
}
