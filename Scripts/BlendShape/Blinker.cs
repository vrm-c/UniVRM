using System.Collections;
using UnityEngine;


namespace VRM
{
    public class Blinker : MonoBehaviour
    {
        [SerializeField]
        public VRMBlendShapeProxy BlendShapes;
        private void Reset()
        {
            BlendShapes = GetComponent<VRMBlendShapeProxy>();
        }

        [SerializeField]
        float m_interVal = 5.0f;

        [SerializeField]
        float m_closingTime = 0.06f;

        [SerializeField]
        float m_openingSeconds = 0.03f;

        [SerializeField]
        float m_closeSeconds = 0.1f;

        Coroutine m_coroutine;

        static readonly string BLINK_NAME = BlendShapePreset.Blink.ToString();

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
                var waitTime = Time.time + Random.value * m_interVal;
                while (waitTime>Time.time)
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
                var closeSpeed = 1.0f / m_closeSeconds;
                while (true)
                {
                    value += Time.deltaTime * closeSpeed;
                    if (value >= 1.0f)
                    {
                        break;
                    }

                    BlendShapes.SetValue(BLINK_NAME, value);
                    yield return null;
                }
                BlendShapes.SetValue(BLINK_NAME, 1.0f);

                // wait...
                yield return new WaitForSeconds(m_closingTime);

                // open
                value = 1.0f;
                var openSpeed = 1.0f / m_openingSeconds;
                while (true)
                {
                    value -= Time.deltaTime * openSpeed;
                    if (value < 0)
                    {
                        break;
                    }

                    BlendShapes.SetValue(BLINK_NAME, value);
                    yield return null;
                }
                BlendShapes.SetValue(BLINK_NAME, 0);
            }
        }

        private void Start()
        {
            m_coroutine = StartCoroutine(BlinkRoutine());
        }

        private void Update()
        {
            /*
            var weight = BlendShapes.GetWeightByFlags(BlendShapeFlags.Eye, m_key);
            if (m_coroutine != null)
            {
                if (weight > 0)
                {
                    Debug.LogFormat("[Blinker] cancel: {0}", BlendShapes.Status);
                    // まばたきキャンセル
                    StopCoroutine(m_coroutine);
                    m_coroutine = null;
                    BlendShapes.Set(m_key, 0);
                }
            }
            else
            {
                if (weight == 0)
                {
                    Debug.Log("[Blinker] start");
                    // 開始
                    m_coroutine = StartCoroutine(BlinkRoutine());
                }
            }
            */
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
