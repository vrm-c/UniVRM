using UnityEngine;
using UniVRM10;

namespace SphereTriangle
{
    public class LineDistanceGizmo : MonoBehaviour
    {
        public VRM10SpringBoneCollider LineA;
        public VRM10SpringBoneCollider LineB;

        void Reset()
        {
            if (LineA == null)
            {
                LineA = GetComponent<VRM10SpringBoneCollider>();
            }
        }

        public void OnDrawGizmos()
        {
            if (LineA.ColliderType == VRM10SpringBoneColliderTypes.Capsule && LineB.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
            {
                var a = new LineSegment(LineA.HeadWorldPosition, LineA.TailWorldPosition);
                var b = new LineSegment(LineB.HeadWorldPosition, LineB.TailWorldPosition);
                var (s, t) = LineSegment.CalcClosest(a, b);
                var a_s = a.GetPoint(s);
                var b_t = b.GetPoint(t);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(a_s, b_t);

                Gizmos.color = Color.gray;
                if (s < 0)
                {
                    Gizmos.DrawLine(LineA.HeadWorldPosition, a_s);
                }
                else if (s > 1)
                {
                    Gizmos.DrawLine(LineA.TailWorldPosition, a_s);
                }
                else
                {
                    Gizmos.DrawWireSphere(a_s, 0.05f);
                }
                if (t < 0)
                {
                    Gizmos.DrawLine(LineB.HeadWorldPosition, b_t);
                }
                else if (t > 1)
                {
                    Gizmos.DrawLine(LineB.HeadWorldPosition, b_t);
                }
                else
                {
                    Gizmos.DrawWireSphere(b_t, 0.05f);
                }
            }
        }
    }
}