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


        // mesh is sharing morph targets.
        private static MeshContext _ImportMeshSharingMorphTarget(ImporterContext ctx, glTFMesh gltfMesh)
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();
            var blendShapes = new List<BlendShape>();
            var meshContext = new MeshContext();
            
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

                if (prim.attributes.TANGENT != -1)
                {
                    tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).Select(x => x.ReverseZ()));
                }

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

                        meshContext.boneWeights.Add(bw);
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
                 : TriangleUtil.FlipTriangle(Enumerable.Range(0, meshContext.positions.Length)).ToArray() // without index array
                 ;
                for (int i = 0; i < indices.Length; ++i)
                {
                    indices[i] += indexOffset;
                }

                meshContext.subMeshes.Add(indices);

                // material
                meshContext.materialIndices.Add(prim.material);
            }

            meshContext.positions = positions.ToArray();
            meshContext.normals = normals.ToArray();
            meshContext.tangents = tangents.ToArray();
            meshContext.uv = uv.ToArray();
            meshContext.blendShapes = blendShapes;

            return meshContext;
        }

        // multiple submMesh is not sharing a VertexBuffer.
        // each subMesh use a independent VertexBuffer.
        private static MeshContext _ImportMeshIndependentVertexBuffer(ImporterContext ctx, glTFMesh gltfMesh)
        {
            //Debug.LogWarning("_ImportMeshIndependentVertexBuffer");

            var targets = gltfMesh.primitives[0].targets;
            for (int i = 1; i < gltfMesh.primitives.Count; ++i)
            {
                if (!gltfMesh.primitives[i].targets.SequenceEqual(targets))
                {
                    throw new NotImplementedException(string.Format("different targets: {0} with {1}",
                        gltfMesh.primitives[i],
                        targets));
                }
            }

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();
            var meshContext = new MeshContext();
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

                if (prim.attributes.TANGENT != -1)
                {
                    tangents.AddRange(ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).Select(x => x.ReverseZ()));
                }

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

                        meshContext.boneWeights.Add(bw);
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
                        meshContext.blendShapes.Add(blendShape);
                    }
                }

                var indices =
                 (indexBuffer >= 0)
                 ? ctx.GLTF.GetIndices(indexBuffer)
                 : TriangleUtil.FlipTriangle(Enumerable.Range(0, meshContext.positions.Length)).ToArray() // without index array
                 ;
                for (int i = 0; i < indices.Length; ++i)
                {
                    indices[i] += indexOffset;
                }

                meshContext.subMeshes.Add(indices);

                // material
                meshContext.materialIndices.Add(prim.material);
            }

            meshContext.positions = positions.ToArray();
            meshContext.normals = normals.ToArray();
            meshContext.tangents = tangents.ToArray();
            meshContext.uv = uv.ToArray();

            return meshContext;
        }


        // multiple submesh sharing same VertexBuffer
        private static MeshContext _ImportMeshSharingVertexBuffer(ImporterContext ctx, glTFMesh gltfMesh)
        {
            var context = new MeshContext();

            {
                var prim = gltfMesh.primitives.First();
                context.positions = ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION).SelectInplace(x => x.ReverseZ());

                // normal
                if (prim.attributes.NORMAL != -1)
                {
                    context.normals = ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL).SelectInplace(x => x.ReverseZ());
                }

                // tangent
                if (prim.attributes.TANGENT != -1)
                {
                    context.tangents = ctx.GLTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT).SelectInplace(x => x.ReverseZ());
                }

                // uv
                if (prim.attributes.TEXCOORD_0 != -1)
                {
                    if (ctx.IsGeneratedUniGLTFAndOlder(1, 16))
                    {
#pragma warning disable 0612
                        // backward compatibility
                        context.uv = ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseY());
#pragma warning restore 0612
                    }
                    else
                    {
                        context.uv = ctx.GLTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0).SelectInplace(x => x.ReverseUV());
                    }
                }
                else
                {
                    // for inconsistent attributes in primitives
                    context.uv = new Vector2[context.positions.Length];
                }

                // color
                if (prim.attributes.COLOR_0 != -1)
                {
                    if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 3)
                    {
                        var vec3Color = ctx.GLTF.GetArrayFromAccessor<Vector3>(prim.attributes.COLOR_0);
                        context.colors = new Color[vec3Color.Length];

                        for (int i = 0; i < vec3Color.Length; i++)
                        {
                            Vector3 color = vec3Color[i];
                            context.colors[i] = new Color(color.x, color.y, color.z);
                        }
                    }
                    else if (ctx.GLTF.accessors[prim.attributes.COLOR_0].TypeCount == 4)
                    {
                        context.colors = ctx.GLTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0);
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

                        context.boneWeights.Add(bw);
                    }
                }

                // blendshape
                if (prim.targets != null && prim.targets.Count > 0)
                {
                    context.blendShapes.AddRange(prim.targets.Select((x, i) => new BlendShape(
                        i < prim.extras.targetNames.Count && !string.IsNullOrEmpty(prim.extras.targetNames[i])
                        ? prim.extras.targetNames[i]
                        : i.ToString())));
                    for (int i = 0; i < prim.targets.Count; ++i)
                    {
                        //var name = string.Format("target{0}", i++);
                        var primTarget = prim.targets[i];
                        var blendShape = context.blendShapes[i];

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
                    context.subMeshes.Add(TriangleUtil.FlipTriangle(Enumerable.Range(0, context.positions.Length)).ToArray());
                }
                else
                {
                    var indices = ctx.GLTF.GetIndices(prim.indices);
                    context.subMeshes.Add(indices);
                }

                // material
                context.materialIndices.Add(prim.material);
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
            var gltfMesh = ctx.GLTF.meshes[meshIndex];

            bool sharedMorphTarget = gltfMesh.extras != null && gltfMesh.extras.targetNames.Count > 0;
            MeshContext meshContext;
            if (sharedMorphTarget)
            {
                meshContext = _ImportMeshSharingMorphTarget(ctx, gltfMesh);
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

                meshContext = sharedAttributes
                    ? _ImportMeshSharingVertexBuffer(ctx, gltfMesh)
                    : _ImportMeshIndependentVertexBuffer(ctx, gltfMesh);
            }

            meshContext.name = gltfMesh.name;
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
                                (meshContext.normals != null && meshContext.normals.Length == mesh.vertexCount && blendShape.Normals.Count() == blendShape.Positions.Count()) ? blendShape.Normals.ToArray() : null,
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
                                (meshContext.normals != null && meshContext.normals.Length == mesh.vertexCount && blendShape.Normals.Count() == blendShape.Positions.Count()) ? blendShape.Normals.ToArray() : null,
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
