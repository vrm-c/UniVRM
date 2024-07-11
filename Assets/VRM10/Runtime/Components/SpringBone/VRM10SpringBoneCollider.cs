using System;
using UnityEngine;

namespace UniVRM10
{
    public enum VRM10SpringBoneColliderTypes
    {
        Sphere,
        Capsule,
        Plane,
        SphereInside,
        CapsuleInside,
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
                    Gizmos.color = Color.magenta;
                    DrawCapsuleGizmo(transform.localToWorldMatrix.MultiplyPoint(Offset), transform.localToWorldMatrix.MultiplyPoint(Tail), Radius);
                    break;

                case VRM10SpringBoneColliderTypes.Plane:
                    Gizmos.color = Color.magenta;
                    DrawPlane(transform.localToWorldMatrix, Offset, Normal, 0.05f);
                    break;

                case VRM10SpringBoneColliderTypes.SphereInside:
                    Gizmos.color = new Color(0.5f, 0, 1.0f);
                    Gizmos.DrawWireSphere(Offset, Radius);
                    break;

                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    Gizmos.color = new Color(0.5f, 0, 1.0f);
                    DrawCapsuleGizmo(transform.localToWorldMatrix.MultiplyPoint(Offset), transform.localToWorldMatrix.MultiplyPoint(Tail), Radius);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static void DrawCapsuleGizmo(Vector3 start, Vector3 end, float radius)
        {
            var tail = end - start;
            var distance = (end - start).magnitude;
            var backup = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.forward, tail), Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.DrawWireSphere(Vector3.forward * distance, radius);
            var capsuleEnd = Vector3.forward * distance;
            var offsets = new Vector3[] { new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Gizmos.DrawLine(offsets[i] * radius, capsuleEnd + offsets[i] * radius);
            }
            Gizmos.matrix = backup;
        }

        public static void DrawPlane(in Matrix4x4 m, in Vector3 offset, in Vector3 _n, float radius)
        {
            var n = _n.normalized;
            var backup = Gizmos.matrix;
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
                {
                    var o = offset;
                    Gizmos.DrawLine(o - xr, o + xr);
                }
                {
                    var o = offset + zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }
                {
                    var o = offset - zr;
                    Gizmos.DrawLine(o - xr, o + xr);
                }

                {
                    var o = offset;
                    Gizmos.DrawLine(o - zr, o + zr);
                }
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
            Gizmos.matrix = backup;
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
