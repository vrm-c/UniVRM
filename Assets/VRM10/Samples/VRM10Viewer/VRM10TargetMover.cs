using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10TargetMover : MonoBehaviour
    {
        [SerializeField]
        float m_radius = 5.0f;

        [SerializeField]
        float m_angularVelocity = 40.0f;

        [SerializeField]
        float m_y = 1.5f;

        [SerializeField]
        float m_height = 3.0f;

        public IEnumerator Start()
        {
            var angle = 0.0f;

            while (true)
            {
                angle += m_angularVelocity * Time.deltaTime * Mathf.Deg2Rad;

                var x = Mathf.Cos(angle) * m_radius;
                var z = Mathf.Sin(angle) * m_radius;
                var y = m_y + m_height * Mathf.Cos(angle / 3);

                transform.localPosition = new Vector3(x, y, z);

                yield return null;
            }
        }
    }
}
