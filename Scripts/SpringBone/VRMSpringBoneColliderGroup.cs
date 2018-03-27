using System;
using UnityEngine;


namespace VRM
{
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
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var y in Colliders)
            {
                Gizmos.DrawWireSphere(y.Offset, y.Radius);
            }
        }
    }
}
