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
                var a = new LineSegment(LineA.transform.TransformPoint(LineA.Offset), LineA.transform.TransformPoint(LineA.TailOrNormal));
                var b = new LineSegment(LineB.transform.TransformPoint(LineB.Offset), LineB.transform.TransformPoint(LineB.TailOrNormal));
                var (s, t) = LineSegment.CalcClosest(a, b);
                var a_s = a.GetPoint(s);
                var b_t = b.GetPoint(t);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(a_s, b_t);

                Gizmos.color = Color.gray;
                if (s < 0)
                {
                    Gizmos.DrawLine(LineA.transform.TransformPoint(LineA.Offset), a_s);
                }
                else if (s > 1)
                {
                    Gizmos.DrawLine(LineA.transform.TransformPoint(LineA.TailOrNormal), a_s);
                }
                else
                {
                    Gizmos.DrawWireSphere(a_s, 0.05f);
                }
                if (t < 0)
                {
                    Gizmos.DrawLine(LineB.transform.TransformPoint(LineB.Offset), b_t);
                }
                else if (t > 1)
                {
                    Gizmos.DrawLine(LineB.transform.TransformPoint(LineB.TailOrNormal), b_t);
                }
                else
                {
                    Gizmos.DrawWireSphere(b_t, 0.05f);
                }
            }
        }
    }
}