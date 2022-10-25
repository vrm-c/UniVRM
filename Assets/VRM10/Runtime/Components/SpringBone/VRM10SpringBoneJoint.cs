using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    [DisallowMultipleComponent]
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

        [SerializeField, Range(0, 0.5f), Header("Collision")]
        public float m_jointRadius = 0.02f;

        void AddJointRecursive(Transform t, VRM10SpringBoneJoint src)
        {
            var joint = t.gameObject.GetComponent<VRM10SpringBoneJoint>();
            if (joint == null)
            {
                joint = t.gameObject.AddComponent<VRM10SpringBoneJoint>();
                Debug.Log($"{joint} added");
            }

            // copy settings
            joint.m_stiffnessForce = src.m_stiffnessForce;
            joint.m_gravityPower = src.m_gravityPower;
            joint.m_gravityDir = src.m_gravityDir;
            joint.m_dragForce = src.m_dragForce;
            joint.m_jointRadius = src.m_jointRadius;

            if (t.childCount > 0)
            {
                // only first child
                AddJointRecursive(t.GetChild(0), src);
            }
        }

        void GetJoints(Transform t, List<VRM10SpringBoneJoint> joints)
        {
            var joint = t.GetComponent<VRM10SpringBoneJoint>();
            if (joint != null)
            {
                joints.Add(joint);
            }

            if (t.childCount > 0)
            {
                GetJoints(t.GetChild(0), joints);
            }
        }

        [ContextMenu("Add joints")]
        private void AddJointsToChild0()
        {
            var root = GetComponentInParent<Vrm10Instance>();
            if (root == null)
            {
                Debug.LogWarning("not Vrm10Instance");
                return;
            }

            if (transform.childCount == 0)
            {
                Debug.LogWarning("no children");
                return;
            }

            AddJointRecursive(transform.GetChild(0), this);

            // updater root
            foreach (var spring in root.SpringBone.Springs)
            {
                for (int i = 0; i < spring.Joints.Count; ++i)
                {
                    if (spring.Joints[i] == this)
                    {
                        // found
                        while (spring.Joints.Count - 1 > i)
                        {
                            // remove after this joint
                            spring.Joints.RemoveAt(spring.Joints.Count - 1);
                        }
                        // get descendants joints
                        var joints = new List<VRM10SpringBoneJoint>();
                        GetJoints(transform.GetChild(0), joints);
                        // add jonits to after this
                        spring.Joints.AddRange(joints);
                        return;
                    }
                }
            }

            Debug.LogWarning($"{this} is found in {root}");
        }


        (Vrm10InstanceSpringBone.Spring, int) FindTail(Vrm10Instance vrm, VRM10SpringBoneJoint head)
        {
            foreach (var spring in vrm.SpringBone.Springs)
            {
                var index = spring.Joints.IndexOf(head);
                if (index != -1)
                {
                    if (index + 1 < spring.Joints.Count)
                    {
                        return (spring, index);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return default;
        }

        private void OnDrawGizmosSelected()
        {
            var vrm = GetComponentInParent<Vrm10Instance>();
            if (vrm == null)
            {
                return;
            }

            var (spring, joint_index) = FindTail(vrm, this);
            if (spring == null)
            {
                return;
            }

            if (spring.Joints != null && joint_index + 1 < spring.Joints.Count)
            {
                var tail = spring.Joints[joint_index + 1];
                if (tail != null)
                {
                    // draw tail
                    Gizmos.color = Color.yellow;
                    // tail の radius は head の m_jointRadius で決まる
                    Gizmos.DrawWireSphere(tail.transform.position, m_jointRadius);
                    Gizmos.DrawLine(transform.position, tail.transform.position);
                }
            }
        }
    }
}
