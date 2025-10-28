using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UniGLTF;
using Unity.Collections;
using UnityEngine;

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
        public void Skinning(INativeArrayManager arrayManager, VertexBuffer vertexBuffer = null)
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
                CalcInverseMatrices(arrayManager);
            }
            else
            {
                if (addRoot)
                {
                    var inverseArray = InverseMatrices.Bytes.Reinterpret<Matrix4x4>(1);
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
                Apply(arrayManager, vertexBuffer);
            }
        }

        void Apply(INativeArrayManager arrayManager, VertexBuffer vertexBuffer)
        {
            var dstPosition = vertexBuffer.Positions.Bytes.Reinterpret<Vector3>(1);
            // Span<Vector3> emptyNormal = stackalloc Vector3[0];
            Apply(arrayManager, vertexBuffer, dstPosition, vertexBuffer.Normals != null ? vertexBuffer.Normals.Bytes.Reinterpret<Vector3>(1) : default);
        }

        public void Apply(INativeArrayManager arrayManager, VertexBuffer vertexBuffer, NativeArray<Vector3> dstPosition, NativeArray<Vector3> dstNormal)
        {
            var jointsBuffer = vertexBuffer.Joints;
            var joints = (jointsBuffer != null || jointsBuffer.Count == 0)
                ? jointsBuffer.GetAsSkinJointsArray()
                : arrayManager.CreateNativeArray<SkinJoints>(vertexBuffer.Count) // when MorphTarget only
                ;

            var weightsBuffer = vertexBuffer.Weights;
            var weights = (weightsBuffer != null || weightsBuffer.Count == 0)
                ? weightsBuffer.GetAsVector4Array()
                : arrayManager.CreateNativeArray<Vector4>(vertexBuffer.Count) // when MorphTarget only
                ;

            var positionBuffer = vertexBuffer.Positions;
            var position = positionBuffer.Bytes.Reinterpret<Vector3>(1);

            bool useNormal = false;
            if (dstNormal.Length > 0)
            {
                useNormal = vertexBuffer.Normals != null && dstNormal.Length == dstPosition.Length;
            }

            for (int i = 0; i < position.Length; ++i)
            {
                var j = joints[i];
                var w = weights[i];

                var sum = (w.x + w.y + w.z + w.w);
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
                if (j.Joint0 == ushort.MaxValue) w.x = 0;
                if (j.Joint1 == ushort.MaxValue) w.y = 0;
                if (j.Joint2 == ushort.MaxValue) w.z = 0;
                if (j.Joint3 == ushort.MaxValue) w.w = 0;

                {
                    var src = position[i]; // 位置ベクトル
                    var dst = Vector3.zero;
                    if (w.x > 0) dst += m_matrices[j.Joint0].MultiplyPoint(src) * w.x * factor;
                    if (w.y > 0) dst += m_matrices[j.Joint1].MultiplyPoint(src) * w.y * factor;
                    if (w.z > 0) dst += m_matrices[j.Joint2].MultiplyPoint(src) * w.z * factor;
                    if (w.w > 0) dst += m_matrices[j.Joint3].MultiplyPoint(src) * w.w * factor;
                    dstPosition[i] = new Vector3(dst.x, dst.y, dst.z);
                }
                if (useNormal)
                {
                    var normalBuffer = vertexBuffer.Normals;
                    var normal = normalBuffer != null ? normalBuffer.Bytes.Reinterpret<Vector3>(1) : dstNormal;
                    var src = normal[i]; // 方向ベクトル
                    var dst = Vector3.zero;
                    if (w.x > 0) dst += m_matrices[j.Joint0].MultiplyVector(src) * w.x * factor;
                    if (w.y > 0) dst += m_matrices[j.Joint1].MultiplyVector(src) * w.y * factor;
                    if (w.z > 0) dst += m_matrices[j.Joint2].MultiplyVector(src) * w.z * factor;
                    if (w.w > 0) dst += m_matrices[j.Joint3].MultiplyVector(src) * w.w * factor;
                    dstNormal[i] = new Vector3(dst.x, dst.y, dst.z);
                }
            }
        }

        // だいたい Identity
        static bool IsIdentity(Matrix4x4 m)
        {
            // 回転・スケール・しあー
            if (
                m.m00 == 1 && m.m10 == 0 && m.m20 == 0 && m.m30 == 0
                && m.m01 == 0 && m.m11 == 1 && m.m21 == 0 && m.m31 == 0
                && m.m02 == 0 && m.m12 == 0 && m.m22 == 1 && m.m32 == 0
                && m.m33 == 1
            )
            {

            }
            else
            {
                return false;
            }

            // translate
            if (Math.Abs(m.m03) > 1e-5f) return false;
            if (Math.Abs(m.m13) > 1e-5f) return false;
            if (Math.Abs(m.m23) > 1e-5f) return false;

            return true;
        }

        public override string ToString()
        {
            if (InverseMatrices != null)
            {
                var sb = new StringBuilder();
                var matrices = InverseMatrices.Bytes.Reinterpret<Matrix4x4>(1);
                var count = 0;
                // var rootMatrix = Matrix4x4.identity;
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

        public void Replace(INativeArrayManager arrayManager, Node src, Node dst)
        {
            var removeIndex = Joints.IndexOf(src);
            if (removeIndex >= 0)
            {
                Joints[removeIndex] = dst;

                // エクスポート時に再計算させる
                CalcInverseMatrices(arrayManager);
            }
        }

        public void CalcInverseMatrices(INativeArrayManager arrayManager)
        {
            // var root = Root;
            // if (root == null)
            // {
            //     root = Joints[0].Ancestors().Last();
            // }
            // root.CalcWorldMatrix(Matrix4x4.identity, true);

            // calc inverse bind matrices
            var matricesBytes = arrayManager.CreateNativeArray<Byte>(Marshal.SizeOf(typeof(Matrix4x4)) * Joints.Count);
            var matrices = matricesBytes.Reinterpret<Matrix4x4>(1);
            for (int i = 0; i < Joints.Count; ++i)
            {
                // var w = Joints[i].Matrix;
                // Matrix4x4.Invert(w, out Matrix4x4 inv);
                if (Joints[i] != null)
                {
                    matrices[i] = Joints[i].InverseMatrix;
                }
            }
            InverseMatrices = new BufferAccessor(arrayManager, matricesBytes, AccessorValueType.FLOAT, AccessorVectorType.MAT4, Joints.Count);
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
    }
}
