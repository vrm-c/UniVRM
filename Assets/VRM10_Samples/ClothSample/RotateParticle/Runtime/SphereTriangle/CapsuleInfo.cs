using UnityEngine;

namespace SphereTriangle
{
    struct CapsuleInfo
    {
        public SphereCapsuleCollider Collider;
        public Triangle Triangle;

        public Vector3 MinOnPlane;
        public float MinDistance;
        public float MinDistanceClmap;
        public Vector3 MinOnPlaneClamp;
        public Vector3 MinPos;
        public Vector3 MinClamp;

        public Vector3 MaxOnPlane;
        public float MaxDistance;
        public float MaxDistanceClamp;
        public Vector3 MaxOnPlaneClamp;
        public Vector3 MaxPos;
        public Vector3 MaxClamp;

        /// <summary>
        /// min and max is tail and head.
        /// </summary>
        public bool Reverse;

        public bool Intersected;

        public CapsuleInfo(in Triangle t, SphereCapsuleCollider collider)
        {
            Collider = collider;
            Triangle = t;
            var headDistance = t.Plane.GetDistanceToPoint(collider.transform.position);
            var tailDistance = t.Plane.GetDistanceToPoint(collider.Tail.position);
            if (headDistance <= tailDistance)
            {
                Reverse = false;
                MinDistance = headDistance;
                MaxDistance = tailDistance;
                MinOnPlane = t.Plane.ClosestPointOnPlane(collider.transform.position);
                MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.Tail.position);
                MinClamp = collider.transform.position;
                MaxClamp = collider.Tail.position;
                MinPos = collider.transform.position;
                MaxPos = collider.Tail.position;
            }
            else
            {
                Reverse = true;
                MaxDistance = headDistance;
                MinDistance = tailDistance;
                MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.transform.position);
                MinOnPlane = t.Plane.ClosestPointOnPlane(collider.Tail.position);
                MaxClamp = collider.transform.position;
                MinClamp = collider.Tail.position;
                MaxPos = collider.transform.position;
                MinPos = collider.Tail.position;
            }

            // Intersect
            Intersected = true;
            MinDistanceClmap = MinDistance;
            MinOnPlaneClamp = MinOnPlane;
            MaxDistanceClamp = MaxDistance;
            MaxOnPlaneClamp = MaxOnPlane;
            if (MinDistance < -Collider.Radius)
            {
                if (MaxDistance < -Collider.Radius)
                {
                    Intersected = false;
                }
                else
                {
                    // clamp Min
                    MinDistanceClmap = -Collider.Radius;
                }
            }
            else if (MaxDistance > Collider.Radius)
            {
                if (MinDistance > Collider.Radius)
                {
                    Intersected = false;
                }
                else
                {
                    // clamp Max
                    MaxDistanceClamp = Collider.Radius;
                }
            }

            // 
            MaxOnPlaneClamp = Vector3.Lerp(MinOnPlane, MaxOnPlane, (MaxDistanceClamp - MinDistance) / (MaxDistance - MinDistance));
            MinOnPlaneClamp = Vector3.Lerp(MaxOnPlane, MinOnPlane, (MinDistanceClmap - MaxDistance) / (MinDistance - MaxDistance));
            MaxClamp = Vector3.Lerp(MinPos, MaxPos, (MaxDistanceClamp - MinDistance) / (MaxDistance - MinDistance));
            MinClamp = Vector3.Lerp(MaxPos, MinPos, (MinDistanceClmap - MaxDistance) / (MinDistance - MaxDistance));
        }

        public void DrawGizmo()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Reverse ? Collider.Tail.position : Collider.transform.position, MinOnPlane);
            Gizmos.DrawWireSphere(MinOnPlane, 0.01f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Reverse ? Collider.transform.position : Collider.Tail.position, MaxOnPlane);
            Gizmos.DrawWireSphere(MaxOnPlane, 0.01f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(MinOnPlaneClamp, MaxOnPlaneClamp);

            Gizmos.color = Color.green;
            Triangle.DrawGizmos();
        }
    }
}