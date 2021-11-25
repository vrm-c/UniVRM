using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UniGLTF
{
    internal class MeshContext
    {
        private readonly List<MeshVertex> _vertices = new List<MeshVertex>();
        private readonly List<int> _indices = new List<int>();
        private readonly List<SubMeshDescriptor> _subMeshes = new List<SubMeshDescriptor>();
        private readonly List<int> _materialIndices = new List<int>();
        private readonly List<BlendShape> _blendShapes = new List<BlendShape>();

        public IReadOnlyList<int> MaterialIndices => _materialIndices;
        public IReadOnlyList<BlendShape> BlendShapes => _blendShapes;

        public bool HasNormal { get; private set; } = true;

        public string Name { get; }

        /// <summary>
        /// 頂点情報をMeshに対して送る
        /// </summary>
        /// <param name="mesh"></param>
        public void UploadMeshVertices(Mesh mesh)
        {
            mesh.SetVertexBufferParams(_vertices.Count, MeshVertex.GetVertexAttributeDescriptor());
            mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Count);
        }
        /// <summary>
        /// インデックス情報をMeshに対して送る
        /// </summary>
        /// <param name="mesh"></param>
        public void UploadMeshIndices(Mesh mesh)
        {
            mesh.SetIndexBufferParams(_indices.Count, IndexFormat.UInt32);
            mesh.SetIndexBufferData(_indices, 0, 0, _indices.Count);
            mesh.subMeshCount = _subMeshes.Count;
            for (var i = 0; i < _subMeshes.Count; i++)
            {
                mesh.SetSubMesh(i, _subMeshes[i]);
            }
        }
        
        private BlendShape GetOrCreateBlendShape(int i)
        {
            if (i < _blendShapes.Count && _blendShapes[i] != null)
            {
                return _blendShapes[i];
            }

            while (_blendShapes.Count <= i)
            {
                _blendShapes.Add(null);
            }

            var blendShape = new BlendShape(i.ToString());
            _blendShapes[i] = blendShape;
            return blendShape;
        }

        public MeshContext(string name, int meshIndex)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"UniGLTF import#{meshIndex}";
            }

            Name = name;
        }

        private static (float x, float y, float z, float w) NormalizeBoneWeight(
            (float x, float y, float z, float w) src)
        {
            var sum = src.x + src.y + src.z + src.w;
            if (sum == 0)
            {
                return src;
            }

            var f = 1.0f / sum;
            src.x *= f;
            src.y *= f;
            src.z *= f;
            src.w *= f;
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
        public void ImportMeshIndependentVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            foreach (var primitives in gltfMesh.primitives)
            {
                var vertexOffset = _vertices.Count;
                var indexBufferCount = primitives.indices;

                // position は必ずある
                var positions = primitives.GetPositions(data);
                var normals = primitives.GetNormals(data, positions.Length);
                var texCoords0 = primitives.GetTexCoords0(data, positions.Length);
                var texCoords1 = primitives.GetTexCoords1(data, positions.Length);
                var colors = primitives.GetColors(data, positions.Length);
                var jointsGetter = primitives.GetJoints(data, positions.Length);
                var weightsGetter = primitives.GetWeights(data, positions.Length);

                CheckAttributeUsages(primitives);

                for (var i = 0; i < positions.Length; ++i)
                {
                    var position = inverter.InvertVector3(positions[i]);
                    var normal = normals != null ? inverter.InvertVector3(normals[i]) : Vector3.zero;

                    var texCoord0 = Vector2.zero;
                    if (texCoords0 != null)
                    {
                        if (data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            // backward compatibility
                            texCoord0 = texCoords0[i].ReverseY();
#pragma warning restore 0612
                        }
                        else
                        {
                            texCoord0 = texCoords0[i].ReverseUV();
                        }
                    }

                    var texCoord1 = texCoords1 != null ? texCoords1[i].ReverseUV() : Vector2.zero;
                    var joints = jointsGetter?.Invoke(i) ?? (0, 0, 0, 0);
                    var weights = weightsGetter != null ? NormalizeBoneWeight(weightsGetter(i)) : (0, 0, 0, 0);

                    var color = colors != null ? colors[i] : Color.black;
                    _vertices.Add(
                        new MeshVertex(
                            position,
                            normal,
                            texCoord0,
                            texCoord1,
                            color,
                            joints.x,
                            joints.y,
                            joints.z,
                            joints.w,
                            weights.x,
                            weights.y,
                            weights.z,
                            weights.w
                        ));
                }

                // blendshape
                if (primitives.targets != null && primitives.targets.Count > 0)
                {
                    for (var i = 0; i < primitives.targets.Count; ++i)
                    {
                        var primTarget = primitives.targets[i];
                        var blendShape = GetOrCreateBlendShape(i);
                        if (primTarget.POSITION != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.POSITION);
                            if (array.Length != positions.Length)
                            {
                                throw new Exception("different length");
                            }

                            blendShape.Positions.AddRange(array.Select(inverter.InvertVector3).ToArray());
                        }

                        if (primTarget.NORMAL != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.NORMAL);
                            if (array.Length != positions.Length)
                            {
                                throw new Exception("different length");
                            }

                            blendShape.Normals.AddRange(array.Select(inverter.InvertVector3).ToArray());
                        }

                        if (primTarget.TANGENT != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.TANGENT);
                            if (array.Length != positions.Length)
                            {
                                throw new Exception("different length");
                            }

                            blendShape.Tangents.AddRange(array.Select(inverter.InvertVector3).ToArray());
                        }
                    }
                }

                if (indexBufferCount >= 0)
                {
                    var indexOffset = _indices.Count;
                    var dataIndices = data.GetIndices(indexBufferCount);
                    _indices.AddRange(dataIndices.Select(index => index + vertexOffset));
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, dataIndices.Length));
                }
                else
                {
                    var indexOffset = _indices.Count;
                    _indices.AddRange(TriangleUtil.FlipTriangle(Enumerable.Range(0, _vertices.Count)).Select(index => index + vertexOffset));
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, _vertices.Count));
                }

                // material
                _materialIndices.Add(primitives.material);
            }
        }

        /// <summary>
        /// 各種頂点属性が使われているかどうかをチェックし、使われていなかったらフラグを切る
        /// MEMO: O(1)で検知する手段がありそう
        /// </summary>
        private void CheckAttributeUsages(glTFPrimitives primitives)
        {
            if (!primitives.HasNormal()) HasNormal = false;
        }

        /// <summary>
        /// 
        /// 各primitiveが同じ attribute を共有している場合専用のローダー。
        ///
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="gltfMesh"></param>
        /// <returns></returns>
        public void ImportMeshSharingVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            {
                //  同じVertexBufferを共有しているので先頭のモノを使う
                var primitives = gltfMesh.primitives.First();

                var positions = primitives.GetPositions(data);
                var normals = primitives.GetNormals(data, positions.Length);
                var texCoords0 = primitives.GetTexCoords0(data, positions.Length);
                var texCoords1 = primitives.GetTexCoords1(data, positions.Length);
                var colors = primitives.GetColors(data, positions.Length);
                var jointsGetter = primitives.GetJoints(data, positions.Length);
                var weightsGetter = primitives.GetWeights(data, positions.Length);

                CheckAttributeUsages(primitives);

                for (var i = 0; i < positions.Length; ++i)
                {
                    var position = inverter.InvertVector3(positions[i]);
                    var normal = normals != null ? inverter.InvertVector3(normals[i]) : Vector3.zero;
                    var texCoord0 = Vector2.zero;
                    if (texCoords0 != null)
                    {
                        if (data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16))
                        {
#pragma warning disable 0612
                            texCoord0 = texCoords0[i].ReverseY();
#pragma warning restore 0612
                        }
                        else
                        {
                            texCoord0 = texCoords0[i].ReverseUV();
                        }
                    }

                    var texCoord1 = texCoords1 != null ? texCoords1[i].ReverseUV() : Vector2.zero;
                    var color = colors != null ? colors[i] : Color.black;
                    var joints = jointsGetter?.Invoke(i) ?? (0, 0, 0, 0);
                    var weights = weightsGetter != null ? NormalizeBoneWeight(weightsGetter(i)) : (0, 0, 0, 0);

                    _vertices.Add(new MeshVertex(
                        position,
                        normal,
                        texCoord0,
                        texCoord1,
                        color,
                        joints.x,
                        joints.y,
                        joints.z,
                        joints.w,
                        weights.x,
                        weights.y,
                        weights.z,
                        weights.w
                    ));
                }

                // blendshape
                if (primitives.targets != null && primitives.targets.Count > 0)
                {
                    _blendShapes.AddRange(primitives.targets.Select((x, i) => new BlendShape(i.ToString())));
                    for (int i = 0; i < primitives.targets.Count; ++i)
                    {
                        //var name = string.Format("target{0}", i++);
                        var primTarget = primitives.targets[i];
                        var blendShape = _blendShapes[i];

                        if (primTarget.POSITION != -1)
                        {
                            blendShape.Positions.Assign(
                                data.GetArrayFromAccessor<Vector3>(primTarget.POSITION), inverter.InvertVector3);
                        }

                        if (primTarget.NORMAL != -1)
                        {
                            blendShape.Normals.Assign(
                                data.GetArrayFromAccessor<Vector3>(primTarget.NORMAL), inverter.InvertVector3);
                        }

                        if (primTarget.TANGENT != -1)
                        {
                            blendShape.Tangents.Assign(
                                data.GetArrayFromAccessor<Vector3>(primTarget.TANGENT), inverter.InvertVector3);
                        }
                    }
                }
            }

            foreach (var primitive in gltfMesh.primitives)
            {
                if (primitive.indices == -1)
                {
                    var indexOffset = _indices.Count;
                    _indices.AddRange(TriangleUtil.FlipTriangle(Enumerable.Range(0, _vertices.Count)));
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, _vertices.Count));
                }
                else
                {
                    var indexOffset = _indices.Count;
                    var indices = data.GetIndices(primitive.indices);
                    _indices.AddRange(indices);
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, indices.Length));
                }

                // material
                _materialIndices.Add(primitive.material);
            }
        }

        public void RenameBlendShape(glTFMesh gltfMesh)
        {
            if (!gltf_mesh_extras_targetNames.TryGet(gltfMesh, out var targetNames)) return;
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

        private static void Truncate<T>(List<T> list, int maxIndex)
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

        public void AddDefaultMaterial()
        {
            if (!_materialIndices.Any())
            {
                // add default material
                _materialIndices.Add(0);
            }
        }

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/610
        ///
        /// VertexBuffer の後ろに未使用頂点がある場合に削除する
        /// </summary>
        public void DropUnusedVertices()
        {
            Profiler.BeginSample("MeshContext.DropUnusedVertices");
            var maxIndex = _indices.Max();
            Truncate(_vertices, maxIndex);
            foreach (var blendShape in _blendShapes)
            {
                Truncate(blendShape.Positions, maxIndex);
                Truncate(blendShape.Normals, maxIndex);
                Truncate(blendShape.Tangents, maxIndex);
            }
            Profiler.EndSample();
        }
    }
}