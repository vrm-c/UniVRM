using System;
using UnityEngine;

namespace UniVRM10
{
    public enum VRM10SpringBoneColliderTypes
    {
        Sphere,
        Capsule,
        Plane,
        SphereIside,
    }

    [Serializable]
    public class VRM10SpringBoneCollider : MonoBehaviour
    {
        public VRM10SpringBoneColliderTypes ColliderType;

        /// <summary>bone local position</summary>
        public Vector3 Offset;

        [Range(0, 1.0f)]
        public float Radius;

        /// <summary>bone local position</summary>
        public Vector3 Tail;

        public Vector3 Normal = Vector3.up;

        public Vector3 TailOrNormal => ColliderType == VRM10SpringBoneColliderTypes.Plane ? Normal : Tail;

        public static int SelectedGuid;

        public bool IsSelected => GetInstanceID() == SelectedGuid;

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            switch (ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(Offset, Radius);
                    break;

                case VRM10SpringBoneColliderTypes.Capsule:
                    Gizmos.color = new Color(1.0f, 0.1f, 0.1f);
                    Gizmos.DrawWireSphere(Offset, Radius);
                    Gizmos.DrawWireSphere(Tail, Radius);
                    Gizmos.DrawLine(Offset, Tail);
                    break;

                case VRM10SpringBoneColliderTypes.Plane:
                    Gizmos.color = new Color(1.0f, 0.5f, 0.0f);
                    // normal offset
                    DrawPlane(transform.localToWorldMatrix, Offset, Normal, Radius);
                    break;

                case VRM10SpringBoneColliderTypes.SphereIside:
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(Offset, Radius);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static void DrawPlane(in Matrix4x4 m, in Vector3 offset, in Vector3 _n, float radius)
        {
            var n = _n.normalized;
            Gizmos.matrix = m;
            Gizmos.DrawLine(offset, offset + n * radius);

            // N Y-Axis として
            // XZ 平面を描画する。
            var z = Vector3.Cross(Vector3.right, n);
            if (z.magnitude > 1e-4)
            {
                var zr = z * radius;
                var x = Vector3.Cross(n, z);
                var xr = x * radius;
                Gizmos.DrawLine(-xr, xr);
                {
                    var o = offset + zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }
                {
                    var o = offset - zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }

                Gizmos.DrawLine(-zr, zr);
                {
                    var o = offset + xr;
                    Gizmos.DrawLine(o - zr, o + zr);
                }
                {
                    var o = offset - xr;
                    Gizmos.DrawLine(o - zr, o + zr);
                }
            }
            else
            {
                // N と ローカル X-Axis が重なっていた場合
                var x = Vector3.Cross(n, Vector3.forward);
                var xr = x * radius;
                var _z = Vector3.Cross(x, n);
                var zr = _z * radius;

                Gizmos.DrawLine(-xr, xr);
                {
                    var o = offset + zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }
                {
                    var o = offset - zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }

                Gizmos.DrawLine(-zr, zr);
                {
                    var o = offset + xr;
                    Gizmos.DrawLine(o - zr, o + zr);
                }
                {
                    var o = offset - xr;
                    Gizmos.DrawLine(o - zr, o + zr);
                }
            }
        }

        public string GetIdentificationName()
        {
            var index = 0;
            var count = 0;

            var colliders = transform.GetComponents<VRM10SpringBoneCollider>();
            foreach (var collider in colliders)
            {
                if (collider.ColliderType == ColliderType)
                {
                    count++;
                }
                if (collider == this)
                {
                    index = count;
                }
            }

            if (count > 1)
            {
                return ColliderType.ToString() + index.ToString();
            }
            else
            {
                return ColliderType.ToString();
            }
        }
    }
}
