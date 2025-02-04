using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;


namespace UniVRM10
{
    [Serializable]
    [DisallowMultipleComponent]
    public class VRM10SpringBoneJoint : MonoBehaviour
    {
        [SerializeField]
        public float m_stiffnessForce = 1.0f;

        [SerializeField]
        public float m_gravityPower = 0;

        [SerializeField]
        public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField, Range(0, 1)]
        public float m_dragForce = 0.4f;

        [SerializeField]
        public float m_jointRadius = 0.02f;

        public BlittableJointMutable Blittable => new BlittableJointMutable
        {
            stiffnessForce = m_stiffnessForce,
            dragForce = m_dragForce,
            gravityDir = m_gravityDir,
            gravityPower = m_gravityPower,
            radius = m_jointRadius,
        };

        void AddJointRecursive(Transform t, VRM10SpringBoneJoint src)
        {
            var joint = t.gameObject.GetOrAddComponent<VRM10SpringBoneJoint>();

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
            if (t.TryGetComponent<VRM10SpringBoneJoint>(out var joint))
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
                UniGLTFLogger.Warning("not Vrm10Instance");
                return;
            }

            if (transform.childCount == 0)
            {
                UniGLTFLogger.Warning("no children");
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

            UniGLTFLogger.Warning($"{this} is found in {root}");
        }

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 m = default;
            m.SetTRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = m;

            var vrm = GetComponentInParent<Vrm10Instance>();
            if (vrm != null && vrm.TryGetRadiusAsTail(this, out var radius))
            {
                if (radius.HasValue)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(Vector3.zero, radius.Value);
                }
                else
                {
                    // root
                    Gizmos.color = new Color(1, 0.75f, 0f);
                    Gizmos.DrawSphere(Vector3.zero, 0.01f);
                }
            }
            else
            {
                // spring を構成しない
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Vector3.zero, m_jointRadius);
            }
        }
    }
}
