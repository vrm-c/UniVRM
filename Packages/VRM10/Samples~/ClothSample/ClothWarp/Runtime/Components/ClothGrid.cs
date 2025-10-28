using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10.ClothWarp.Components
{
    [AddComponentMenu("ClothWarp/ClothGrid")]
    [DisallowMultipleComponent]
    public class ClothGrid : MonoBehaviour
    {
        [SerializeField]
        public bool LoopIsClosed = false;

        [SerializeField]
        public List<ClothWarpRoot> Warps = new();

        public void Reset()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<ClothWarpRoot>(out var warp))
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

        void DrawWeft(ClothWarpRoot w0, ClothWarpRoot w1)
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
                if (p0.Transform != null && p1.Transform != null)
                {
                    Gizmos.DrawLine(p0.Transform.position, p1.Transform.position);
                }
            }
        }
    }
}