using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// VRMC_node_collider
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("VRM10/VRM10SpringBoneColliderGroup")]
    public class VRM10SpringBoneColliderGroup : MonoBehaviour
    {
        void Reset()
        {
            // default shape
            Colliders.Add(new VRM10SpringBoneCollider
            {
                ColliderType = VRM10SpringBoneColliderTypes.Capsule,
                Radius = 0.1f
            });
        }

        [SerializeField]
        public List<VRM10SpringBoneCollider> Colliders = new List<VRM10SpringBoneCollider>();

        public static void DrawWireCapsule(Vector3 headPos, Vector3 tailPos, float radius)
        {
            var headToTail = tailPos - headPos;
            if (headToTail.sqrMagnitude <= float.Epsilon)
            {
                Gizmos.DrawWireSphere(headPos, radius);
                return;
            }

            var forward = headToTail.normalized * radius;

            var xLen = Mathf.Abs(forward.x);
            var yLen = Mathf.Abs(forward.y);
            var zLen = Mathf.Abs(forward.z);
            var rightWorldAxis = (yLen > xLen && yLen > zLen) ? Vector3.right : Vector3.up;

            var up = Vector3.Cross(forward, rightWorldAxis).normalized * radius;
            var right = Vector3.Cross(up, forward).normalized * radius;

            const int division = 24;
            DrawWireCircle(headPos, up, right, division, division);
            DrawWireCircle(headPos, up, -forward, division, division / 2);
            DrawWireCircle(headPos, right, -forward, division, division / 2);

            DrawWireCircle(tailPos, up, right, division, division);
            DrawWireCircle(tailPos, up, forward, division, division / 2);
            DrawWireCircle(tailPos, right, forward, division, division / 2);

            Gizmos.DrawLine(headPos + right, tailPos + right);
            Gizmos.DrawLine(headPos - right, tailPos - right);
            Gizmos.DrawLine(headPos + up, tailPos + up);
            Gizmos.DrawLine(headPos - up, tailPos - up);
        }

        private static void DrawWireCircle(Vector3 centerPos, Vector3 xAxis, Vector3 yAxis, int division, int count)
        {
            for (var idx = 0; idx < division && idx < count; ++idx)
            {
                var s = ((idx + 0) % division) / (float)division * Mathf.PI * 2f;
                var t = ((idx + 1) % division) / (float)division * Mathf.PI * 2f;

                Gizmos.DrawLine(
                    centerPos + xAxis * Mathf.Cos(s) + yAxis * Mathf.Sin(s),
                    centerPos + xAxis * Mathf.Cos(t) + yAxis * Mathf.Sin(t)
                );
            }
        }

        public void DrawGizmos(Color color)
        {
            Gizmos.color = color;
            Matrix4x4 mat = transform.localToWorldMatrix;
            Gizmos.matrix = mat * Matrix4x4.Scale(new Vector3(
                1.0f / transform.lossyScale.x,
                1.0f / transform.lossyScale.y,
                1.0f / transform.lossyScale.z
                ));
            foreach (var y in Colliders)
            {
                switch (y.ColliderType)
                {
                    case VRM10SpringBoneColliderTypes.Sphere:
                        Gizmos.DrawWireSphere(y.Offset, y.Radius);
                        break;

                    case VRM10SpringBoneColliderTypes.Capsule:
                        DrawWireCapsule(y.Offset, y.Tail, y.Radius);
                        break;
                }
            }
        }
    }
}
