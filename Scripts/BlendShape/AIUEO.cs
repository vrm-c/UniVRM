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

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                if (m_appyerA != null) m_appyerA.Apply(1.0f);
                yield return new WaitForSeconds(m_wait);
                if (m_appyerA != null) m_appyerA.Apply(0);

                if (m_appyerI != null) m_appyerI.Apply(1.0f);
                yield return new WaitForSeconds(m_wait);
                if (m_appyerI != null) m_appyerI.Apply(0);

                if (m_appyerU != null) m_appyerU.Apply(1.0f);
                yield return new WaitForSeconds(m_wait);
                if (m_appyerU != null) m_appyerU.Apply(0);

                if (m_appyerE != null) m_appyerE.Apply(1.0f);
                yield return new WaitForSeconds(m_wait);
                if (m_appyerE != null) m_appyerE.Apply(0);

                if (m_appyerO != null) m_appyerO.Apply(1.0f);
                yield return new WaitForSeconds(m_wait);
                if (m_appyerO != null) m_appyerO.Apply(0);
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
