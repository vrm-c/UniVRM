using System.Collections;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// 喜怒哀楽驚を循環させる
    /// </summary>
    public class VRM10AutoExpression : MonoBehaviour
    {
        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        public float Happy = 0.0f;
        public float Angry = 0.0f;
        public float Sad = 0.0f;
        public float Relaxed = 0.0f;
        public float Surprised = 0.0f;

        void SetWeight(ExpressionPreset preset, float value)
        {
            switch (preset)
            {
                case ExpressionPreset.happy: Happy = value; break;
                case ExpressionPreset.angry: Angry = value; break;
                case ExpressionPreset.sad: Sad = value; break;
                case ExpressionPreset.relaxed: Relaxed = value; break;
                case ExpressionPreset.surprised: Surprised = value; break;
                default: break;
            }
        }

        IEnumerator RoutineNest(ExpressionPreset preset, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                SetWeight(preset, value);
                yield return null;
            }
            SetWeight(preset, 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                SetWeight(preset, value);
                yield return null;
            }
            SetWeight(preset, 0);
            yield return new WaitForSeconds(wait * 2);
        }

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                var velocity = 0.01f;

                yield return RoutineNest(ExpressionPreset.happy, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.angry, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.sad, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.relaxed, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.surprised, velocity, m_wait);
            }
        }

        private void OnEnable()
        {
            m_coroutine = StartCoroutine(Routine());
        }

        private void OnDisable()
        {
            StopCoroutine(m_coroutine);
        }
    }
}
