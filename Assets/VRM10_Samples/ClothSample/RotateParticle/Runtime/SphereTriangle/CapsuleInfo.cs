using UnityEngine;
using UniVRM10;

namespace SphereTriangle
{
    struct CapsuleInfo
    {
        public VRM10SpringBoneCollider Collider;
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

        public CapsuleInfo(in Triangle t, VRM10SpringBoneCollider collider)
        {
            Collider = collider;
            Triangle = t;
            var headDistance = t.Plane.GetDistanceToPoint(collider.HeadWorldPosition);
            var tailDistance = t.Plane.GetDistanceToPoint(collider.TailWorldPosition);
            if (headDistance <= tailDistance)
            {
                Reverse = false;
                MinDistance = headDistance;
                MaxDistance = tailDistance;
                MinOnPlane = t.Plane.ClosestPointOnPlane(collider.HeadWorldPosition);
                MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.TailWorldPosition);
                MinClamp = collider.HeadWorldPosition;
                MaxClamp = collider.TailWorldPosition;
                MinPos = collider.HeadWorldPosition;
                MaxPos = collider.TailWorldPosition;
            }
            else
            {
                Reverse = true;
                MaxDistance = headDistance;
                MinDistance = tailDistance;
                MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.HeadWorldPosition);
                MinOnPlane = t.Plane.ClosestPointOnPlane(collider.TailWorldPosition);
                MaxClamp = collider.HeadWorldPosition;
                MinClamp = collider.TailWorldPosition;
                MaxPos = collider.HeadWorldPosition;
                MinPos = collider.TailWorldPosition;
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
            Gizmos.DrawLine(Reverse ? Collider.TailWorldPosition : Collider.HeadWorldPosition, MinOnPlane);
            Gizmos.DrawWireSphere(MinOnPlane, 0.01f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Reverse ? Collider.HeadWorldPosition : Collider.TailWorldPosition, MaxOnPlane);
            Gizmos.DrawWireSphere(MaxOnPlane, 0.01f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(MinOnPlaneClamp, MaxOnPlaneClamp);

            Gizmos.color = Color.green;
            Triangle.DrawGizmos();
        }
    }
}