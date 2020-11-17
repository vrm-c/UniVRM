using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


namespace UniGLTF
{
    public class MeshImporter
    {
        const float FRAME_WEIGHT = 100.0f;

        public class MeshContext
        {
            [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct Float4
            {
                public float x;
                public float y;
                public float z;
                public float w;

                public Float4 One()
                {
                    var sum = x + y + z + w;
                    var f = 1.0f / sum;
                    return new Float4
                    {
                        x = x * f,
                        y = y * f,
                        z = z * f,
                        w = w * f,
                    };
                }
            }

            string m_name;
            public string name => m_name;

            readonly List<Vector3> m_positions = new List<Vector3>();
            public List<Vector3> Positions => m_positions;

            readonly List<Vector3> m_normals = new List<Vector3>();
            public List<Vector3> Normals => m_normals;

            [Obsolete]
            readonly List<Vector4> m_tangents = new List<Vector4>();
            [Obsolete]
            public List<Vector4> Tangetns => m_tangents;

            readonly List<Vector2> m_uv = new List<Vector2>();
            public List<Vector2> UV => m_uv;

            readonly List<Vector2> m_uv2 = new List<Vector2>();
            public List<Vector2> UV2 => m_uv2;

            readonly List<Color> m_colors = new List<Color>();
            public List<Color> Colors => m_colors;

            readonly List<BoneWeight> m_boneWeights = new List<BoneWeight>();
            public List<BoneWeight> BoneWeights => m_boneWeights;

            readonly List<int[]> m_subMeshes = new List<int[]>();
            public List<int[]> SubMeshes => m_subMeshes;

            readonly List<int> m_materialIndices = new List<int>();
            public List<int> MaterialIndices => m_materialIndices;

            readonly List<BlendShape> m_blendShapes = new List<BlendShape>();
            public List<BlendShape> BlendShapes => m_blendShapes;

            public MeshContext(string name, int meshIndex)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = string.Format("UniGLTF import#{0}", meshIndex);
                }
                m_name = name;
            }

            void FillZero<T>(IList<T> list)
            {
                if (list.Count != m_positions.Count)
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// 各 primitive の attribute の要素が同じでない。=> uv が有るものと無いものが混在するなど
            /// glTF 的にはありうる。
            /// 
            /// primitive を独立した(Independent) Mesh として扱いこれを連結する。
            /// </summary>
            /// <param name="ctx"></param>
            /// <param name="gltfMesh"></param>
            /// <returns></returns>
            public void ImportMeshIndependentVertexBuffer(ImporterContext ctx, glTFMesh gltfMesh)
            {
                foreach (var prim in gltfMesh.primitives)
                {
                    var indexOffset = m_positions.Count;
                    var indexBuffer = prim.indices;

                    // position は必ずある
                    var positionCount = m_positions.Count;
                    m_positions.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).Select(x => x.ReverseZ()));
                    positionCount = m_positions.Count - positionCount;

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        FillZero(m_normals);
                        m_normals.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).Select(x => x.ReverseZ()));
                    }

#if false
                    if (prim.attributes.TANGENT != -1)
                    {
                        FillZero(tangetns);
                        tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).Select(x => x.ReverseZ()));
                    }
