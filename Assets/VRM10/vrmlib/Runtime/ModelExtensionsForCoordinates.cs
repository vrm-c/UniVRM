using System;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelExtensionsForCoordinates
    {
        static void ReverseX(BufferAccessor ba)
        {
            if (ba.ComponentType != AccessorValueType.FLOAT)
            {
                throw new Exception();
            }
            if (ba.AccessorType == AccessorVectorType.VEC3)
            {
                var span = SpanLike.Wrap<Vector3>(ba.Bytes);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseX();
                }
            }
            else if (ba.AccessorType == AccessorVectorType.MAT4)
            {
                var span = SpanLike.Wrap<Matrix4x4>(ba.Bytes);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseX();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static void ReverseZ(BufferAccessor ba)
        {
            if (ba.ComponentType != AccessorValueType.FLOAT)
            {
                throw new Exception();
            }
            if (ba.AccessorType == AccessorVectorType.VEC3)
            {
                var span = SpanLike.Wrap<Vector3>(ba.Bytes);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseZ();
                }
            }
            else if (ba.AccessorType == AccessorVectorType.MAT4)
            {
                var span = SpanLike.Wrap<Matrix4x4>(ba.Bytes);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseZ();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        struct Reverser
        {
            public Action<BufferAccessor> ReverseBuffer;
            public Func<Vector3, Vector3> ReverseVector3;
            public Func<Matrix4x4, Matrix4x4> ReverseMatrix;
        }

        static Reverser ZReverser => new Reverser
        {
            ReverseBuffer = ReverseZ,
            ReverseVector3 = v => v.ReverseZ(),
            ReverseMatrix = m => m.ReverseZ(),
        };

        static Reverser XReverser => new Reverser
        {
            ReverseBuffer = ReverseX,
            ReverseVector3 = v => v.ReverseX(),
            ReverseMatrix = m => m.ReverseX(),
        };

        /// <summary>
        /// ignoreVrm: VRM-0.XX では無変換で入出力してた。VRM-1.0 では変換する。
        /// </summary>
        public static void ConvertCoordinate(this Model model, Coordinates coordinates, bool ignoreVrm = false)
        {
            if (model.Coordinates.Equals(coordinates))
            {
                return;
            }

            if (model.Coordinates.IsVrm0 && coordinates.IsUnity)
            {
                model.ReverseAxisAndFlipTriangle(ZReverser, ignoreVrm);
                model.UVVerticalFlip();
            }
            else if (model.Coordinates.IsUnity && coordinates.IsVrm0)
            {
                model.ReverseAxisAndFlipTriangle(ZReverser, ignoreVrm);
                model.UVVerticalFlip();
            }
            else if (model.Coordinates.IsVrm1 && coordinates.IsUnity)
            {
                model.ReverseAxisAndFlipTriangle(XReverser, ignoreVrm);
                model.UVVerticalFlip();
            }
            else if (model.Coordinates.IsUnity && coordinates.IsVrm1)
            {
                model.ReverseAxisAndFlipTriangle(XReverser, ignoreVrm);
                model.UVVerticalFlip();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// UVのVを反転する。 => V = 1.0 - V
        /// </summary>
        static void UVVerticalFlip(this Model model)
        {
            foreach (var g in model.MeshGroups)
            {
                foreach (var m in g.Meshes)
                {
                    var uv = m.VertexBuffer.TexCoords;
                    if (uv != null)
                    {
                        var span = SpanLike.Wrap<Vector2>(uv.Bytes);
                        for (int i = 0; i < span.Length; ++i)
                        {
                            span[i] = span[i].UVVerticalFlip();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// * Position, Normal の Z座標に -1 を乗算する
        /// * Rotation => Axis Angle に分解 => Axis の Z座標に -1 を乗算。Angle に -1 を乗算
        /// * Triangle の index を 0, 1, 2 から 2, 1, 0 に反転する
        /// </summary>
        static void ReverseAxisAndFlipTriangle(this Model model, Reverser reverser, bool ignoreVrm)
        {
            foreach (var g in model.MeshGroups)
            {
                foreach (var m in g.Meshes)
                {
                    foreach (var (k, v) in m.VertexBuffer)
                    {
                        if (k == VertexBuffer.PositionKey || k == VertexBuffer.NormalKey)
                        {
                            reverser.ReverseBuffer(v);
                        }
                        else if (k == VertexBuffer.TangentKey)
                        {
                            // I don't know
                        }
                    }

                    switch (m.IndexBuffer.ComponentType)
                    {
                        case AccessorValueType.UNSIGNED_BYTE:
                            FlipTriangle(SpanLike.Wrap<Byte>(m.IndexBuffer.Bytes));
                            break;
                        case AccessorValueType.UNSIGNED_SHORT:
                            FlipTriangle(SpanLike.Wrap<UInt16>(m.IndexBuffer.Bytes));
                            break;
                        case AccessorValueType.UNSIGNED_INT:
                            FlipTriangle(SpanLike.Wrap<UInt32>(m.IndexBuffer.Bytes));
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    foreach (var mt in m.MorphTargets)
                    {
                        foreach (var (k, v) in mt.VertexBuffer)
                        {
                            if (k == VertexBuffer.PositionKey || k == VertexBuffer.NormalKey)
                            {
                                reverser.ReverseBuffer(v);
                            }
                            if (k == VertexBuffer.TangentKey)
                            {
                                // I don't know
                            }
                        }
                    }
                }
            }

            // 親から順に処理する
            // Rootは原点決め打ちのノード(GLTFに含まれない)
            foreach (var n in model.Root.Traverse().Skip(1))
            {
                n.SetMatrix(reverser.ReverseMatrix(n.Matrix), false);
            }
            // 親から順に処理したので不要
            // model.Root.CalcWorldMatrix();

            foreach (var s in model.Skins)
            {
                if (s.InverseMatrices != null)
                {
                    reverser.ReverseBuffer(s.InverseMatrices);
                }
            }

            foreach (var a in model.Animations)
            {
                // TODO:
            }
        }

        static void FlipTriangle(SpanLike<byte> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }

        static void FlipTriangle(SpanLike<ushort> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }

        static void FlipTriangle(SpanLike<uint> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }
    }
}
