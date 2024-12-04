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

            static Color JointColor(VRM10SpringBoneJoint joint)
            {
#if UNITY_EDITOR
                if (joint != null && UnityEditor.Selection.activeGameObject == joint.gameObject)
                {
                    return Color.green;
                }
#endif
                return Color.yellow;
            }

            public void DrawGizmos()
            {
                if (Joints.Count > 0)
                {
                    var backup = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.identity;
                    VRM10SpringBoneJoint lastJoint = Joints[0];
                    for (int i = 1; i < Joints.Count; ++i)
                    {
                        var joint = Joints[i];
                        Gizmos.color = JointColor(lastJoint);
                        if (joint != null && lastJoint != null)
                        {
                            Gizmos.DrawLine(lastJoint.transform.position, joint.transform.position);
                        }
                        lastJoint = joint;
                    }
                    Gizmos.matrix = backup;
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
