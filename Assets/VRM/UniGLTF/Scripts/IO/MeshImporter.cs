using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


namespace UniGLTF
{
    public class MeshImporter
    {
        const float FRAME_WEIGHT = 100.0f;

        private static IEnumerable<T> GetAttrBuffer<T>(VGltf.ResourcesStore store,
                                                       Dictionary<string, int> relation,
                                                       string name) where T : struct
        {
            int index;
            if (relation.TryGetValue(name, out index))
            {
                var buffer = store.GetOrLoadTypedBufferByAccessorIndex(index);
                return buffer.GetEntity<T>().GetEnumerable();
            }

            return null;
        }

        // multiple subMesh is not sharing a VertexBuffer.
        // each subMesh use a independent VertexBuffer.
        private static MeshContext _ImportMeshIndependentVertexBuffer(
            ImporterContext ctx,
            VGltf.Types.Mesh gltfMesh
            )
        {
            //Debug.LogWarning("_ImportMeshIndependentVertexBuffer");

            var targets = gltfMesh.Primitives[0].Targets;
            for (int i = 1; i < gltfMesh.Primitives.Count; ++i)
            {
                if (!gltfMesh.Primitives[i].Targets.SequenceEqual(targets))
                {
                    throw new NotImplementedException(string.Format("diffirent targets: {0} with {1}",
                        gltfMesh.Primitives[i],
                        targets));
                }
            }

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();

            var meshContext = new MeshContext();

            foreach (var prim in gltfMesh.Primitives)
            {
                var indexOffset = positions.Count;
                var indexBuffer = prim.Indices.Value;

                var positionCount = positions.Count;

                var position = GetAttrBuffer<Vector3>(ctx.Store, prim.Attributes, "POSITION");
                positions.AddRange(position.Select(x => x.ReverseZ()));

                positionCount = positions.Count - positionCount; // ???
                //positionCount = meshContext.positions.Count - positionCount; // ?

                // normal
                var normal = GetAttrBuffer<Vector3>(ctx.Store, prim.Attributes, "NORMAL");
                if (normal != null)
                {
                    normals.AddRange(normal.Select(x => x.ReverseZ()));
                }

                var tangent = GetAttrBuffer<Vector4>(ctx.Store, prim.Attributes, "TANGENT");
                if (tangent != null)
                {
                    tangents.AddRange(tangent.Select(x => x.ReverseZ()));
                }

                // uv
                var texcoord0 = GetAttrBuffer<Vector2>(ctx.Store, prim.Attributes, "TEXCOORD_0");
                if (texcoord0 != null)
                {
                    if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                    {
#pragma warning disable 0612
                        // backward compatibility
                        uv.AddRange(texcoord0.Select(x => x.ReverseY()));
#pragma warning restore 0612
                    }
                    else
                    {
                        uv.AddRange(texcoord0.Select(x => x.ReverseUV()));
                    }
                }
                else
                {
                    // for inconsistent attributes in primitives
                    uv.AddRange(new Vector2[positionCount]);
                }

                // color
                var color0 = GetAttrBuffer<Color>(ctx.Store, prim.Attributes, "COLOR_0");
                if (color0 != null)
                {
                    colors.AddRange(color0);
                }

                // skin
                var joints0 = GetAttrBuffer<UShort4>(ctx.Store, prim.Attributes, "JOINTS_0");
                var weights0 = GetAttrBuffer<Float4>(ctx.Store, prim.Attributes, "WEIGHTS_0");
                if (joints0 != null && weights0 != null)
                {
                    var joints0Arr = joints0.ToArray();
                    var weights0Arr = weights0.ToArray();

                    for (int j = 0; j < joints0Arr.Length; ++j)
                    {
                        var bw = new BoneWeight();

                        bw.boneIndex0 = joints0Arr[j].x;
                        bw.weight0 = weights0Arr[j].x;

                        bw.boneIndex1 = joints0Arr[j].y;
                        bw.weight1 = weights0Arr[j].y;

                        bw.boneIndex2 = joints0Arr[j].z;
                        bw.weight2 = weights0Arr[j].z;

                        bw.boneIndex3 = joints0Arr[j].w;
                        bw.weight3 = weights0Arr[j].w;

                        meshContext.boneWeights.Add(bw);
                    }
                }

                // blendshape
                if (prim.Targets != null && prim.Targets.Count > 0)
                {
                    var targetNames = prim.Extras as Dictionary<int, string>;
                    for (int i = 0; i < prim.Targets.Count; ++i)
                    {
                        //var name = string.Format("target{0}", i++);
                        var primTarget = prim.Targets[i];

                        string targetName;
                        if (targetNames == null || !targetNames.TryGetValue(i, out targetName))
                        {
                            targetName = i.ToString();
                        }
                        var blendShape = new BlendShape(targetName);

                        var targetPositions = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "POSITION");
                        if (targetPositions != null)
                        {
                            blendShape.Positions.AddRange(targetPositions.Select(x => x.ReverseZ()));
                        }

                        var targetNormals = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "NORMAL");
                        if (targetNormals != null)
                        {
                            blendShape.Normals.AddRange(targetNormals.Select(x => x.ReverseZ()));
                        }

                        var targetTangents = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "TANGENT");
                        if (targetTangents != null)
                        {
                            blendShape.Tangents.AddRange(targetTangents.Select(x => x.ReverseZ()));
                        }

                        meshContext.blendShapes.Add(blendShape);
                    }
                }

