using System.Collections.Generic;
using UnityEngine;


namespace VRM
{
    public class LookAtTargetSwitcher : MonoBehaviour
    {
        [SerializeField]
        List<Transform> m_targets = new List<Transform>();

        [SerializeField, Range(0, 90.0f)]
        float m_thresholdDegrees = 60.0f;

        [SerializeField]
        VRMLookAtHead m_lookAtHead;

        [SerializeField]
        Blinker m_blinker;

        private void Reset()
        {
#if UNITY_2022_3_OR_NEWER
            m_lookAtHead = GameObject.FindFirstObjectByType<VRMLookAtHead>();
            m_blinker = GameObject.FindFirstObjectByType<Blinker>();
#else
            m_lookAtHead = GameObject.FindObjectOfType<VRMLookAtHead>();
            m_blinker = GameObject.FindObjectOfType<Blinker>();
#endif
        }

        float CalcScore(Transform target)
        {
            return Vector3.Dot(m_lookAtHead.Head.forward, target.position - m_lookAtHead.Head.position);
        }

        Transform ChooseTarget()
        {
            Transform target = null;
            float maxScore = 0;
            var min = System.Math.Cos(m_thresholdDegrees * Mathf.Deg2Rad);
            foreach (var x in m_targets)
            {
                var score = CalcScore(x);
                if (score > min && score > maxScore)
                {
                    maxScore = score;
                    target = x;
                }
            }
            return target;
        }

        Transform m_lastTarget;

        private void Update()
        {
            if (m_targets == null || m_targets.Count == 0) return;

            var target = ChooseTarget();
            if (target != m_lastTarget)
            {
                // blink
                //Debug.Log("request");
                m_lastTarget = target;
                m_blinker.Request = true;
            }

            Vector3 targetPosition;
            if (target == null)
            {
                // forward
                targetPosition = m_lookAtHead.Head.position + m_lookAtHead.Head.forward * 20.0f;
            }
            else
            {
                targetPosition = target.position;
            }
            // half move
            transform.position += (targetPosition - transform.position) * 0.5f;
        }
    }
}