#endif

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        FillZero(m_uv);
                        if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            m_uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            m_uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseUV()));
                        }
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        FillZero(m_uv2);
                        m_uv2.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).Select(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        FillZero(m_colors);
                        m_colors.AddRange(ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
                        FillZero(m_boneWeights);

                        var joints0 = ctx.GLTF.GetArrayFromAccessor<UShort4>(prim.attributes.JOINTS_0); // uint4
                        var weights0 = ctx.GLTF.GetArrayFromAccessor<Float4>(prim.attributes.WEIGHTS_0).Select(x => x.One()).ToArray();

                        for (int j = 0; j < joints0.Length; ++j)
                        {
                            var bw = new BoneWeight();

                            bw.boneIndex0 = joints0[j].x;
                            bw.weight0 = weights0[j].x;

                            bw.boneIndex1 = joints0[j].y;
                            bw.weight1 = weights0[j].y;

                            bw.boneIndex2 = joints0[j].z;
                            bw.weight2 = weights0[j].z;

                            bw.boneIndex3 = joints0[j].w;
                            bw.weight3 = weights0[j].w;

                            m_boneWeights.Add(bw);
                        }
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            var primTarget = prim.targets[i];
                            var blendShape = new BlendShape(i.ToString());
                            if (primTarget.POSITION != -1)
                            {
                                FillZero(blendShape.Positions);
                                blendShape.Positions.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                FillZero(blendShape.Normals);
                                blendShape.Normals.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                FillZero(blendShape.Tangents);
                                blendShape.Tangents.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT).Select(x => x.ReverseZ()).ToArray());
                            }
                            m_blendShapes.Add(blendShape);
                        }
                    }

                    var indices =
                     (indexBuffer >= 0)
                     ? ctx.GLTF.GetIndices(indexBuffer)
                     : TriangleUtil.FlipTriangle(Enumerable.Range(0, m_positions.Count)).ToArray() // without index array
                     ;
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        indices[i] += indexOffset;
                    }

                    m_subMeshes.Add(indices);

                    // material
                    m_materialIndices.Add(prim.material);
                }
            }

            /// <summary>
            /// 
            /// 各primitiveが同じ attribute を共有している場合専用のローダー。
            ///
            /// </summary>
            /// <param name="ctx"></param>
            /// <param name="gltfMesh"></param>
            /// <returns></returns>
            public void ImportMeshSharingVertexBuffer(ImporterContext ctx, glTFMesh gltfMesh)
            {
                {
                    //  同じVertexBufferを共有しているので先頭のモノを使う
                    var prim = gltfMesh.primitives.First();
                    m_positions.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).SelectInplace(x => x.ReverseZ()));

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        m_normals.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).SelectInplace(x => x.ReverseZ()));
                    }

#if false
                    // tangent
                    if (prim.attributes.TANGENT != -1)
                    {
                        tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).SelectInplace(x => x.ReverseZ()));
                    }
