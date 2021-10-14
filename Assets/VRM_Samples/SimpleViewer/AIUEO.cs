using System.Collections;
using UnityEngine;


namespace VRM.SimpleViewer
{
    public class AIUEO : MonoBehaviour
    {
        [SerializeField]
        public VRMBlendShapeProxy BlendShapes;
        private void Reset()
        {
            BlendShapes = GetComponent<VRMBlendShapeProxy>();
        }

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        private void Awake()
        {
            if (BlendShapes == null)
            {
                BlendShapes = GetComponent<VRM.VRMBlendShapeProxy>();
            }
        }

        IEnumerator RoutineNest(BlendShapePreset preset, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), value);
                yield return null;
            }
            BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), value);
                yield return null;
            }
            BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0);
            yield return new WaitForSeconds(wait * 2);
        }

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                var velocity = 0.1f;

                yield return RoutineNest(BlendShapePreset.A, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.I, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.U, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.E, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.O, velocity, m_wait);
            }
        }

        private void OnEnable()
        {
            m_coroutine = StartCoroutine(Routine());
        }

        private void OnDisable()
        {
            Debug.Log("StopCoroutine");
            StopCoroutine(m_coroutine);
        }
    }
}
