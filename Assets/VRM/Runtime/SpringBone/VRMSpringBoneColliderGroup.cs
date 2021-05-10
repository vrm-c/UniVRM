using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
#if UNITY_5_5_OR_NEWER
    [DefaultExecutionOrder(11001)]
#endif
    public class VRMSpringBoneColliderGroup : MonoBehaviour
    {
        [Serializable]
        public class SphereCollider
        {
            public Vector3 Offset;

            [Range(0, 1.0f)]
            public float Radius;
        }

        [SerializeField]
        public SphereCollider[] Colliders = new SphereCollider[]{
            new SphereCollider
            {
                Radius=0.1f
            }
        };

        [SerializeField]
        Color m_gizmoColor = Color.magenta;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = m_gizmoColor;
            Matrix4x4 mat = transform.localToWorldMatrix;
            var ls = transform.UniformedLossyScale();
            Gizmos.matrix = mat * Matrix4x4.Scale(new Vector3(
                1.0f / transform.lossyScale.x * ls,
                1.0f / transform.lossyScale.y * ls,
                1.0f / transform.lossyScale.z * ls
                ));
            foreach (var y in Colliders)
            {
                Gizmos.DrawWireSphere(y.Offset, y.Radius);
            }
        }

        public IEnumerable<Validation> Validate()
        {
            if (transform.localScale != Vector3.one)
            {
                yield return Validation.Warning($"'{name}' GameObject has none 1 scaling");
            }
            else if (transform.lossyScale != Vector3.one)
            {
                yield return Validation.Warning($"'{name}' parent GameObject has none 1 scaling");
            }
        }
    }
}
