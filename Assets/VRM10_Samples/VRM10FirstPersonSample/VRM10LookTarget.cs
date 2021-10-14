using UnityEngine;
using UniGLTF;


namespace UniVRM10
{
    public class VRM10LookTarget : MonoBehaviour
    {
        [SerializeField]
        public Transform Target;

        [SerializeField]
        Vector3 m_offset = new Vector3(0, 0.05f, 0);

        [SerializeField, Range(0, 3.0f)]
        float m_distance = 0.7f;

        public VRM10OffsetOnTransform m_offsetTransform;

        void Update()
        {
            if (Target != m_offsetTransform.Transform)
            {
                m_offsetTransform = VRM10OffsetOnTransform.Create(Target);
            }

            var target = m_offsetTransform.Transform;
            if (target != null)
            {
                var targetPosition = target.position + m_offset;
                transform.position = targetPosition + (m_offsetTransform.WorldMatrix.ExtractRotation() * Vector3.forward) * m_distance;
                transform.LookAt(targetPosition);
            }
        }
    }
}
