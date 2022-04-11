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

        [SerializeField]
        public bool m_exclude;

        [SerializeField, Range(0, 0.5f), Header("Collision")]
        public float m_jointRadius = 0.02f;

        void AddJointRecursive(Transform t)
        {
            var joint = t.gameObject.GetComponent<VRM10SpringBoneJoint>();
            if (joint == null)
            {
                joint = t.gameObject.AddComponent<VRM10SpringBoneJoint>();
                Debug.Log($"{joint} added");
            }

            if (t.childCount > 0)
            {
                // only first child
                AddJointRecursive(t.GetChild(0));
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

        [ContextMenu("AddJointsToAllChild0")]
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

            AddJointRecursive(transform.GetChild(0));

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
    }
}
