using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;

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
            BlendShape GetOrCreateBlendShape(int i)
            {
                if (i < m_blendShapes.Count && m_blendShapes[i] != null)
                {
                    return m_blendShapes[i];
                }

                while (m_blendShapes.Count <= i)
                {
                    m_blendShapes.Add(null);
                }

                var blendShape = new BlendShape(i.ToString());
                m_blendShapes[i] = blendShape;
                return blendShape;
            }

            public MeshContext(string name, int meshIndex)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = string.Format("UniGLTF import#{0}", meshIndex);
                }
                m_name = name;
            }

            /// <summary>
            /// Fill list with 0s with the specified length
            /// </summary>
            /// <param name="list"></param>
            /// <param name="fillLength"></param>
            /// <typeparam name="T"></typeparam>
            static void FillZero<T>(IList<T> list, int fillLength)
            {
                if (list.Count > fillLength)
                {
                    throw new Exception("Impossible");
                }
                while (list.Count < fillLength)
                {
                    list.Add(default);
                }
            }

            public static BoneWeight NormalizeBoneWeight(BoneWeight src)
            {
                var sum = src.weight0 + src.weight1 + src.weight2 + src.weight3;
                if (sum == 0)
                {
                    return src;
                }
                var f = 1.0f / sum;
                src.weight0 *= f;
                src.weight1 *= f;
                src.weight2 *= f;
                src.weight3 *= f;
                return src;
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
            public void ImportMeshIndependentVertexBuffer(glTF gltf, glTFMesh gltfMesh, IAxisInverter inverter)
            {
                foreach (var prim in gltfMesh.primitives)
                {
                    var indexOffset = m_positions.Count;
                    var indexBuffer = prim.indices;

                    // position は必ずある
                    var positions = gltf.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION);
                    m_positions.AddRange(positions.Select(inverter.InvertVector3));
                    var fillLength = m_positions.Count;

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        var normals = gltf.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL);
                        if (normals.Length != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        m_normals.AddRange(normals.Select(inverter.InvertVector3));
                        FillZero(m_normals, fillLength);
                    }

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        var uvs = gltf.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0);
                        if (uvs.Length != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        if (gltf.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            m_uv.AddRange(uvs.Select(x => x.ReverseY()));
                            FillZero(m_uv, fillLength);
#pragma warning restore 0612
                        }
                        else
                        {
                            m_uv.AddRange(uvs.Select(x => x.ReverseUV()));
                            FillZero(m_uv, fillLength);
                        }
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        var uvs = gltf.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1);
                        if (uvs.Length != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        m_uv2.AddRange(uvs.Select(x => x.ReverseUV()));
                        FillZero(m_uv2, fillLength);
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        var colors = gltf.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0);
                        if (colors.Length != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        m_colors.AddRange(colors);
                        FillZero(m_colors, fillLength);
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
                        var (joints0, jointsLength) = JointsAccessor.GetAccessor(gltf, prim.attributes.JOINTS_0);
                        var (weights0, weightsLength) = WeightsAccessor.GetAccessor(gltf, prim.attributes.WEIGHTS_0);
                        if (jointsLength != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        if (weightsLength != positions.Length)
                        {
                            throw new Exception("different length");
                        }
                        for (int j = 0; j < jointsLength; ++j)
                        {
                            var bw = new BoneWeight();

                            var joints = joints0(j);
                            var weights = weights0(j);

                            bw.boneIndex0 = joints.x;
                            bw.weight0 = weights.x;

                            bw.boneIndex1 = joints.y;
                            bw.weight1 = weights.y;

                            bw.boneIndex2 = joints.z;
                            bw.weight2 = weights.z;

                            bw.boneIndex3 = joints.w;
                            bw.weight3 = weights.w;

                            bw = NormalizeBoneWeight(bw);

                            m_boneWeights.Add(bw);
                        }
                        FillZero(m_boneWeights, fillLength);
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            var primTarget = prim.targets[i];
                            var blendShape = GetOrCreateBlendShape(i);
                            if (primTarget.POSITION != -1)
                            {
                                var array = gltf.GetArrayFromAccessor<Vector3>(primTarget.POSITION);
                                if (array.Length != positions.Length)
                                {
                                    throw new Exception("different length");
                                }
                                blendShape.Positions.AddRange(array.Select(inverter.InvertVector3).ToArray());
                                FillZero(blendShape.Positions, fillLength);
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                var array = gltf.GetArrayFromAccessor<Vector3>(primTarget.NORMAL);
                                if (array.Length != positions.Length)
                                {
                                    throw new Exception("different length");
                                }
                                blendShape.Normals.AddRange(array.Select(inverter.InvertVector3).ToArray());
                                FillZero(blendShape.Normals, fillLength);
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                var array = gltf.GetArrayFromAccessor<Vector3>(primTarget.TANGENT);
                                if (array.Length != positions.Length)
                                {
                                    throw new Exception("different length");
                                }
                                blendShape.Tangents.AddRange(array.Select(inverter.InvertVector3).ToArray());
                                FillZero(blendShape.Tangents, fillLength);
                            }
                        }
                    }

                    var indices =
                     (indexBuffer >= 0)
                     ? gltf.GetIndices(indexBuffer)
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
            public void ImportMeshSharingVertexBuffer(glTF gltf, glTFMesh gltfMesh, IAxisInverter inverter)
            {
                {
                    //  同じVertexBufferを共有しているので先頭のモノを使う
                    var prim = gltfMesh.primitives.First();
                    m_positions.AddRange(gltf.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).SelectInplace(inverter.InvertVector3));

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        m_normals.AddRange(gltf.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).SelectInplace(inverter.InvertVector3));
                    }

