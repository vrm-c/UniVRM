/// <summary>
/// Unity互換のためのダミー
/// </summary>

#if UNITY_5_6_OR_NEWER
#else
using System;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;
    }

    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public static Vector3 zero => new Vector3();

        public bool Equals(Vector3 other)
        {
            if (x != other.x) return false;
            if (y != other.y) return false;
            if (z != other.z) return false;
            return true;
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
}
#endif
