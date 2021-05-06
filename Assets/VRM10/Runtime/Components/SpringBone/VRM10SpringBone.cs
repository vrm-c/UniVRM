using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// The base algorithm is http://rocketjump.skr.jp/unity3d/109/ of @ricopin416
    /// DefaultExecutionOrder(11000) means calculate springbone after FinalIK( VRIK )
    /// 
    /// Setup の Editor の都合から、VRM10SpringBone は MonoBehaviour にする。
    /// Data置き場。Updateは使わない。
    /// 
    /// Springの根元のTranform (Joints.First()) に AddComponent して使う。
    /// 
    /// </summary>
    [DisallowMultipleComponent]
    [Serializable]
    public class VRM10SpringBone : MonoBehaviour
    {
        [SerializeField]
        Color m_gizmoColor = Color.yellow;

        [SerializeField]
        public string Comment;

        [SerializeField]
        public List<VRM10SpringJoint> Joints = new List<VRM10SpringJoint>();

        [SerializeField]
        public List<VRM10SpringBoneColliderGroup> ColliderGroups = new List<VRM10SpringBoneColliderGroup>();

        [ContextMenu("Reset bones")]
        public void ResetJoints()
        {
            foreach (var joint in Joints)
            {
                if (joint != null)
                {
                    joint.Transform.localRotation = Quaternion.identity;
                }
            }
        }

        Transform m_center;

        List<SpringBoneLogic.InternalCollider> m_colliderList = new List<SpringBoneLogic.InternalCollider>();
        public void Process(Transform center)
        {
            m_center = center;
            if (Joints == null)
            {
                return;
            }

            // gather colliders
            m_colliderList.Clear();
            if (ColliderGroups != null)
            {
                foreach (var group in ColliderGroups.Where(x => x != null))
                {
                    foreach (var collider in group.Colliders)
                    {
                        switch (collider.ColliderType)
                        {
                            case VRM10SpringBoneColliderTypes.Sphere:
                                m_colliderList.Add(new SpringBoneLogic.InternalCollider
                                {
                                    ColliderTypes = VRM10SpringBoneColliderTypes.Sphere,
                                    WorldPosition = group.transform.TransformPoint(collider.Offset),
                                    Radius = collider.Radius,

                                });
                                break;

                            case VRM10SpringBoneColliderTypes.Capsule:
                                m_colliderList.Add(new SpringBoneLogic.InternalCollider
                                {
                                    ColliderTypes = VRM10SpringBoneColliderTypes.Capsule,
                                    WorldPosition = group.transform.TransformPoint(collider.Offset),
                                    Radius = collider.Radius,
                                    WorldTail = group.transform.TransformPoint(collider.Tail)
                                });
                                break;
                        }
                    }
                }
            }

            {
                // udpate joints
                VRM10SpringJoint lastJoint = Joints.FirstOrDefault(x => x != null);
                foreach (var joint in Joints.Where(x => x != null).Skip(1))
                {
                    lastJoint.Update(center, Time.deltaTime, m_colliderList, joint);
                    lastJoint = joint;
                }
                lastJoint.Update(center, Time.deltaTime, m_colliderList, null);
            }
        }

        public void OnDrawGizmosSelected()
        {
            foreach (var joint in Joints)
            {
                if (joint != null)
                {
                    joint.DrawGizmo(m_center, m_gizmoColor);
                }
            }
        }
    }
}
