using System;
using System.Collections.Generic;
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

            [SerializeField]
            public Transform Center;

            public Spring(string name)
            {
                Name = name;
            }

            public IEnumerable<(VRM10SpringBoneJoint, Transform)> EnumHeadTail()
            {
                for (int i = 0; i < Joints.Count; ++i)
                {
                    var head = Joints[i];
                    if (head == null)
                    {
                        continue;
                    }
                    for (int j = i + 1; j < Joints.Count; ++j)
                    {
                        var tail = Joints[j];
                        if (tail != null)
                        {
                            yield return (head, tail.transform);
                            break;
                        }
                    }
                }
            }

            /// <summary>
            /// VRM10SpringBoneJoint.OnDrawGizmosSelected から複数の描画 Request が来うる。
            /// Vrm10Instance.OnDrawGizmos 経由で１回だけ描画する。
            /// </summary>
            int m_drawRequest;
            public void RequestDrawGizmos()
            {
                m_drawRequest++;
            }

            public void DrawGizmos()
            {
                if (m_drawRequest == 0)
                {
                    return;
                }
                m_drawRequest = 0;

                VRM10SpringBoneJoint lastJoint = null;
                foreach (var joint in Joints)
                {
                    Gizmos.color = joint == VRM10SpringBoneJoint.s_activeForGizmoDraw ? Color.green : Color.yellow;
                    Gizmos.matrix = joint.transform.localToWorldMatrix;
                    if (lastJoint == null)
                    {
                        const float f = 0.005f;
                        Gizmos.DrawCube(Vector3.zero, new Vector3(f, f, f));
                    }
                    else
                    {
                        Gizmos.DrawLine(Vector3.zero, -joint.transform.localPosition);
                        Gizmos.DrawWireSphere(Vector3.zero, joint.m_jointRadius);
                    }
                    lastJoint = joint;
                }
            }
        }

        [SerializeField]
        public List<Spring> Springs = new List<Spring>();

        public (Spring, int)? FindJoint(VRM10SpringBoneJoint joint)
        {
            for (int i = 0; i < Springs.Count; ++i)
            {
                if (Springs[i].Joints.Contains(joint))
                {
                    return (Springs[i], i);
                }
            }
            return default;
        }
    }
}
