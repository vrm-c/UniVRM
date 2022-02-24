using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public struct PosRot
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public static PosRot FromGlobalTransform(Transform t)
        {
            return new PosRot
            {
                Position = t.position,
                Rotation = t.rotation,
            };
        }
    }

    public class BlendShape
    {
        public string Name;

        public BlendShape(string name)
        {
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector3>();
        }

        public BlendShape(string name, int vertexCount, bool hasPositions, bool hasNormals, bool hasTangents)
        {
            Name = name;
            if (hasPositions)
            {
                Positions = new List<Vector3>(vertexCount);
            }
            else
            {
                Positions = new List<Vector3>();
            }

            if (hasNormals)
            {
                Normals = new List<Vector3>(vertexCount);
            }
            else
            {
                Normals = new List<Vector3>();
            }

            if (hasTangents)
            {
                Tangents = new List<Vector3>(vertexCount);
            }
            else
            {
                Tangents = new List<Vector3>();
            }
        }

        public List<Vector3> Positions { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Vector3> Tangents { get; private set; }
    }

    public static class UnityExtensions
    {
        const float EPSILON = 1e-5f;

        public static bool NearlyEqual(this float lhs, float rhs)
        {
            return Math.Abs(lhs - rhs) <= EPSILON;
        }

        public static bool NearlyEqual(this Vector3 lhs, Vector3 rhs)
        {
            if (Math.Abs(lhs.x - rhs.x) > EPSILON) return false;
            if (Math.Abs(lhs.y - rhs.y) > EPSILON) return false;
            if (Math.Abs(lhs.z - rhs.z) > EPSILON) return false;
            return true;
        }

        public static bool NearlyEqual(this Quaternion lhs, Quaternion rhs)
        {
            if (Math.Abs(lhs.x - rhs.x) > EPSILON) return false;
            if (Math.Abs(lhs.y - rhs.y) > EPSILON) return false;
            if (Math.Abs(lhs.z - rhs.z) > EPSILON) return false;
            if (Math.Abs(lhs.w - rhs.w) > EPSILON) return false;
            return true;
        }

        public static (Vector3, Quaternion, Vector3) Decompose(this Matrix4x4 m)
        {
            return (m.ExtractPosition(), m.ExtractRotation(), m.ExtractScale());
        }

        public static Vector2 UVVerticalFlip(this Vector2 src)
        {
            return new Vector2(src.x, 1.0f - src.y);
        }

        public static Vector4 ReverseZ(this Vector4 v)
        {
            return new Vector4(v.x, v.y, -v.z, v.w);
        }

        public static Vector4 ReverseX(this Vector4 v)
        {
            return new Vector4(-v.x, v.y, v.z, v.w);
        }

        public static Vector3 ReverseZ(this Vector3 v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }

        public static Vector3 ReverseX(this Vector3 v)
        {
            return new Vector3(-v.x, v.y, v.z);
        }

        [Obsolete]
        public static Vector2 ReverseY(this Vector2 v)
        {
            return new Vector2(v.x, -v.y);
        }

        public static Vector2 ReverseUV(this Vector2 v)
        {
            return new Vector2(v.x, 1.0f - v.y);
        }

        public static Quaternion ReverseZ(this Quaternion q)
        {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return Quaternion.AngleAxis(-angle, ReverseZ(axis));
        }

        public static Quaternion ReverseX(this Quaternion q)
        {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return Quaternion.AngleAxis(-angle, ReverseX(axis));
        }

        public static Matrix4x4 Matrix4x4FromColumns(Vector4 c0, Vector4 c1, Vector4 c2, Vector4 c3)
        {
#if UNITY_2017_1_OR_NEWER
            return new Matrix4x4(c0, c1, c2, c3);
#else
            var m = default(Matrix4x4);
            m.SetColumn(0, c0);
            m.SetColumn(1, c1);
            m.SetColumn(2, c2);
            m.SetColumn(3, c3);
            return m;
#endif
        }

        public static Matrix4x4 Matrix4x4FromRotation(Quaternion q)
        {
#if UNITY_2017_1_OR_NEWER
            return Matrix4x4.Rotate(q);
#else
            var m = default(Matrix4x4);
            m.SetTRS(Vector3.zero, q, Vector3.one);
            return m;
#endif
        }

        public static Matrix4x4 ReverseZ(this Matrix4x4 m)
        {
            m.SetTRS(m.ExtractPosition().ReverseZ(), m.ExtractRotation().ReverseZ(), m.ExtractScale());
            return m;
        }

        public static Matrix4x4 ReverseX(this Matrix4x4 m)
        {
            m.SetTRS(m.ExtractPosition().ReverseX(), m.ExtractRotation().ReverseX(), m.ExtractScale());
            return m;
        }

        public static Matrix4x4 MatrixFromArray(float[] values)
        {
            var m = new Matrix4x4();
            m.m00 = values[0];
            m.m10 = values[1];
            m.m20 = values[2];
            m.m30 = values[3];
            m.m01 = values[4];
            m.m11 = values[5];
            m.m21 = values[6];
            m.m31 = values[7];
            m.m02 = values[8];
            m.m12 = values[9];
            m.m22 = values[10];
            m.m32 = values[11];
            m.m03 = values[12];
            m.m13 = values[13];
            m.m23 = values[14];
            m.m33 = values[15];
            return m;
        }

        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            return matrix.rotation;
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        public static bool Nearly(in Matrix4x4 lhs, in Matrix4x4 rhs, float epsilon = 1e-3f)
        {
            if (Mathf.Abs(lhs.m00 - rhs.m00) > epsilon) return false;
            if (Mathf.Abs(lhs.m01 - rhs.m01) > epsilon) return false;
            if (Mathf.Abs(lhs.m02 - rhs.m02) > epsilon) return false;
            if (Mathf.Abs(lhs.m03 - rhs.m03) > epsilon) return false;
            if (Mathf.Abs(lhs.m10 - rhs.m10) > epsilon) return false;
            if (Mathf.Abs(lhs.m11 - rhs.m11) > epsilon) return false;
            if (Mathf.Abs(lhs.m12 - rhs.m12) > epsilon) return false;
            if (Mathf.Abs(lhs.m13 - rhs.m13) > epsilon) return false;
            if (Mathf.Abs(lhs.m20 - rhs.m20) > epsilon) return false;
            if (Mathf.Abs(lhs.m21 - rhs.m21) > epsilon) return false;
            if (Mathf.Abs(lhs.m22 - rhs.m22) > epsilon) return false;
            if (Mathf.Abs(lhs.m23 - rhs.m23) > epsilon) return false;
            if (Mathf.Abs(lhs.m30 - rhs.m30) > epsilon) return false;
            if (Mathf.Abs(lhs.m31 - rhs.m31) > epsilon) return false;
            if (Mathf.Abs(lhs.m32 - rhs.m32) > epsilon) return false;
            if (Mathf.Abs(lhs.m33 - rhs.m33) > epsilon) return false;
            return true;
        }

        public static (Vector3 T, Quaternion R, Vector3 S) Extract(this Matrix4x4 m)
        {
            if (m.determinant < 0)
            {
                // ミラーリングを試行する

                // -X
                {
                    var mm = m * Matrix4x4.Scale(new Vector3(-1, 1, 1));
                    var ss = mm.ExtractScale();
                    mm = mm * Matrix4x4.Scale(new Vector3(1 / ss.x, 1 / ss.y, 1 / ss.z));
                    var tt = mm.ExtractPosition();
                    var rr = mm.ExtractRotation();
                    ss.x = -ss.x;
                    var mmm = Matrix4x4.TRS(tt, rr, ss);
                    if (Nearly(m, mmm))
                    {
                        return (tt, rr, ss);
                    }
                }
            }

            var s = m.ExtractScale();
            var t = m.ExtractPosition();
            var r = m.ExtractRotation();
            return (t, r, s);
        }

        public static string RelativePathFrom(this Transform self, Transform root)
        {
            var path = new List<String>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return String.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }

        public static Transform GetChildByName(this Transform self, string childName)
        {
            foreach (Transform child in self)
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            throw new KeyNotFoundException();
        }

        public static Transform GetFromPath(this Transform self, string path)
        {
            var current = self;

            var split = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var childName in split)
            {
                current = current.GetChildByName(childName);
            }

            return current;
        }

        public static IEnumerable<Transform> GetChildren(this Transform self)
        {
            foreach (Transform child in self)
            {
                yield return child;
            }
        }

        public static IEnumerable<Transform> Traverse(this Transform t)
        {
            yield return t;
            foreach (Transform x in t)
            {
                foreach (Transform y in x.Traverse())
                {
                    yield return y;
                }
            }
        }

        [Obsolete("Use FindDescendant(name)")]
        public static Transform FindDescenedant(this Transform t, string name)
        {
            return FindDescendant(t, name);
        }

        public static Transform FindDescendant(this Transform t, string name)
        {
            return t.Traverse().First(x => x.name == name);
        }

        public static IEnumerable<Transform> Ancestors(this Transform t)
        {
            yield return t;
            if (t.parent != null)
            {
                foreach (Transform x in t.parent.Ancestors())
                {
                    yield return x;
                }
            }
        }

        public static float[] ToArray(this Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        public static float[] ToArray(this Vector3 v)
        {
            return new float[] { v.x, v.y, v.z };
        }

        public static float[] ToArray(this Vector4 v)
        {
            return new float[] { v.x, v.y, v.z, v.w };
        }

        public static void ReverseRecursive(this Transform root, IAxisInverter axisInverter)
        {
            var globalMap = root.Traverse().ToDictionary(x => x, x => PosRot.FromGlobalTransform(x));

            foreach (var x in root.Traverse())
            {
                x.position = axisInverter.InvertVector3(globalMap[x].Position);
                x.rotation = axisInverter.InvertQuaternion(globalMap[x].Rotation);
            }
        }

        public static Mesh GetSharedMesh(this Transform t)
        {
            var meshFilter = t.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return meshFilter.sharedMesh;
            }

            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.sharedMesh;
            }

            return null;
        }

        public static Material[] GetSharedMaterials(this Transform t)
        {
            var renderer = t.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.sharedMaterials;
            }

            return new Material[] { };
        }

        public static bool Has<T>(this Transform transform, T t) where T : Component
        {
            return transform.GetComponent<T>() == t;
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            return go.AddComponent<T>();
        }

        public static bool EnableForExport(this Component mono)
        {
            if (mono.transform.Ancestors().Any(x => !x.gameObject.activeSelf))
            {
                // 自分か祖先に !activeSelf がいる
                return false;
            }
            return true;
        }
    }
}