                var indices =
                    TriangleUtil.FlipTriangle(
                        indexBuffer >= 0
                        ? ctx.Store.GetOrLoadTypedBufferByAccessorIndex(indexBuffer).GetPrimitivesAsCasted<int>()
                        : Enumerable.Range(0, meshContext.positions.Length)
                        ).ToArray();
                for (int i = 0; i < indices.Length; ++i)
                {
                    indices[i] += indexOffset;
                }

                meshContext.subMeshes.Add(indices);

                // material
                if (prim.Material != null)
                {
                    meshContext.materialIndices.Add(prim.Material.Value);
                }
            }

            meshContext.positions = positions.ToArray();
            meshContext.normals = normals.ToArray();
            meshContext.tangents = tangents.ToArray();
            meshContext.uv = uv.ToArray();

            return meshContext;
        }

        // multiple submesh sharing same VertexBuffer
        private static MeshContext _ImportMeshSharingVertexBuffer(ImporterContext ctx,
                                                                  VGltf.Types.Mesh gltfMesh)
        {
            var context = new MeshContext();

            {
                var prim = gltfMesh.Primitives.First();

                var position = GetAttrBuffer<Vector3>(ctx.Store, prim.Attributes, "POSITION");
                context.positions = position.Select(x => x.ReverseZ()).ToArray();

                // normal
                var normal = GetAttrBuffer<Vector3>(ctx.Store, prim.Attributes, "NORMAL");
                if (normal != null)
                {
                    context.normals = normal.Select(x => x.ReverseZ()).ToArray();
                }

                // tangent
                var tangent = GetAttrBuffer<Vector4>(ctx.Store, prim.Attributes, "TANGENT");
                if (tangent != null)
                {
                    context.tangents = tangent.Select(x => x.ReverseZ()).ToArray();
                }

                // uv
                var texcoord0 = GetAttrBuffer<Vector2>(ctx.Store, prim.Attributes, "TEXCOORD_0");
                if (texcoord0 != null)
                {
                    if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                    {
#pragma warning disable 0612
                        // backward compatibility
                        context.uv = texcoord0.Select(x => x.ReverseY()).ToArray();
#pragma warning restore 0612
                    }
                    else
                    {
                        context.uv = texcoord0.Select(x => x.ReverseUV()).ToArray();
                    }
                }
                else
                {
                    // for inconsistent attributes in primitives
                    context.uv = new Vector2[context.positions.Length];
                }

                // color
                var color0 = GetAttrBuffer<Color>(ctx.Store, prim.Attributes, "COLOR_0");
                if (color0 != null)
                {
                    context.colors = color0.ToArray();
                }

                // skin
                var joints0 = GetAttrBuffer<UShort4>(ctx.Store, prim.Attributes, "JOINTS_0");
                var weights0 = GetAttrBuffer<Float4>(ctx.Store, prim.Attributes, "WEIGHTS_0"); // TODO: support normalized integers
                if (joints0 != null && weights0 != null)
                {
                    var joints0Arr = joints0.ToArray();
                    var weights0Arr = weights0.ToArray();

                    for (int i = 0; i < weights0Arr.Length; ++i)
                    {
                        weights0Arr[i] = weights0Arr[i].One();
                    }

                    for (int j = 0; j < joints0Arr.Length; ++j)
                    {
                        var bw = new BoneWeight();

                        bw.boneIndex0 = joints0Arr[j].x;
                        bw.weight0 = weights0Arr[j].x;

                        bw.boneIndex1 = joints0Arr[j].y;
                        bw.weight1 = weights0Arr[j].y;

                        bw.boneIndex2 = joints0Arr[j].z;
                        bw.weight2 = weights0Arr[j].z;

                        bw.boneIndex3 = joints0Arr[j].w;
                        bw.weight3 = weights0Arr[j].w;

                        context.boneWeights.Add(bw);
                    }
                }

                // blendshape
                if (prim.Targets != null && prim.Targets.Count > 0)
                {
                    var targetNames = prim.Extras as Dictionary<int, string>;
                    for (int i = 0; i < prim.Targets.Count; ++i)
                    {
                        //var name = string.Format("target{0}", i++);
                        var primTarget = prim.Targets[i];

                        string targetName;
                        if (targetNames == null || !targetNames.TryGetValue(i, out targetName))
                        {
                            targetName = i.ToString();
                        }
                        var blendShape = new BlendShape(targetName);

                        var targetPositions = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "POSITION");
                        if (targetPositions != null)
                        {
                            blendShape.Positions.AddRange(targetPositions.Select(x => x.ReverseZ()));
                        }

                        var targetNormals = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "NORMAL");
                        if (targetNormals != null)
                        {
                            blendShape.Normals.AddRange(targetNormals.Select(x => x.ReverseZ()));
                        }

                        var targetTangents = GetAttrBuffer<Vector3>(ctx.Store, primTarget, "TANGENT");
                        if (targetTangents != null)
                        {
                            blendShape.Tangents.AddRange(targetTangents.Select(x => x.ReverseZ()));
                        }

                        context.blendShapes.Add(blendShape);
                    }
                }
            }

            foreach (var prim in gltfMesh.Primitives)
            {
                var meshes =
                    prim.Indices != null
                    ? ctx.Store.GetOrLoadTypedBufferByAccessorIndex(prim.Indices.Value).GetPrimitivesAsCasted<int>()
                    : Enumerable.Range(0, context.positions.Length)
                    ;
                context.subMeshes.Add(TriangleUtil.FlipTriangle(meshes).ToArray());

                // material
                if (prim.Material != null)
                {
                    context.materialIndices.Add(prim.Material.Value);
                }
            }

            return context;
        }


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


        public class MeshContext
        {
            public string name;
            public Vector3[] positions;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Vector2[] uv;
            public Color[] colors;
            public List<BoneWeight> boneWeights = new List<BoneWeight>();
            public List<int[]> subMeshes = new List<int[]>();
            public List<int> materialIndices = new List<int>();
            public List<BlendShape> blendShapes = new List<BlendShape>();
        }


        public MeshContext ReadMesh(ImporterContext ctx, int meshIndex)
        {
            var gltfMesh = ctx.GLTF2.Meshes[meshIndex];

            Dictionary<string, int> lastAttributes = null;
            var sharedAttributes = true;
            foreach (var prim in gltfMesh.Primitives)
            {
                if (lastAttributes != null &&
                    !prim.Attributes.OrderBy(kv => kv.Key).SequenceEqual(lastAttributes.OrderBy(kv => kv.Key)))
                {
                    sharedAttributes = false;
                    break;
                }
                lastAttributes = prim.Attributes;
            }

            var meshContext = sharedAttributes
                ? _ImportMeshSharingVertexBuffer(ctx, gltfMesh)
                : _ImportMeshIndependentVertexBuffer(ctx, gltfMesh)
                ;
            meshContext.name = gltfMesh.Name;
            if (string.IsNullOrEmpty(meshContext.name))
            {
                meshContext.name = string.Format("UniGLTF import#{0}", meshIndex);
            }

            return meshContext;
        }


        public static MeshWithMaterials BuildMesh(ImporterContext ctx, MeshImporter.MeshContext meshContext)
        {
            if (!meshContext.materialIndices.Any())
            {
                meshContext.materialIndices.Add(0);
            }

            //Debug.Log(prims.ToJson());
            var mesh = new Mesh();
            mesh.name = meshContext.name;

            if (meshContext.positions.Length > UInt16.MaxValue)
            {
#if UNITY_2017_3_OR_NEWER
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                Debug.LogWarningFormat("vertices {0} exceed 65535. not implemented. Unity2017.3 supports large mesh",
                    meshContext.positions.Length);
#endif
            }

            mesh.vertices = meshContext.positions;
            bool recalculateNormals = false;
            if (meshContext.normals != null && meshContext.normals.Length > 0)
            {
                mesh.normals = meshContext.normals;
            }
            else
            {
                recalculateNormals = true;
            }

            if (meshContext.uv != null && meshContext.uv.Length > 0)
            {
                mesh.uv = meshContext.uv;
            }

            bool recalculateTangents = true;
#if UNIGLTF_IMPORT_TANGENTS
            if (meshContext.tangents != null && meshContext.tangents.Length > 0)
            {
                mesh.tangents = meshContext.tangents;
                recalculateTangents = false;
            }
#endif

            if (meshContext.colors != null && meshContext.colors.Length > 0)
            {
                mesh.colors = meshContext.colors;
            }
            if (meshContext.boneWeights != null && meshContext.boneWeights.Count > 0)
            {
                mesh.boneWeights = meshContext.boneWeights.ToArray();
            }
            mesh.subMeshCount = meshContext.subMeshes.Count;
            for (int i = 0; i < meshContext.subMeshes.Count; ++i)
            {
                mesh.SetTriangles(meshContext.subMeshes[i], i);
            }

            if (recalculateNormals)
            {
                mesh.RecalculateNormals();
            }
            if (recalculateTangents)
            {
#if UNITY_5_6_OR_NEWER
                mesh.RecalculateTangents();
#else
                CalcTangents(mesh);
#endif
            }

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.materialIndices.Select(x => ctx.GetMaterial(x)).ToArray()
            };

            if (meshContext.blendShapes != null)
            {
                Vector3[] emptyVertices = null;
                foreach (var blendShape in meshContext.blendShapes)
                {
                    if (blendShape.Positions.Count > 0)
                    {
                        if (blendShape.Positions.Count == mesh.vertexCount)
                        {
                            mesh.AddBlendShapeFrame(blendShape.Name, FRAME_WEIGHT,
                                blendShape.Positions.ToArray(),
                                (meshContext.normals != null && meshContext.normals.Length == mesh.vertexCount) ? blendShape.Normals.ToArray() : null,
                                null
                                );
                        }
                        else
                        {
                            Debug.LogWarningFormat("May be partial primitive has blendShape. Rquire separete mesh or extend blend shape, but not implemented: {0}", blendShape.Name);
                        }
                    }
                    else
                    {
                        if (emptyVertices == null)
                        {
                            emptyVertices = new Vector3[mesh.vertexCount];
                        }
                        // Debug.LogFormat("empty blendshape: {0}.{1}", mesh.name, blendShape.Name);
                        // add empty blend shape for keep blend shape index
                        mesh.AddBlendShapeFrame(blendShape.Name, FRAME_WEIGHT,
                            emptyVertices,
                            null,
                            null
                            );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Meshの法線を元にタンジェントを計算する。
        /// </summary>
        /// <param name="mesh">メッシュ</param>
        /// <returns>タンジェント</returns>
        public static void CalcTangents(Mesh mesh)
        {
            int vertexCount = mesh.vertexCount;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] texcoords = mesh.uv;
            int[] triangles = mesh.triangles;
            int triangleCount = triangles.Length / 3;

            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            int tri = 0;

            for (int i = 0; i < (triangleCount); i++)
            {
                int i1 = triangles[tri];
                int i2 = triangles[tri + 1];
                int i3 = triangles[tri + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = texcoords[i1];
                Vector2 w2 = texcoords[i2];
                Vector2 w3 = texcoords[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;

                tri += 3;
            }

            for (int i = 0; i < (vertexCount); i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];

                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);
                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;

                // Calculate handedness
                tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }
    }
}
