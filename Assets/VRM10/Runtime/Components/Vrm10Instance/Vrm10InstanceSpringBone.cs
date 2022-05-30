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
        }

        [SerializeField]
        public List<Spring> Springs = new List<Spring>();
    }
}
