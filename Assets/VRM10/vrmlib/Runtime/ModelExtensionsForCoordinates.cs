using System;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelExtensionsForCoordinates
    {
        /// <summary>
        /// ignoreVrm: VRM-0.XX では無変換で入出力してた。VRM-1.0 では変換する。
        /// </summary>
        public static void ConvertCoordinate(this Model model, Coordinates coordinates, bool ignoreVrm = false)
        {
            if (model.Coordinates.Equals(coordinates))
            {
                return;
            }

            if (model.Coordinates.IsGltf && coordinates.IsUnity)
            {
                model.ReverseZAndFlipTriangle(ignoreVrm);
                model.UVVerticalFlip();
            }
            else if (model.Coordinates.IsUnity && coordinates.IsGltf)
            {
                model.ReverseZAndFlipTriangle(ignoreVrm);
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
        static void ReverseZAndFlipTriangle(this Model model, bool ignoreVrm)
        {
            foreach (var g in model.MeshGroups)
            {
                foreach (var m in g.Meshes)
                {
                    foreach (var (k, v) in m.VertexBuffer)
                    {
                        if (k == VertexBuffer.PositionKey || k == VertexBuffer.NormalKey)
                        {
                            ReverseZ(v);
                        }
                        if (k == VertexBuffer.TangentKey)
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
                                ReverseZ(v);
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
                n.SetMatrix(n.Matrix.ReverseZ(), false);
            }
            // 親から順に処理したので不要
            // model.Root.CalcWorldMatrix();

            foreach (var s in model.Skins)
            {
                if (s.InverseMatrices != null)
                {
                    ReverseZ(s.InverseMatrices);
                }
            }

            foreach (var a in model.Animations)
            {
                // TODO:
            }

            if (model.Vrm != null)
            {
                if (!ignoreVrm)
                {
                    // LookAt
                    if (model.Vrm.LookAt != null)
                    {
                        model.Vrm.LookAt.OffsetFromHeadBone = model.Vrm.LookAt.OffsetFromHeadBone.ReverseZ();
                    }

                    // SpringBone
                    if (model.Vrm.SpringBone != null)
                    {
                        foreach (var b in model.Vrm.SpringBone.Springs)
                        {
                            foreach (var c in b.Colliders)
                            {
                                for (int i = 0; i < c.Colliders.Count; ++i)
                                {
                                    var s = c.Colliders[i];
                                    switch (s.ColliderType)
                                    {
                                        case VrmSpringBoneColliderTypes.Sphere:
                                            c.Colliders[i] = VrmSpringBoneCollider.CreateSphere(s.Offset.ReverseZ(), s.Radius);
                                            break;

                                        case VrmSpringBoneColliderTypes.Capsule:
                                            c.Colliders[i] = VrmSpringBoneCollider.CreateCapsule(s.Offset.ReverseZ(), s.Radius, s.CapsuleTail.ReverseZ());
                                            break;

                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                            }

                            foreach (var j in b.Joints)
                            {
                                j.GravityDir = j.GravityDir.ReverseZ();
                            }
                        }
                    }
                }
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
