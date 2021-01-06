using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace VrmLib
{
    // Bone skinning
    public class Skin : GltfId
    {
        public BufferAccessor InverseMatrices;

        public Node Root;

        public List<Node> Joints = new List<Node>();

        Matrix4x4[] m_matrices;
        public Matrix4x4[] SkinningMatrices => m_matrices;

        ushort m_indexOfRoot = ushort.MaxValue;

        public Skin()
        {
        }

        /// <summary>
        /// BoneSkinningもしくはMorphTargetの適用
        /// <summary>
        public void Skinning(VertexBuffer vertexBuffer = null)
        {
            m_indexOfRoot = (ushort)Joints.IndexOf(Root);
            var addRoot = Root != null && m_indexOfRoot == ushort.MaxValue;
            if (addRoot)
            {
                m_indexOfRoot = (ushort)Joints.Count;
                Joints.Add(Root);
            }

            if (m_matrices == null)
            {
                m_matrices = new Matrix4x4[Joints.Count];
            }

            if (InverseMatrices == null)
            {
                CalcInverseMatrices();
            }
            else
            {
                if (addRoot)
                {
                    var inverseArray = SpanLike.Wrap<Matrix4x4>(InverseMatrices.Bytes).ToArray();
                    var concat = inverseArray.Concat(new[] { Root.InverseMatrix }).ToArray();
                    InverseMatrices.Assign(concat);
                }
            }

            var inverse = InverseMatrices.GetSpan<Matrix4x4>();

            // if (Root != null)
            // {
            //     var rootInverse = Root.InverseMatrix;
            //     var root = Root.Matrix;
            //     for (int i = 0; i < m_matrices.Length; ++i)
            //     {
            //         m_matrices[i] = inverse[i] * Joints[i].Matrix * rootInverse;
            //     }
            // }
            // else
            {
                for (int i = 0; i < m_matrices.Length; ++i)
                {
                    var inv = i < inverse.Length ? inverse[i] : Joints[i].InverseMatrix;
                    m_matrices[i] = inv * Joints[i].Matrix;
                }
            }

            if (vertexBuffer != null)
            {
                Apply(vertexBuffer);
            }
        }

        void Apply(VertexBuffer vertexBuffer)
        {
            var dstPosition = SpanLike.Wrap<Vector3>(vertexBuffer.Positions.Bytes);
            // Span<Vector3> emptyNormal = stackalloc Vector3[0];
            Apply(vertexBuffer, dstPosition, vertexBuffer.Normals != null ? SpanLike.Wrap<Vector3>(vertexBuffer.Normals.Bytes) : default);
        }

        public void Apply(VertexBuffer vertexBuffer, SpanLike<Vector3> dstPosition, SpanLike<Vector3> dstNormal)
        {
            var jointsBuffer = vertexBuffer.Joints;
            var joints = (jointsBuffer != null || jointsBuffer.Count == 0)
                ? SpanLike.Wrap<SkinJoints>(jointsBuffer.Bytes)
                : SpanLike.Create<SkinJoints>(vertexBuffer.Count) // when MorphTarget only
                ;

            var weightsBuffer = vertexBuffer.Weights;
            var weights = (weightsBuffer != null || weightsBuffer.Count == 0)
                ? SpanLike.Wrap<Vector4>(weightsBuffer.Bytes)
                : SpanLike.Create<Vector4>(vertexBuffer.Count) // when MorphTarget only
                ;

            var positionBuffer = vertexBuffer.Positions;
            var position = SpanLike.Wrap<Vector3>(positionBuffer.Bytes);

            bool useNormal = false;
            if (dstNormal.Length > 0)
            {
                useNormal = vertexBuffer.Normals != null && dstNormal.Length == dstPosition.Length;
            }

            for (int i = 0; i < position.Length; ++i)
            {
                var j = joints[i];
                var w = weights[i];

                var sum = (w.X + w.Y + w.Z + w.W);
                float factor;
                if (sum > 0)
                {
                    factor = 1.0f / sum;
                }
                else
                {
                    factor = 1.0f;
                    j = new SkinJoints(m_indexOfRoot, 0, 0, 0);
                    w = new Vector4(1, 0, 0, 0);
                }
                if (j.Joint0 == ushort.MaxValue) w.X = 0;
                if (j.Joint1 == ushort.MaxValue) w.Y = 0;
                if (j.Joint2 == ushort.MaxValue) w.Z = 0;
                if (j.Joint3 == ushort.MaxValue) w.W = 0;

                {
                    var src = new Vector4(position[i], 1); // 位置ベクトル
                    var dst = Vector4.Zero;
                    if (w.X > 0) dst += Vector4.Transform(src, m_matrices[j.Joint0]) * w.X * factor;
                    if (w.Y > 0) dst += Vector4.Transform(src, m_matrices[j.Joint1]) * w.Y * factor;
                    if (w.Z > 0) dst += Vector4.Transform(src, m_matrices[j.Joint2]) * w.Z * factor;
                    if (w.W > 0) dst += Vector4.Transform(src, m_matrices[j.Joint3]) * w.W * factor;
                    dstPosition[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
                if (useNormal)
                {
                    var normalBuffer = vertexBuffer.Normals;
                    var normal = normalBuffer != null ? SpanLike.Wrap<Vector3>(normalBuffer.Bytes) : dstNormal;
                    var src = new Vector4(normal[i], 0); // 方向ベクトル
                    var dst = Vector4.Zero;
                    if (w.X > 0) dst += Vector4.Transform(src, m_matrices[j.Joint0]) * w.X * factor;
                    if (w.Y > 0) dst += Vector4.Transform(src, m_matrices[j.Joint1]) * w.Y * factor;
                    if (w.Z > 0) dst += Vector4.Transform(src, m_matrices[j.Joint2]) * w.Z * factor;
                    if (w.W > 0) dst += Vector4.Transform(src, m_matrices[j.Joint3]) * w.W * factor;
                    dstNormal[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
            }
        }

        // だいたい Identity
        static bool IsIdentity(Matrix4x4 m)
        {
            // 回転・スケール・しあー
            if (
                m.M11 == 1 && m.M12 == 0 && m.M13 == 0 && m.M14 == 0
                && m.M21 == 0 && m.M22 == 1 && m.M23 == 0 && m.M24 == 0
                && m.M31 == 0 && m.M32 == 0 && m.M33 == 1 && m.M34 == 0
                && m.M44 == 1
            )
            {

            }
            else
            {
                return false;
            }

            if (Math.Abs(m.M41) > 1e-5f) return false;
            if (Math.Abs(m.M42) > 1e-5f) return false;
            if (Math.Abs(m.M43) > 1e-5f) return false;

            return true;
        }

        public override string ToString()
        {
            if (InverseMatrices != null)
            {
                var sb = new StringBuilder();
                var matrices = SpanLike.Wrap<Matrix4x4>(InverseMatrices.Bytes);
                var count = 0;
                // var rootMatrix = Matrix4x4.Identity;
                // if (Root != null)
                // {
                //     rootMatrix = Root.InverseMatrix;
                // }
                for (int i = 0; i < matrices.Length; ++i)
                {
                    var m = matrices[i] * Joints[i].Matrix;
                    if (!IsIdentity(m))
                    {
                        ++count;
                    }
                }
                if (count > 0)
                {
                    sb.Append($"{count}/{Joints.Count} is not normalized");
                }
                else
                {
                    sb.Append($"{Joints.Count} joints normalized");
                }
                return sb.ToString();
            }
            else
            {
                return $"{Joints.Count} joints without InverseMatrices";
            }
        }

        public void Replace(Node src, Node dst)
        {
            var removeIndex = Joints.IndexOf(src);
            if (removeIndex >= 0)
            {
                Joints[removeIndex] = dst;

                // エクスポート時に再計算させる
                CalcInverseMatrices();
            }
        }

        public void CalcInverseMatrices()
        {
            // var root = Root;
            // if (root == null)
            // {
            //     root = Joints[0].Ancestors().Last();
            // }
            // root.CalcWorldMatrix(Matrix4x4.Identity, true);

            // calc inverse bind matrices
            var matricesBytes = new Byte[Marshal.SizeOf(typeof(Matrix4x4)) * Joints.Count];
            var matrices = SpanLike.Wrap<Matrix4x4>(new ArraySegment<byte>(matricesBytes));
            for (int i = 0; i < Joints.Count; ++i)
            {
                // var w = Joints[i].Matrix;
                // Matrix4x4.Invert(w, out Matrix4x4 inv);
                if (Joints[i] != null)
                {
                    matrices[i] = Joints[i].InverseMatrix;
                }
            }
            InverseMatrices = new BufferAccessor(new ArraySegment<byte>(matricesBytes), AccessorValueType.FLOAT, AccessorVectorType.MAT4, Joints.Count);
        }

        static void Update(ref float weight, ref ushort index, int[] indexMap)
        {
            if (indexMap[index] == -1)
            {
                if (weight > 0)
                {
                    throw new Exception();
                }
                //削除された
                weight = 0;
                index = 0;
            }
            else
            {
                // 参照を更新(変わっているかもしれない)
                index = (ushort)indexMap[index];
            }
        }

        /// <summary>
        /// nullになったjointを除去して、boneweightを前に詰める
        /// </summary>
        public void FixBoneWeight(BufferAccessor jointsAccessor, BufferAccessor weightsAccessor)
        {
            var map = Joints.Select((x, i) => ValueTuple.Create(i, x)).Where(x => x.Item2 != null).ToArray();
            var indexMap = Enumerable.Repeat(-1, Joints.Count).ToArray();
            {
                for (int i = 0; i < map.Length; ++i)
                {
                    indexMap[map[i].Item1] = i;
                }
            }
            Joints.RemoveAll(x => x == null);

            var joints = jointsAccessor.GetSpan<SkinJoints>();
            var weights = weightsAccessor.GetSpan<Vector4>();
            for (int i = 0; i < joints.Length; ++i)
            {
                var j = joints[i];
                var w = weights[i];

                Update(ref w.X, ref j.Joint0, indexMap);
                Update(ref w.Y, ref j.Joint1, indexMap);
                Update(ref w.Z, ref j.Joint2, indexMap);
                Update(ref w.W, ref j.Joint3, indexMap);

                joints[i] = j;
                weights[i] = w;
            }

            CalcInverseMatrices();
        }
    }
}