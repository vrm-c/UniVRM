using System.Collections;
using UnityEngine;


namespace UniVRM10.Samples
{
    public class AIUEO : MonoBehaviour
    {
        [SerializeField]
        public VRM10Controller VRM;
        private void Reset()
        {
            VRM = GetComponent<VRM10Controller>();
        }

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        private void Awake()
        {
            if (VRM == null)
            {
                VRM = GetComponent<VRM10Controller>();
            }
        }

        IEnumerator RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                VRM.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), value);
                yield return null;
            }
            VRM.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                VRM.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), value);
                yield return null;
            }
            VRM.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), 0);
            yield return new WaitForSeconds(wait * 2);
        }

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                var velocity = 0.1f;

                yield return RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa, velocity, m_wait);
                yield return RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih, velocity, m_wait);
                yield return RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou, velocity, m_wait);
                yield return RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee, velocity, m_wait);
                yield return RoutineNest(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh, velocity, m_wait);
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
