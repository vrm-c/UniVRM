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

            public readonly List<Vector3> positions = new List<Vector3>();
            public readonly List<Vector3> normals = new List<Vector3>();

            [Obsolete]
            public readonly List<Vector4> tangents = new List<Vector4>();

            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<Vector2> uv2 = new List<Vector2>();
            public readonly List<Color> colors = new List<Color>();
            public readonly List<BoneWeight> boneWeights = new List<BoneWeight>();
            public readonly List<int[]> subMeshes = new List<int[]>();
            public readonly List<int> materialIndices = new List<int>();
            public readonly List<BlendShape> blendShapes = new List<BlendShape>();

            public MeshContext(string name, int meshIndex)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = string.Format("UniGLTF import#{0}", meshIndex);
                }
                m_name = name;
            }

            /// <summary>
            /// mesh.extras.targetNames が存在する => UniVRMでエクスポートしたものであると仮定
            /// 
            /// * 各primitiveが同じ attribute を共有している
            /// * 各primitiveが同じ targets を共有している
            /// * 各primitiveは indices が異なる(submesh)
            /// 
            /// を仮定して Mesh をロードする。
            /// </summary>
            /// <param name="ctx"></param>
            /// <param name="gltfMesh"></param>
            /// <returns></returns>
            public void ImportMeshSharingMorphTarget(ImporterContext ctx, glTFMesh gltfMesh)
            {
                // blendshapes
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
                    var blendShape = new BlendShape(!string.IsNullOrEmpty(targetNames[i]) ? targetNames[i] : i.ToString());
                    blendShapes.Add(blendShape);
                }

                foreach (var prim in gltfMesh.primitives)
                {
                    var indexOffset = positions.Count;
                    var indexBuffer = prim.indices;

                    var positionCount = positions.Count;
                    positions.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).Select(x => x.ReverseZ()));
                    positionCount = positions.Count - positionCount;

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        normals.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).Select(x => x.ReverseZ()));
                    }

#if false
                // tangent
                if (prim.attributes.TANGENT != -1)
                {
                    tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).Select(x => x.ReverseZ()));
                }
#endif

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseUV()));
                        }
                    }
                    else
                    {
                        // for inconsistent attributes in primitives
                        uv.AddRange(new Vector2[positionCount]);
                    }

                    // uv1
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        uv2.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).Select(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        colors.AddRange(ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
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

                            boneWeights.Add(bw);
                        }
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            var primTarget = prim.targets[i];
                            if (primTarget.POSITION != -1)
                            {
                                blendShapes[i].Positions.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                blendShapes[i].Normals.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                blendShapes[i].Tangents.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT).Select(x => x.ReverseZ()).ToArray());
                            }
                        }
                    }

                    var indices =
                     (indexBuffer >= 0)
                     ? ctx.GLTF.GetIndices(indexBuffer)
                     : TriangleUtil.FlipTriangle(Enumerable.Range(0, positions.Count)).ToArray() // without index array
                     ;
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        indices[i] += indexOffset;
                    }

                    subMeshes.Add(indices);

                    // material
                    materialIndices.Add(prim.material);
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
                var targets = gltfMesh.primitives[0].targets;
                for (int i = 1; i < gltfMesh.primitives.Count; ++i)
                {
                    if (!gltfMesh.primitives[i].targets.SequenceEqual(targets))
                    {
                        //
                        // 各 primitive の morphTarget の内容が違うことは許容しない
                        //
                        throw new NotImplementedException(string.Format("different targets: {0} with {1}",
                            gltfMesh.primitives[i],
                            targets));
                    }
                }

                foreach (var prim in gltfMesh.primitives)
                {
                    var indexOffset = positions.Count;
                    var indexBuffer = prim.indices;

                    var positionCount = positions.Count;
                    positions.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).Select(x => x.ReverseZ()));
                    positionCount = positions.Count - positionCount;

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        normals.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).Select(x => x.ReverseZ()));
                    }

#if false
                if (prim.attributes.TANGENT != -1)
                {
                    tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).Select(x => x.ReverseZ()));
                }