#if false
                    // tangent
                    if (prim.attributes.TANGENT != -1)
                    {
                        tangents.AddRange(gltf.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).SelectInplace(inverter.InvertVector4));
                    }
#endif

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        if (gltf.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            m_uv.AddRange(gltf.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            m_uv.AddRange(gltf.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseUV()));
                        }
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        m_uv2.AddRange(gltf.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).SelectInplace(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        if (gltf.accessors[prim.attributes.COLOR_0].TypeCount == 3)
                        {
                            var vec3Color = gltf.GetArrayFromAccessor<Vector3>(prim.attributes.COLOR_0);
                            m_colors.AddRange(new Color[vec3Color.Length]);

                            for (int i = 0; i < vec3Color.Length; i++)
                            {
                                Vector3 color = vec3Color[i];
                                m_colors[i] = new Color(color.x, color.y, color.z);
                            }
                        }
                        else if (gltf.accessors[prim.attributes.COLOR_0].TypeCount == 4)
                        {
                            m_colors.AddRange(gltf.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                        }
                        else
                        {
                            throw new NotImplementedException(string.Format("unknown color type {0}", gltf.accessors[prim.attributes.COLOR_0].type));
                        }
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
                        var (joints0, jointsLength) = JointsAccessor.GetAccessor(gltf, prim.attributes.JOINTS_0);
                        var (weights0, weightsLength) = WeightsAccessor.GetAccessor(gltf, prim.attributes.WEIGHTS_0);

                        for (int j = 0; j < jointsLength; ++j)
                        {
                            var bw = new BoneWeight();

                            var joints = joints0(j);
                            var weights = weights0(j);

                            bw.boneIndex0 = joints.x;
                            bw.weight0 = weights.x;

                            bw.boneIndex1 = joints.y;
                            bw.weight1 = weights.y;

                            bw.boneIndex2 = joints.z;
                            bw.weight2 = weights.z;

                            bw.boneIndex3 = joints.w;
                            bw.weight3 = weights.w;

                            bw = NormalizeBoneWeight(bw);

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
                                    gltf.GetArrayFromAccessor<Vector3>(primTarget.POSITION), inverter.InvertVector3);
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                blendShape.Normals.Assign(
                                    gltf.GetArrayFromAccessor<Vector3>(primTarget.NORMAL), inverter.InvertVector3);
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                blendShape.Tangents.Assign(
                                    gltf.GetArrayFromAccessor<Vector3>(primTarget.TANGENT), inverter.InvertVector3);
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
                        var indices = gltf.GetIndices(prim.indices);
                        m_subMeshes.Add(indices);
                    }

                    // material
                    m_materialIndices.Add(prim.material);
                }
            }

            public void RenameBlendShape(glTFMesh gltfMesh)
            {
                if (gltf_mesh_extras_targetNames.TryGet(gltfMesh, out List<string> targetNames))
                {
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

        public MeshContext ReadMesh(glTF gltf, int meshIndex, IAxisInverter inverter)
        {
            var gltfMesh = gltf.meshes[meshIndex];

            var meshContext = new MeshContext(gltfMesh.name, meshIndex);
            if (HasSharedVertexBuffer(gltfMesh))
            {
                meshContext.ImportMeshSharingVertexBuffer(gltf, gltfMesh, inverter);
            }
            else
            {
                meshContext.ImportMeshIndependentVertexBuffer(gltf, gltfMesh, inverter);
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

        public static async Task<MeshWithMaterials> BuildMeshAsync(IAwaitCaller awaitCaller, Func<int, Material> ctx, MeshImporter.MeshContext meshContext)
        {
            var (mesh, recalculateTangents) = _BuildMesh(meshContext);

            if (recalculateTangents)
            {
                await awaitCaller.NextFrame();
                mesh.RecalculateTangents();
                await awaitCaller.NextFrame();
            }

            // 先にすべてのマテリアルを作成済みなのでテクスチャーは生成済み。Resultを使ってよい
            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.MaterialIndices.Select(ctx).ToArray()
            };

            await awaitCaller.NextFrame();
            if (meshContext.BlendShapes.Count > 0)
            {
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in meshContext.BlendShapes)
                {
                    BuildBlendShape(mesh, meshContext, blendShape, emptyVertices);
                }
            }

            mesh.UploadMeshData(false);

            return result;
        }
    }
}
