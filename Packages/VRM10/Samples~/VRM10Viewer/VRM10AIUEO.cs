using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10AIUEO : MonoBehaviour
    {
        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        public float Aa = 0.0f;
        public float Ih = 0.0f;
        public float Ou = 0.0f;
        public float Ee = 0.0f;
        public float Oh = 0.0f;

        void SetWeight(ExpressionPreset preset, float value)
        {
            switch (preset)
            {
                case ExpressionPreset.aa: Aa = value; break;
                case ExpressionPreset.ih: Ih = value; break;
                case ExpressionPreset.ou: Ou = value; break;
                case ExpressionPreset.ee: Ee = value; break;
                case ExpressionPreset.oh: Oh = value; break;
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

                var velocity = 0.1f;

                yield return RoutineNest(ExpressionPreset.aa, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.ih, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.ou, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.ee, velocity, m_wait);
                yield return RoutineNest(ExpressionPreset.oh, velocity, m_wait);
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
