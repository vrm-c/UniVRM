using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class MeshExporter_DividedVertexBuffer
    {
        /// <summary>
        /// Divide vertex buffer(Position, Normal, UV, VertexColor, Skinning and BlendShapes) by submesh usage, then export
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="gltfBuffer"></param>
        /// <param name="unityMesh"></param>
        /// <param name="unityMaterials"></param>
        /// <param name="axisInverter"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static (glTFMesh, Dictionary<int, int>) Export(ExportingGltfData data,
            MeshExportInfo unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter, GltfExportSettings settings)
        {
            var mesh = unityMesh.Mesh;
            var gltfMesh = new glTFMesh(mesh.name);

            if (settings.ExportTangents)
            {
                // no support
                throw new NotImplementedException();
            }

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uv = mesh.uv;
            var boneWeights = mesh.boneWeights;
            if (boneWeights.All(x => x.weight0 == 0 && x.weight1 == 0 && x.weight2 == 0 && x.weight3 == 0))
            {
                boneWeights = null;
            }
            var colors = mesh.colors;

            Func<int, int> getJointIndex = null;
            if (boneWeights != null && boneWeights.Length == positions.Length)
            {
                getJointIndex = unityMesh.GetJointIndex;
            }

            Vector3[] blendShapePositions = new Vector3[mesh.vertexCount];
            Vector3[] blendShapeNormals = new Vector3[mesh.vertexCount];

            var vColorState = VertexColorUtility.DetectVertexColor(mesh, unityMaterials);
            var exportVertexColor = (
                (settings.ExportVertexColor && mesh.colors != null && mesh.colors.Length == mesh.vertexCount) // vertex color を残す設定
                || vColorState == VertexColorState.ExistsAndIsUsed // VColor使っている
                || vColorState == VertexColorState.ExistsAndMixed // VColorを使っているところと使っていないところが混在(とりあえずExportする)
            );

            var usedIndices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i);
                var hash = new HashSet<int>(indices);

                // aggregate vertex attributes
                var buffer = new MeshExportUtil.VertexBuffer(indices.Length, getJointIndex);
                usedIndices.Clear();
                for (int k = 0; k < positions.Length; ++k)
                {
                    if (hash.Contains(k))
                    {
                        // aggregate indices
                        usedIndices.Add(k);
                        buffer.PushVertex(k,
                            axisInverter.InvertVector3(positions[k]), // POSITION
                            axisInverter.InvertVector3(normals[k]), // NORMAL
                            uv[k].ReverseUV() // UV
                            );
                        if (getJointIndex != null)
                        {
                            buffer.PushBoneWeight(boneWeights[k]);
                        }
                        if (exportVertexColor)
                        {
                            buffer.PushColor(colors[k]);
                        }
                    }
                }

                var material = unityMesh.Materials[i];
                var materialIndex = -1;
                if (material != null)
                {
                    materialIndex = unityMaterials.IndexOf(material);
                }

                var flipped = new List<int>();
                for (int j = 0; j < indices.Length; j += 3)
                {
                    var t0 = indices[j];
                    var t1 = indices[j + 1];
                    var t2 = indices[j + 2];
                    flipped.Add(t2);
                    flipped.Add(t1);
                    flipped.Add(t0);
                }
                var gltfPrimitive = buffer.ToGltfPrimitive(data, materialIndex, flipped);

                // blendShape(morph target)
                for (int j = 0; j < mesh.blendShapeCount; ++j)
                {
                    var blendShape = new MeshExportUtil.BlendShapeBuffer(usedIndices.Count);

                    // aggriage morph target
                    mesh.GetBlendShapeFrameVertices(j, 0, blendShapePositions, blendShapeNormals, null);
                    int l = 0;
                    foreach (var k in usedIndices)
                    {
                        blendShape.Set(l++,
                            axisInverter.InvertVector3(blendShapePositions[k]),
                            axisInverter.InvertVector3(blendShapeNormals[k]));
                    }

                    gltfPrimitive.targets.Add(blendShape.ToGltf(data, !settings.ExportOnlyBlendShapePosition,
                        settings.UseSparseAccessorForMorphTarget));
                }

                gltfMesh.primitives.Add(gltfPrimitive);
            }

            var targetNames = Enumerable.Range(0, mesh.blendShapeCount).Select(x => mesh.GetBlendShapeName(x)).ToArray();
            gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames, BlendShapeTargetNameLocationFlags.Both);

            return (gltfMesh, Enumerable.Range(0, mesh.blendShapeCount).ToDictionary(x => x, x => x));
        }
    }
}