#endif

                    // uv
                    if (prim.attributes.TEXCOORD_0 != -1)
                    {
                        if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).Select(x => x.ReverseUV()));
                        }
                    }
                    else
                    {
                        // for inconsistent attributes in primitives
                        uv.AddRange(new Vector2[positionCount]);
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        uv2.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).Select(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        colors.AddRange(ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                    }

                    // skin
                    if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                    {
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

                            boneWeights.Add(bw);
                        }
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            //var name = string.Format("target{0}", i++);
                            var primTarget = prim.targets[i];
                            var blendShape = new BlendShape(!string.IsNullOrEmpty(prim.extras.targetNames[i])
                                ? prim.extras.targetNames[i]
                                : i.ToString())
                                ;
                            if (primTarget.POSITION != -1)
                            {
                                blendShape.Positions.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.NORMAL != -1)
                            {
                                blendShape.Normals.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL).Select(x => x.ReverseZ()).ToArray());
                            }
                            if (primTarget.TANGENT != -1)
                            {
                                blendShape.Tangents.AddRange(
                                    ctx.GLTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT).Select(x => x.ReverseZ()).ToArray());
                            }
                            blendShapes.Add(blendShape);
                        }
                    }

                    var indices =
                     (indexBuffer >= 0)
                     ? ctx.GLTF.GetIndices(indexBuffer)
                     : TriangleUtil.FlipTriangle(Enumerable.Range(0, positions.Count)).ToArray() // without index array
                     ;
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        indices[i] += indexOffset;
                    }

                    subMeshes.Add(indices);

                    // material
                    materialIndices.Add(prim.material);
                }
            }

            // multiple submesh sharing same VertexBuffer
            public void ImportMeshSharingVertexBuffer(ImporterContext ctx, glTFMesh gltfMesh)
            {
                {
                    var prim = gltfMesh.primitives.First();
                    positions.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).SelectInplace(x => x.ReverseZ()));

                    // normal
                    if (prim.attributes.NORMAL != -1)
                    {
                        normals.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).SelectInplace(x => x.ReverseZ()));
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
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseY()));
#pragma warning restore 0612
                        }
                        else
                        {
                            uv.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseUV()));
                        }
                    }

                    // uv2
                    if (prim.attributes.TEXCOORD_1 != -1)
                    {
                        uv2.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1).SelectInplace(x => x.ReverseUV()));
                    }

                    // color
                    if (prim.attributes.COLOR_0 != -1)
                    {
                        if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 3)
                        {
                            var vec3Color = ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.COLOR_0);
                            colors.AddRange(new Color[vec3Color.Length]);

                            for (int i = 0; i < vec3Color.Length; i++)
                            {
                                Vector3 color = vec3Color[i];
                                colors[i] = new Color(color.x, color.y, color.z);
                            }
                        }
                        else if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 4)
                        {
                            colors.AddRange(ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
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

                            boneWeights.Add(bw);
                        }
                    }

                    // blendshape
                    if (prim.targets != null && prim.targets.Count > 0)
                    {
                        blendShapes.AddRange(prim.targets.Select((x, i) => new BlendShape(
                            i < prim.extras.targetNames.Count && !string.IsNullOrEmpty(prim.extras.targetNames[i])
                            ? prim.extras.targetNames[i]
                            : i.ToString())));
                        for (int i = 0; i < prim.targets.Count; ++i)
                        {
                            //var name = string.Format("target{0}", i++);
                            var primTarget = prim.targets[i];
                            var blendShape = blendShapes[i];

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
                        subMeshes.Add(TriangleUtil.FlipTriangle(Enumerable.Range(0, positions.Count)).ToArray());
                    }
                    else
                    {
                        var indices = ctx.GLTF.GetIndices(prim.indices);
                        subMeshes.Add(indices);
                    }

                    // material
                    materialIndices.Add(prim.material);
                }
            }
        }

        public MeshContext ReadMesh(ImporterContext ctx, int meshIndex)
        {
            var gltfMesh = ctx.GLTF.meshes[meshIndex];

            bool sharedMorphTarget = gltfMesh.extras != null && gltfMesh.extras.targetNames.Count > 0;
            var meshContext = new MeshContext(gltfMesh.name, meshIndex);
            if (sharedMorphTarget)
            {
                meshContext.ImportMeshSharingMorphTarget(ctx, gltfMesh);
            }
            else
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

                if (sharedAttributes)
                {
                    meshContext.ImportMeshSharingVertexBuffer(ctx, gltfMesh);
                }
                else
                {
                    meshContext.ImportMeshIndependentVertexBuffer(ctx, gltfMesh);
                }
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

            if (meshContext.positions.Count > UInt16.MaxValue)
            {
#if UNITY_2017_3_OR_NEWER
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                Debug.LogWarningFormat("vertices {0} exceed 65535. not implemented. Unity2017.3 supports large mesh",
                    meshContext.positions.Length);
#endif
            }

            mesh.vertices = meshContext.positions.ToArray();
            bool recalculateNormals = false;
            if (meshContext.normals != null && meshContext.normals.Count > 0)
            {
                mesh.normals = meshContext.normals.ToArray();
            }
            else
            {
                recalculateNormals = true;
            }

            if (meshContext.uv != null && meshContext.uv.Count == mesh.vertexCount)
            {
                mesh.uv = meshContext.uv.ToArray();
            }
            if (meshContext.uv2 != null && meshContext.uv2.Count == mesh.vertexCount)
            {
                mesh.uv2 = meshContext.uv2.ToArray();
            }

            bool recalculateTangents = true;
#if UNIGLTF_IMPORT_TANGENTS
            if (meshContext.tangents != null && meshContext.tangents.Length > 0)
            {
                mesh.tangents = meshContext.tangents;
                recalculateTangents = false;
            }
#endif

            if (meshContext.colors != null && meshContext.colors.Count == mesh.vertexCount)
            {
                mesh.colors = meshContext.colors.ToArray();
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
                                (meshContext.normals != null && meshContext.normals.Count == mesh.vertexCount && blendShape.Normals.Count() == blendShape.Positions.Count()) ? blendShape.Normals.ToArray() : null,
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

        public static IEnumerator BuildMeshCoroutine(ImporterContext ctx, MeshImporter.MeshContext meshContext)
        {
            if (!meshContext.materialIndices.Any())
            {
                meshContext.materialIndices.Add(0);
            }

            var mesh = new Mesh();
            mesh.name = meshContext.name;

            if (meshContext.positions.Count > UInt16.MaxValue)
            {
#if UNITY_2017_3_OR_NEWER
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                Debug.LogWarningFormat("vertices {0} exceed 65535. not implemented. Unity2017.3 supports large mesh",
                    meshContext.positions.Length);
#endif
            }


            mesh.vertices = meshContext.positions.ToArray();
            bool recalculateNormals = false;
            if (meshContext.normals != null && meshContext.normals.Count == mesh.vertexCount)
            {

                mesh.normals = meshContext.normals.ToArray();
            }
            else
            {
                recalculateNormals = true;
            }

            if (meshContext.uv != null && meshContext.uv.Count == mesh.vertexCount)
            {

                mesh.uv = meshContext.uv.ToArray();
            }

            bool recalculateTangents = true;
#if UNIGLTF_IMPORT_TANGENTS
            if (meshContext.tangents != null && meshContext.tangents.Count == mesh.vertexCount)
            {
                mesh.tangents = meshContext.tangents.ToArray();
                recalculateTangents = false;
            }
#endif

            if (meshContext.colors != null && meshContext.colors.Count == mesh.vertexCount)
            {
                mesh.colors = meshContext.colors.ToArray();
            }

            if (meshContext.boneWeights != null && meshContext.boneWeights.Count == mesh.vertexCount)
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
                yield return null;
                mesh.RecalculateTangents();
                yield return null;
#else
                CalcTangents(mesh);
#endif
            }

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.materialIndices.Select(x => ctx.GetMaterial(x)).ToArray()
            };

            yield return null;
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
                                (meshContext.normals != null && meshContext.normals.Count == mesh.vertexCount && blendShape.Normals.Count() == blendShape.Positions.Count()) ? blendShape.Normals.ToArray() : null,
                                null
                            );
                            yield return null;
                        }
                        else
                        {
                            Debug.LogWarningFormat("May be partial primitive has blendShape. Require separate mesh or extend blend shape, but not implemented: {0}", blendShape.Name);
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
                        yield return null;
                    }
                }
            }

            yield return result;
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
