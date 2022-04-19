using System.Collections;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10AIUEO : MonoBehaviour
    {
        [SerializeField]
        public Vrm10Instance Controller;
        private void Reset()
        {
            Controller = GetComponent<Vrm10Instance>();
        }

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        private void Awake()
        {
            if (Controller == null)
            {
                Controller = GetComponent<Vrm10Instance>();
            }
        }

        IEnumerator RoutineNest(ExpressionPreset preset, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                Controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), value);
                yield return null;
            }
            Controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                Controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), value);
                yield return null;
            }
            Controller.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), 0);
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
