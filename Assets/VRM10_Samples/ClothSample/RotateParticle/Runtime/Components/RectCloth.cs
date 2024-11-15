using System.Collections.Generic;
using UnityEngine;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/RectCloth")]
    [DisallowMultipleComponent]
    public class RectCloth : MonoBehaviour
    {
        [SerializeField]
        public bool LoopIsClosed = false;

        [SerializeField]
        public List<Warp> Warps = new();

        void Reset()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<Warp>(out var warp))
                {
                    Warps.Add(warp);
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (Warps.Count == 0)
            {
                return;
            }

            // Gizmos.color = Color.red;
            Gizmos.color = new Color(1, 0.5f, 0);
            for (int i = 0; i < Warps.Count; ++i)
            {
                if (i + 1 == Warps.Count)
                {
                    if (LoopIsClosed)
                    {
                        DrawWeft(Warps[i], Warps[0]);
                    }
                }
                else
                {
                    DrawWeft(Warps[i], Warps[i + 1]);
                }
            }
        }

        void DrawWeft(Warp w0, Warp w1)
        {
            if (w0 == null || w1 == null)
            {
                return;
            }

            Gizmos.DrawLine(w0.transform.position, w1.transform.position);

            for (int i = 0; i < w0.Particles.Count && i < w1.Particles.Count; ++i)
            {
                var p0 = w0.Particles[i];
                var p1 = w1.Particles[i];
                if (p0 != null && p0.Transform != null && p1 != null && p1.Transform != null)
                {
                    Gizmos.DrawLine(p0.Transform.position, p1.Transform.position);
                }
            }
        }
    }
}