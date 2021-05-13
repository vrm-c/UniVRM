using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class MeshExporterDivided
    {
        public static glTFMesh Export(glTF gltf, int bufferIndex,
            MeshWithRenderer unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter, MeshExportSettings settings)
        {
            var mesh = unityMesh.Mesh;
            var gltfMesh = new glTFMesh(mesh.name);

            if (settings.ExportTangents)
            {
                // support しない
                throw new NotImplementedException();
            }

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uv = mesh.uv;
            var boneWeights = mesh.boneWeights;

            Func<int, int> getJointIndex = null;
            if (boneWeights != null && boneWeights.Length == positions.Length)
            {
                getJointIndex = unityMesh.GetJointIndex;
            }

            Vector3[] blendShapePositions = new Vector3[mesh.vertexCount];
            Vector3[] blendShapeNormals = new Vector3[mesh.vertexCount];

            var usedIndices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i);
                var hash = new HashSet<int>(indices);

                // mesh
                // index の順に attributes を蓄える                
                var buffer = new MeshExportUtil.VertexBuffer(indices.Length, getJointIndex);
                usedIndices.Clear();
                for (int k = 0; k < positions.Length; ++k)
                {
                    if (hash.Contains(k))
                    {
                        // indices から参照される頂点だけを蓄える
                        usedIndices.Add(k);
                        buffer.Push(k, axisInverter.InvertVector3(positions[k]), axisInverter.InvertVector3(normals[k]), uv[k].ReverseUV());
                        if (getJointIndex != null)
                        {
                            buffer.Push(boneWeights[k]);
                        }
                    }
                }

                var material = unityMesh.Renderer.sharedMaterials[i];
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
                var gltfPrimitive = buffer.ToGltfPrimitive(gltf, bufferIndex, materialIndex, flipped);

                // blendShape
                for (int j = 0; j < mesh.blendShapeCount; ++j)
                {
                    var blendShape = new MeshExportUtil.BlendShapeBuffer(indices.Length);

                    // index の順に attributes を蓄える                
                    mesh.GetBlendShapeFrameVertices(j, 0, blendShapePositions, blendShapeNormals, null);
                    foreach (var k in usedIndices)
                    {
                        blendShape.Push(
                            axisInverter.InvertVector3(blendShapePositions[k]),
                            axisInverter.InvertVector3(blendShapeNormals[k]));
                    }

                    gltfPrimitive.targets.Add(blendShape.ToGltf(gltf, bufferIndex, !settings.ExportOnlyBlendShapePosition));
                }

                gltfMesh.primitives.Add(gltfPrimitive);
            }

            var targetNames = Enumerable.Range(0, mesh.blendShapeCount).Select(x => mesh.GetBlendShapeName(x)).ToArray();
            gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames);

            return gltfMesh;
        }
    }
}
