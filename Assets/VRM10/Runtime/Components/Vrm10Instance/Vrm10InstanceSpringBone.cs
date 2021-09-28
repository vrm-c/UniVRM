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
    public sealed class Vrm10InstanceSpringBone
    {
        [SerializeField]
        public List<VRM10SpringBoneColliderGroup> ColliderGroups = new List<VRM10SpringBoneColliderGroup>();

        [Serializable]
        public class Spring
        {
            [SerializeField]
            public string Name;

            public string GUIName(int i) => $"{i:00}:{Name}";

            [SerializeField]
            public List<VRM10SpringBoneColliderGroup> ColliderGroups = new List<VRM10SpringBoneColliderGroup>();

            [SerializeField]
            public List<VRM10SpringBoneJoint> Joints = new List<VRM10SpringBoneJoint>();

            Transform m_center;
            List<SpringBoneLogic.InternalCollider> m_colliderList;
            List<(VRM10SpringBoneJoint, SpringBoneLogic)> m_logics;

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
                if (m_colliderList == null)
                {
                    m_colliderList = new List<SpringBoneLogic.InternalCollider>();
                }
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

                if (m_logics == null)
                {
                    m_logics = Enumerable.Zip(Joints, Joints.Skip(1), (head, tail) =>
                    {
                        var localPosition = tail.transform.localPosition;
                        var scale = tail.transform.lossyScale;
                        var logic = new SpringBoneLogic(center, head.transform,
                            new Vector3(
                                localPosition.x * scale.x,
                                localPosition.y * scale.y,
                                localPosition.z * scale.z
                                ));
                        return (head, logic);
                    }).ToList();
                }
                foreach (var (head, logic) in m_logics)
                {
                    logic.Update(center,
                        head.m_stiffnessForce * Time.deltaTime,
                        head.m_dragForce,
                        head.m_gravityDir * (head.m_gravityPower * Time.deltaTime),
                        m_colliderList, head.m_jointRadius);
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
