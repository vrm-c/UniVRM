using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// SpringBone の情報をすべて保持する
    /// 
    /// * SpringBoneCollider
    /// * SpringBoneJoint
    /// 
    /// は、個別の MonoBehaviour として設定する
    /// 
    /// </summary>
    [Serializable]
    public sealed class VRM10ControllerSpringBone
    {
        [Serializable]
        public class ColliderGroup
        {
            [SerializeField]
            public string Name;
            [SerializeField]
            public List<VRM10SpringBoneCollider> Colliders = new List<VRM10SpringBoneCollider>();
        }
        [SerializeField]
        public List<ColliderGroup> ColliderGroups = new List<ColliderGroup>();

        [Serializable]
        public class Spring
        {
            [SerializeField]
            public string Name;
            [SerializeField]
            public List<ColliderGroup> ColliderGroups = new List<ColliderGroup>();

            [SerializeField]
            public List<VRM10SpringJoint> Joints = new List<VRM10SpringJoint>();

            Transform m_center;
            List<SpringBoneLogic.InternalCollider> m_colliderList = new List<SpringBoneLogic.InternalCollider>();

            public Spring(string name)
            {
                Name = name;
            }

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
                                        WorldPosition = collider.transform.TransformPoint(collider.Offset),
                                        Radius = collider.Radius,

                                    });
                                    break;

                                case VRM10SpringBoneColliderTypes.Capsule:
                                    m_colliderList.Add(new SpringBoneLogic.InternalCollider
                                    {
                                        ColliderTypes = VRM10SpringBoneColliderTypes.Capsule,
                                        WorldPosition = collider.transform.TransformPoint(collider.Offset),
                                        Radius = collider.Radius,
                                        WorldTail = collider.transform.TransformPoint(collider.Tail)
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
        }
        [SerializeField]
        public List<Spring> Springs = new List<Spring>();

        public void Process(Transform center)
        {
            foreach (var spring in Springs)
            {
                spring.Process(center);
            }
        }
    }
}
