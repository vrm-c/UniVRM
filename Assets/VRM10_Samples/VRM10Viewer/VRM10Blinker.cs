using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// VRMBlendShapeProxy によるランダムに瞬きするサンプル。
    /// VRMBlendShapeProxy のある GameObject にアタッチする。
    /// </summary>
    public class VRM10Blinker : MonoBehaviour
    {
        Vrm10Instance m_controller;

        [FormerlySerializedAs("m_interVal")]
        [SerializeField]
        public float Interval = 5.0f;

        [FormerlySerializedAs("m_closingTime")]
        [SerializeField]
        public float ClosingTime = 0.06f;

        [FormerlySerializedAs("m_openingSeconds")]
        [SerializeField]
        public float OpeningSeconds = 0.03f;

        [FormerlySerializedAs("m_closeSeconds")]
        [SerializeField]
        public float CloseSeconds = 0.1f;

        Coroutine m_coroutine;

        float m_nextRequest;
        bool m_request;
        public bool Request
        {
            get { return m_request; }
            set
            {
                if (Time.time < m_nextRequest)
                {
                    return;
                }
                m_request = value;
                m_nextRequest = Time.time + 1.0f;
            }
        }

        IEnumerator BlinkRoutine()
        {
            while (true)
            {
                var waitTime = Time.time + Random.value * Interval;
                while (waitTime > Time.time)
                {
                    if (Request)
                    {
                        m_request = false;
                        break;
                    }
                    yield return null;
                }

                // close
                var value = 0.0f;
                var closeSpeed = 1.0f / CloseSeconds;
                while (true)
                {
                    value += Time.deltaTime * closeSpeed;
                    if (value >= 1.0f)
                    {
                        break;
                    }

                    m_controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), value);
                    yield return null;
                }
                m_controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), 1.0f);

                // wait...
                yield return new WaitForSeconds(ClosingTime);

                // open
                value = 1.0f;
                var openSpeed = 1.0f / OpeningSeconds;
                while (true)
                {
                    value -= Time.deltaTime * openSpeed;
                    if (value < 0)
                    {
                        break;
                    }

                    m_controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), value);
                    yield return null;
                }
                m_controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), 0);
            }
        }

        private void OnEnable()
        {
            m_controller = GetComponent<Vrm10Instance>();
            m_coroutine = StartCoroutine(BlinkRoutine());
        }

        private void OnDisable()
        {
            if (m_coroutine != null)
            {
                StopCoroutine(m_coroutine);
                m_coroutine = null;
            }
        }
    }
}
