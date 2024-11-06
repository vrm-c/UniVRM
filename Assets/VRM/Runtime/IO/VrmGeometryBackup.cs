using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace VRM
{
    /// <summary>
    /// SpringBone, LookAt の座標を記録しておき、Dispose で復帰する。
    /// </summary>
    public class VrmGeometryBackup : IDisposable
    {
        struct VrmSpringBoneColliderBackup
        {
            struct Collider
            {
                Vector3 _offsetInWorld;
                float _radiusMultLossyScale;

                public Collider(Transform transform, VRMSpringBoneColliderGroup.SphereCollider collider)
                {
                    _offsetInWorld = transform.TransformPoint(collider.Offset);
                    _radiusMultLossyScale = transform.UniformedLossyScale() * collider.Radius;
                }

                public void Restore(Transform transform, VRMSpringBoneColliderGroup.SphereCollider collider)
                {
                    collider.Offset = transform.worldToLocalMatrix.MultiplyPoint(_offsetInWorld);
                    collider.Radius = _radiusMultLossyScale / transform.UniformedLossyScale();
                }
            }
            Collider[] _colliers;

            public VrmSpringBoneColliderBackup(VRMSpringBoneColliderGroup colliderGroup)
            {
                _colliers = colliderGroup.Colliders.Select(x => new Collider(colliderGroup.transform, x)).ToArray();
            }

            public void Restore(VRMSpringBoneColliderGroup colliderGroup)
            {
                for (int i = 0; i < colliderGroup.Colliders.Length; ++i)
                {
                    _colliers[i].Restore(colliderGroup.transform, colliderGroup.Colliders[i]);
                }
            }
        }

        struct VrmSpringBoneBackup
        {
            Vector3 _gravityDirInWorld;
            float _radiusMultLossyScale;

            public VrmSpringBoneBackup(VRMSpringBone springBone)
            {
                _gravityDirInWorld = springBone.transform.TransformDirection(springBone.m_gravityDir);
                _radiusMultLossyScale = springBone.transform.UniformedLossyScale() * springBone.m_hitRadius;
            }
            public void Restore(VRMSpringBone springBone)
            {
                springBone.m_gravityDir = springBone.transform.worldToLocalMatrix.MultiplyVector(_gravityDirInWorld);
                springBone.m_hitRadius = _radiusMultLossyScale / springBone.transform.UniformedLossyScale();
            }
        }

        Dictionary<VRMSpringBoneColliderGroup, VrmSpringBoneColliderBackup> _springBoneColliders;
        Dictionary<VRMSpringBone, VrmSpringBoneBackup> _springBones;
        VRMFirstPerson _firstPerson;
        Vector3 _firstPersonOffsetInWorld;

        public VrmGeometryBackup(GameObject root)
        {
            _springBoneColliders = root.GetComponentsInChildren<VRMSpringBoneColliderGroup>().ToDictionary(x => x, x => new VrmSpringBoneColliderBackup(x));
            _springBones = root.GetComponentsInChildren<VRMSpringBone>().ToDictionary(x => x, x => new VrmSpringBoneBackup(x));

            _firstPerson = root.GetComponent<VRMFirstPerson>();
            if (_firstPerson != null)
            {
                _firstPersonOffsetInWorld = _firstPerson.transform.TransformPoint(_firstPerson.FirstPersonOffset);
            }
        }

        public void Dispose()
        {
            foreach (var (k, v) in _springBoneColliders)
            {
                v.Restore(k);
            }
            foreach (var (k, v) in _springBones)
            {
                v.Restore(k);
            }

            if (_firstPerson != null)
            {
                _firstPerson.FirstPersonOffset = _firstPerson.transform.worldToLocalMatrix.MultiplyPoint(_firstPersonOffsetInWorld);
            }
        }
    }
}