using System;
using System.Numerics;

namespace UniGLTF
{
    public static class NumericsExtensions
    {
        const float EPSILON = 1e-5f;

        public static bool NearlyEqual(this float lhs, float rhs)
        {
            return Math.Abs(lhs - rhs) <= EPSILON;
        }

        public static bool NearlyEqual(this Vector3 lhs, Vector3 rhs)
        {
            if (Math.Abs(lhs.X - rhs.X) > EPSILON) return false;
            if (Math.Abs(lhs.Y - rhs.Y) > EPSILON) return false;
            if (Math.Abs(lhs.Z - rhs.Z) > EPSILON) return false;
            return true;
        }

        public static bool NearlyEqual(this Quaternion lhs, Quaternion rhs)
        {
            if (Math.Abs(lhs.X - rhs.X) > EPSILON) return false;
            if (Math.Abs(lhs.Y - rhs.Y) > EPSILON) return false;
            if (Math.Abs(lhs.Z - rhs.Z) > EPSILON) return false;
            if (Math.Abs(lhs.W - rhs.W) > EPSILON) return false;
            return true;
        }

        public const float TO_RAD = (float)(Math.PI / 180.0);

        public static Vector2 UVVerticalFlip(this Vector2 src)
        {
            return new Vector2(src.X, 1.0f - src.Y);
        }

        public static Vector3 ReverseX(this Vector3 src)
        {
            return new Vector3(-src.X, src.Y, src.Z);
        }

        public static Vector3 ReverseZ(this Vector3 src)
        {
            return new Vector3(src.X, src.Y, -src.Z);
        }

        public static (Vector3, float) GetAxisAngle(this Quaternion q)
        {
            var qw = q.W;
            if (qw == 1)
            {
                return (Vector3.UnitX, 0);
            }
            var angle = 2 * Math.Acos(qw);
            var x = q.X / Math.Sqrt(1 - qw * qw);
            var y = q.Y / Math.Sqrt(1 - qw * qw);
            var z = q.Z / Math.Sqrt(1 - qw * qw);
            return (new Vector3((float)x, (float)y, (float)z), (float)angle);
        }

        public static Quaternion ReverseX(this Quaternion src)
        {
            var (axis, angle) = src.GetAxisAngle();
            return Quaternion.CreateFromAxisAngle(axis.ReverseX(), -angle);
        }

        public static Quaternion ReverseZ(this Quaternion src)
        {
            var (axis, angle) = src.GetAxisAngle();
            return Quaternion.CreateFromAxisAngle(axis.ReverseZ(), -angle);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.X = matrix.M41;
            position.Y = matrix.M42;
            position.Z = matrix.M43;
            return position;
        }

        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            return Quaternion.CreateFromRotationMatrix(matrix);
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.X = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14).Length();
            scale.Y = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24).Length();
            scale.Z = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34).Length();
            return scale;
        }

        public static Matrix4x4 FromTRS(Vector3 t, Quaternion r, Vector3 s)
        {
            var tt = Matrix4x4.CreateTranslation(t);
            var rr = Matrix4x4.CreateFromQuaternion(r);
            var ss = Matrix4x4.CreateScale(s);
            // return tt * rr * ss;
            return ss * rr * tt;
        }

        public static (Vector3, Quaternion, Vector3) Decompose(this Matrix4x4 m)
        {
            var s = m.ExtractScale();
            var mm = Matrix4x4.CreateScale(1.0f / s.X, 1.0f / s.Y, 1.0f / s.Z) * m;
            return (mm.ExtractPosition(), mm.ExtractRotation(), s);
        }

        public static bool IsOnlyTranslation(this Matrix4x4 m)
        {
            if (m.M11 != 1.0f) return false;
            if (m.M22 != 1.0f) return false;
            if (m.M33 != 1.0f) return false;
            if (m.M12 != 0) return false;
            if (m.M13 != 0) return false;
            if (m.M23 != 0) return false;
            if (m.M21 != 0) return false;
            if (m.M31 != 0) return false;
            if (m.M32 != 0) return false;
            return true;
        }

        /// <summary>
        /// 移動 z反転
        /// 回転 z反転
        /// 拡大 据え置き
        ///
        /// これでいいのか？
        /// </summary>
        public static Matrix4x4 ReverseZ(this Matrix4x4 m)
        {
            if (m.IsOnlyTranslation())
            {
                var ret = m;
                // R, R, R, 0
                // R, R, R, 0
                // R, R, R, 0
                // T, T, T, 1
                ret.M43 = -ret.M43;
                return ret;
            }
            else
            {
                var (t, r, s) = m.Decompose();
                return FromTRS(t.ReverseZ(), r.ReverseZ(), s);
            }
        }

        public static Matrix4x4 ReverseX(this Matrix4x4 m)
        {
            if (m.IsOnlyTranslation())
            {
                var ret = m;
                // R, R, R, 0
                // R, R, R, 0
                // R, R, R, 0
                // T, T, T, 1
                ret.M41 = -ret.M41;
                return ret;
            }
            else
            {
                var (t, r, s) = m.Decompose();
                return FromTRS(t.ReverseX(), r.ReverseX(), s);
            }
        }

    }
}
