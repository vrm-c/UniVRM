using System.Collections;
using UnityEngine;


namespace VRM
{
    public class AIUEO : MonoBehaviour
    {
        [SerializeField]
        public VRMBlendShapeProxy BlendShapes;
        private void Reset()
        {
            BlendShapes = GetComponent<VRMBlendShapeProxy>();
        }

        BlendShapeClipHandler m_appyerA;
        BlendShapeClipHandler m_appyerI;
        BlendShapeClipHandler m_appyerU;
        BlendShapeClipHandler m_appyerE;
        BlendShapeClipHandler m_appyerO;

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        private void Awake()
        {
            if (BlendShapes == null) BlendShapes = GetComponent<VRM.VRMBlendShapeProxy>();
            if (BlendShapes == null) return;
            if (BlendShapes.BlendShapeAvatar == null) return;
            var avatar = BlendShapes.BlendShapeAvatar;

            m_appyerA = new BlendShapeClipHandler(avatar.GetClip("A"), transform);
            m_appyerI = new BlendShapeClipHandler(avatar.GetClip("I"), transform);
            m_appyerU = new BlendShapeClipHandler(avatar.GetClip("U"), transform);
            m_appyerE = new BlendShapeClipHandler(avatar.GetClip("E"), transform);
            m_appyerO = new BlendShapeClipHandler(avatar.GetClip("O"), transform);
        }

        static IEnumerator RoutineNest(BlendShapeClipHandler applyer, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                if (applyer != null) applyer.Apply(value);
                yield return null;
            }
            if (applyer != null) applyer.Apply(1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                if (applyer != null) applyer.Apply(value);
                yield return null;
            }
            if (applyer != null) applyer.Apply(0);
            yield return new WaitForSeconds(wait * 2);
        }

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                var velocity = 0.1f;

                yield return RoutineNest(m_appyerA, velocity, m_wait);
                yield return RoutineNest(m_appyerI, velocity, m_wait);
                yield return RoutineNest(m_appyerU, velocity, m_wait);
                yield return RoutineNest(m_appyerE, velocity, m_wait);
                yield return RoutineNest(m_appyerO, velocity, m_wait);
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
