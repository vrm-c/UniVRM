using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace UniVRM10
{
    /// <summary>
    /// SpringBone, LookAt の座標を記録しておき、Dispose で復帰する。
    /// </summary>
    public class Vrm10GeometryBackup : IDisposable
    {
        struct Vrm10SpringBoneColliderBackup
        {
            Vector3 _offsetInWorld;
            float _radiusMultLossyScale;

            public Vrm10SpringBoneColliderBackup(VRM10SpringBoneCollider collider)
            {
                _offsetInWorld = collider.transform.TransformPoint(collider.Offset);
                _radiusMultLossyScale = collider.transform.UniformedLossyScale() * collider.Radius;
            }

            public void Restore(VRM10SpringBoneCollider collider)
            {
                collider.Offset = collider.transform.worldToLocalMatrix.MultiplyPoint(_offsetInWorld);
                collider.Radius = _radiusMultLossyScale / collider.transform.UniformedLossyScale();
            }
        }

        struct Vrm10SpringBoneJointBackup
        {
            Vector3 _gravityDirInWorld;
            float _radiusMultLossyScale;

            public Vrm10SpringBoneJointBackup(VRM10SpringBoneJoint joint)
            {
                _gravityDirInWorld = joint.transform.TransformDirection(joint.m_gravityDir);
                _radiusMultLossyScale = joint.transform.UniformedLossyScale() * joint.m_jointRadius;
            }
            public void Restore(VRM10SpringBoneJoint joint)
            {
                joint.m_gravityDir = joint.transform.worldToLocalMatrix.MultiplyVector(_gravityDirInWorld);
                joint.m_jointRadius = _radiusMultLossyScale / joint.transform.UniformedLossyScale();
            }
        }

        Dictionary<VRM10SpringBoneCollider, Vrm10SpringBoneColliderBackup> _springBoneColliders;
        Dictionary<VRM10SpringBoneJoint, Vrm10SpringBoneJointBackup> _springBoneJoints;
        Vrm10Instance _vrm;
        Vector3 _lookAtOffsetInWorld;

        public Vrm10GeometryBackup(GameObject root)
        {
            _springBoneColliders = root.GetComponentsInChildren<VRM10SpringBoneCollider>().ToDictionary(x => x, x => new Vrm10SpringBoneColliderBackup(x));
            _springBoneJoints = root.GetComponentsInChildren<VRM10SpringBoneJoint>().ToDictionary(x => x, x => new Vrm10SpringBoneJointBackup(x));
            _vrm = root.GetComponent<Vrm10Instance>();
            _lookAtOffsetInWorld = _vrm.transform.TransformPoint(_vrm.Vrm.LookAt.OffsetFromHead);
        }

        public void Dispose()
        {
            foreach (var (k, v) in _springBoneColliders)
            {
                v.Restore(k);
            }
            foreach (var (k, v) in _springBoneJoints)
            {
                v.Restore(k);
            }
            _vrm.Vrm.LookAt.OffsetFromHead = _vrm.transform.worldToLocalMatrix.MultiplyPoint(_lookAtOffsetInWorld);
        }
    }
}