#endif

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            m_uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            m_uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseUV()));
                        }
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        m_uv2.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).SelectInplace(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 3)
                        {
                            var vec3Color = ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.COLOR_0);
                            m_colors.AddRange(new Color[vec3Color.Length]);

                            for (int i = 0; i < vec3Color.Length; i++)
                            {
                                Vector3 color = vec3Color[i];
                                m_colors[i] = new Color(color.x, color.y, color.z);
                            }
                        }
                        else if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 4)
                        {
                            m_colors.AddRange(ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                        }
                        else
                        {
                            throw new NotImplementedException(string.Format("unknown color type {0}", ctx.GLTF.accessors[prim.attributes.COLOR_0].type));
                        }
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
                        var joints0 = ctx.GLTF.GetArrayFromAccessor<UShort4>(prim.attributes.JOINTS_0); // uint4
                        var weights0 = ctx.GLTF.GetArrayFromAccessor<Float4>(prim.attributes.WEIGHTS_0);
                        for (int i = 0; i < weights0.Length; ++i)
                        {
                            weights0[i] = weights0[i].One();
                        }

                        for (int j = 0; j < joints0.Length; ++j)
                        {
                            var bw = new BoneWeight();

                            bw.boneIndex0 = joints0[j].x;
                            bw.weight0 = weights0[j].x;

                            bw.boneIndex1 = joints0[j].y;
                            bw.weight1 = weights0[j].y;

                            bw.boneIndex2 = joints0[j].z;
                            bw.weight2 = weights0[j].z;

                            bw.boneIndex3 = joints0[j].w;
                            bw.weight3 = weights0[j].w;

                            m_boneWeights.Add(bw);
                        }
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        m_blendShapes.AddRange(prim.targets.Select((x, i) => new BlendShape(i.ToString())));
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            //var name = string.Format("target{0}", i++);
                            var primTarget = prim.targets[i];
                            var blendShape = m_blendShapes[i];

                            if (primTarget.POSITION != -1)
                            {
                                blendShape.Positions.Assign(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION), x => x.ReverseZ());
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                blendShape.Normals.Assign(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL), x => x.ReverseZ());
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                blendShape.Tangents.Assign(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT), x => x.ReverseZ());
                            }
                        }
                    }
                }

                foreach (var prim in gltfMesh.primitives)
                {
                    if (prim.indices == -1)
                    {
                        m_subMeshes.Add(TriangleUtil.FlipTriangle(Enumerable.Range(0, m_positions.Count)).ToArray());
                    }
                    else
                    {
                        var indices = ctx.GLTF.GetIndices(prim.indices);
                        m_subMeshes.Add(indices);
                    }

                    // material
                    m_materialIndices.Add(prim.material);
                }
            }

            public void RenameBlendShape(glTFMesh gltfMesh)
            {
                if (gltfMesh.extras != null && gltfMesh.extras.targetNames != null)
                {
                    var targetNames = gltfMesh.extras.targetNames;
                    for (int i = 1; i < gltfMesh.primitives.Count; ++i)
                    {
                        if (gltfMesh.primitives[i].targets.Count != targetNames.Count)
                        {
                            throw new FormatException(string.Format("different targets length: {0} with targetNames length.",
                                gltfMesh.primitives[i]));
                        }
                    }
                    for (var i = 0; i < targetNames.Count; i++)
                    {
                        BlendShapes[i].Name = targetNames[i];
                    }
                    return;
                }

                var prim = gltfMesh.primitives[0];
                {
                    if (prim.extras != null && prim.extras.targetNames != null)
                    {
                        var targetNames = prim.extras.targetNames;
                        for (var i = 0; i < BlendShapes.Count; i++)
                        {
                            if (i >= targetNames.Count)
                            {
                                Debug.LogWarning($"invalid primitive.extras.targetNames length");
                                break;
                            }
                            BlendShapes[i].Name = targetNames[i];
                        }
                    }
                }
            }

            static void Truncate<T>(List<T> list, int maxIndex)
            {
                if (list == null)
                {
                    return;
                }
                var count = maxIndex + 1;
                if (list.Count > count)
                {
                    // Debug.LogWarning($"remove {count} to {list.Count}");
                    list.RemoveRange(count, list.Count - count);
                }
            }

            //
            // https://github.com/vrm-c/UniVRM/issues/610
            //
            // VertexBuffer の後ろに未使用頂点がある場合に削除する
            //
            public void DropUnusedVertices()
            {
                var maxIndex = m_subMeshes.SelectMany(x => x).Max();
                Truncate(m_positions, maxIndex);
                Truncate(m_normals, maxIndex);
                Truncate(m_uv, maxIndex);
                Truncate(m_uv2, maxIndex);
                Truncate(m_colors, maxIndex);
                Truncate(m_boneWeights, maxIndex);
#if false                
                Truncate(m_tangents, maxIndex);
#endif
                foreach (var blendshape in m_blendShapes)
                {
                    Truncate(blendshape.Positions, maxIndex);
                    Truncate(blendshape.Normals, maxIndex);
                    Truncate(blendshape.Tangents, maxIndex);
                }
            }
        }

        static bool HasSharedVertexBuffer(glTFMesh gltfMesh)
        {
            glTFAttributes lastAttributes = null;
            var sharedAttributes = true;
            foreach (var prim in gltfMesh.primitives)
            {
                if (lastAttributes != null && !prim.attributes.Equals(lastAttributes))
                {
                    sharedAttributes = false;
                    break;
                }

                lastAttributes = prim.attributes;
            }
            return sharedAttributes;
        }

        public MeshContext ReadMesh(ImporterContext ctx, int meshIndex)
        {
            var gltfMesh = ctx.GLTF.meshes[meshIndex];

            var meshContext = new MeshContext(gltfMesh.name, meshIndex);
            if (HasSharedVertexBuffer(gltfMesh))
            {
                meshContext.ImportMeshSharingVertexBuffer(ctx, gltfMesh);
            }
            else
            {
                meshContext.ImportMeshIndependentVertexBuffer(ctx, gltfMesh);
            }

            meshContext.RenameBlendShape(gltfMesh);

            meshContext.DropUnusedVertices();

            return meshContext;
        }

        static (Mesh, bool) _BuildMesh(MeshImporter.MeshContext meshContext)
        {
            if (!meshContext.MaterialIndices.Any())
            {
                // add default material
                meshContext.MaterialIndices.Add(0);
            }

            //Debug.Log(prims.ToJson());
            var mesh = new Mesh();
            mesh.name = meshContext.name;

            if (meshContext.Positions.Count > UInt16.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = meshContext.Positions.ToArray();
            bool recalculateNormals = false;
            if (meshContext.Normals != null && meshContext.Normals.Count > 0)
            {
                mesh.normals = meshContext.Normals.ToArray();
            }
            else
            {
                recalculateNormals = true;
            }

            if (meshContext.UV.Count == mesh.vertexCount)
            {
                mesh.uv = meshContext.UV.ToArray();
            }
            if (meshContext.UV2.Count == mesh.vertexCount)
            {
                mesh.uv2 = meshContext.UV2.ToArray();
            }

            bool recalculateTangents = true;
#if UNIGLTF_IMPORT_TANGENTS
            if (meshContext.Tangents.Length > 0)
            {
                mesh.tangents = meshContext.Tangents.ToArray();
                recalculateTangents = false;
            }
#endif

            if (meshContext.Colors.Count == mesh.vertexCount)
            {
                mesh.colors = meshContext.Colors.ToArray();
            }
            if (meshContext.BoneWeights.Count > 0)
            {
                mesh.boneWeights = meshContext.BoneWeights.ToArray();
            }
            mesh.subMeshCount = meshContext.SubMeshes.Count;
            for (int i = 0; i < meshContext.SubMeshes.Count; ++i)
            {
                mesh.SetTriangles(meshContext.SubMeshes[i], i);
            }

            if (recalculateNormals)
            {
                mesh.RecalculateNormals();
            }

            return (mesh, recalculateTangents);
        }

        static void BuildBlendShape(Mesh mesh, MeshContext meshContext, BlendShape blendShape, Vector3[] emptyVertices)
        {
            if (blendShape.Positions.Count > 0)
            {
                if (blendShape.Positions.Count == mesh.vertexCount)
                {
                    mesh.AddBlendShapeFrame(blendShape.Name, FRAME_WEIGHT,
                        blendShape.Positions.ToArray(),
                        (meshContext.Normals.Count == mesh.vertexCount && blendShape.Normals.Count == blendShape.Positions.Count()) ? blendShape.Normals.ToArray() : null,
                        null
                        );
                }
                else
                {
                    Debug.LogWarningFormat("May be partial primitive has blendShape. Require separate mesh or extend blend shape, but not implemented: {0}", blendShape.Name);
                }
            }
            else
            {
                // Debug.LogFormat("empty blendshape: {0}.{1}", mesh.name, blendShape.Name);
                // add empty blend shape for keep blend shape index
                mesh.AddBlendShapeFrame(blendShape.Name, FRAME_WEIGHT,
                    emptyVertices,
                    null,
                    null
                    );
            }
        }

        public static MeshWithMaterials BuildMesh(ImporterContext ctx, MeshImporter.MeshContext meshContext)
        {
            var (mesh, recalculateTangents) = _BuildMesh(meshContext);

            if (recalculateTangents)
            {
                mesh.RecalculateTangents();
            }

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.MaterialIndices.Select(x => ctx.GetMaterial(x)).ToArray()
            };

            if (meshContext.BlendShapes.Count > 0)
            {
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in meshContext.BlendShapes)
                {
                    BuildBlendShape(mesh, meshContext, blendShape, emptyVertices);
                }
            }
            return result;
        }

        public static IEnumerator BuildMeshCoroutine(ImporterContext ctx, MeshImporter.MeshContext meshContext)
        {
            var (mesh, recalculateTangents) = _BuildMesh(meshContext);

            if (recalculateTangents)
            {
                yield return null;
                mesh.RecalculateTangents();
                yield return null;
            }

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.MaterialIndices.Select(x => ctx.GetMaterial(x)).ToArray()
            };

            yield return null;
            if (meshContext.BlendShapes.Count > 0)
            {
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in meshContext.BlendShapes)
                {
                    BuildBlendShape(mesh, meshContext, blendShape, emptyVertices);
                }
            }

            yield return result;
        }
    }
}
