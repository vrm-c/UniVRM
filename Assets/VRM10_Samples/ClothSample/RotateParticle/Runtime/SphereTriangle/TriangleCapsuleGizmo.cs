using UnityEngine;
using UniVRM10;


namespace SphereTriangle
{
    public class TriangleCapsuleGizmo : MonoBehaviour
    {
        public Transform B;
        public Transform C;
        public VRM10SpringBoneCollider Capsule;

        void Reset()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                switch (i)
                {
                    case 0:
                        if (this.B == null) B = transform.GetChild(i);
                        break;

                    case 1:
                        if (this.C == null) C = transform.GetChild(i);
                        break;
                }
            }
        }

        [SerializeField]
        TriangleCapsuleCollisionSolver _solver = new();

        // public void OnDrawGizmos()
        // {
        //     if (B == null) return;
        //     if (C == null) return;
        //     var t = new Triangle(transform.position, B.position, C.position);

        //     if (Capsule.ColliderType != VRM10SpringBoneColliderTypes.Capsule) return;
        //     var capsule = new LineSegment(Capsule.HeadWorldPosition, Capsule.TailWorldPosition);

        //     _solver.BeginFrame();
        //     var result = _solver.Collide(t, Capsule, capsule, Capsule.Radius);
        //     var type = result.TryGetClosest(out var l);
        //     if (!type.HasValue)
        //     {
        //         Gizmos.color = Color.gray;
        //         t.DrawGizmos();
        //         return;
        //     }

        //     _solver.DrawGizmos(t, 1.0f, Capsule.Radius);

        //     Gizmos.color = Color.magenta;
        //     Gizmos.DrawLine(l.Start, l.End);
        //     Gizmos.DrawSphere(l.End, 0.01f);
        //     Gizmos.DrawWireSphere(l.Start, 0.02f);

        //     var delta = l.Vector.normalized * (Capsule.Radius - l.Length);
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawLine(l.End, l.End + delta);
        //     Gizmos.DrawSphere(l.End + delta, 0.01f);
        // }
    }
}