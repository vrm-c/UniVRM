using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UniGLTF
{
    internal class MeshData : IDisposable
    {
        private NativeArray<MeshVertex> _vertices;
        public NativeArray<MeshVertex> Vertices => _vertices.GetSubArray(0, _currentVertexCount);
        int _currentVertexCount = 0;

        private NativeArray<SkinnedMeshVertex> _skinnedMeshVertices;
        public NativeArray<SkinnedMeshVertex> SkinnedMeshVertices => _skinnedMeshVertices.GetSubArray(0, _currentSkinCount);
        int _currentSkinCount = 0;

        private NativeArray<int> _indices;
        public NativeArray<int> Indices => _indices.GetSubArray(0, _currentIndexCount);
        int _currentIndexCount = 0;

        private readonly List<SubMeshDescriptor> _subMeshes = new List<SubMeshDescriptor>();
        public IReadOnlyList<SubMeshDescriptor> SubMeshes => _subMeshes;

        private readonly List<int> _materialIndices = new List<int>();
        public IReadOnlyList<int> MaterialIndices => _materialIndices;

        private readonly List<BlendShape> _blendShapes = new List<BlendShape>();
        public IReadOnlyList<BlendShape> BlendShapes => _blendShapes;

        public bool HasNormal { get; private set; }
        public string Name { get; private set; }
        public bool ShouldSetRendererNodeAsBone { get; private set; }

        public MeshData(int vertexCapacity, int indexCapacity)
        {
            _vertices = new NativeArray<MeshVertex>(vertexCapacity, Allocator.Persistent);
            _skinnedMeshVertices = new NativeArray<SkinnedMeshVertex>(vertexCapacity, Allocator.Persistent);
            _indices = new NativeArray<int>(indexCapacity, Allocator.Persistent);
        }

        public void Dispose()
        {
            _vertices.Dispose();
            _skinnedMeshVertices.Dispose();
            _indices.Dispose();
        }

        void Clear()
        {
            _currentVertexCount = 0;
            _currentSkinCount = 0;
            _currentIndexCount = 0;
            _subMeshes.Clear();
            _materialIndices.Clear();
            _blendShapes.Clear();
            Name = null;
            HasNormal = false;
            ShouldSetRendererNodeAsBone = false;
        }

        /// <summary>
        /// バッファ共有方式(vrm-0.x)の判定。
        /// import の後方互換性のためで、vrm-1.0 export では使いません。
        /// 
        /// バッファ共用方式は連結済みの VertexBuffer を共有して、SubMeshの index buffer による参照がスライドしていく方式
        /// 
        /// * バッファがひとつのとき
        /// * すべての primitive の attribute が 同一の accessor を使用している時
        /// 
        /// </summary>
        public static bool HasSharedVertexBuffer(glTFMesh gltfMesh)
        {
            glTFAttributes lastAttributes = null;
            foreach (var prim in gltfMesh.primitives)
            {
                if (lastAttributes != null && !prim.attributes.Equals(lastAttributes))
                {
                    return false;
                }
                lastAttributes = prim.attributes;
            }
            return true;
        }

        /// <summary>
        /// glTF から 頂点バッファと index バッファ、BlendShape を蓄える。
        /// 右手系と左手系の反転(ZもしくはX軸の反転)も実行する。
        /// </summary>
        public void LoadFromGltf(GltfData data, int meshIndex, IAxisInverter inverter)
        {
            Profiler.BeginSample("MeshData.CreateFromGltf");
            Clear();

            var gltfMesh = data.GLTF.meshes[meshIndex];

            var name = gltfMesh.name;
            if (string.IsNullOrEmpty(name))
            {
                name = $"UniGLTF import#{meshIndex}";
            }
            Name = name;

            if (HasSharedVertexBuffer(gltfMesh))
            {
                ImportMeshSharingVertexBuffer(data, gltfMesh, inverter);
            }
            else
            {
                ImportMeshIndependentVertexBuffer(data, gltfMesh, inverter);
            }

            RenameBlendShape(gltfMesh);

            DropUnusedVertices();

            AddDefaultMaterial();

            Profiler.EndSample();
        }

        private void AddIndex(int i)
        {
            _indices[_currentIndexCount] = i;
            _currentIndexCount += 1;
        }

        /// <summary>
        /// * flip triangle(gltfとtriangleの CW と CCW が異なる)
        /// * add submesh offset(gltfのprimitiveは、頂点バッファが分かれているので連結。連結すると index が変わる(offset))
        /// </summary>
        private void PushIndices(BufferAccessor src, int offset)
        {
            switch (src.ComponentType)
            {
                case AccessorValueType.UNSIGNED_BYTE:
                    {
                        var indices = src.Bytes;
                        for (int i = 0; i < src.Count; i += 3)
                        {
                            AddIndex(offset + indices[i + 2]);
                            AddIndex(offset + indices[i + 1]);
                            AddIndex(offset + indices[i]);
                        }
                    }
                    break;

                case AccessorValueType.UNSIGNED_SHORT:
                    {
                        var indices = src.Bytes.Reinterpret<ushort>(1);
                        for (int i = 0; i < src.Count; i += 3)
                        {
                            AddIndex(offset + indices[i + 2]);
                            AddIndex(offset + indices[i + 1]);
                            AddIndex(offset + indices[i]);
                        }
                    }
                    break;

                case AccessorValueType.UNSIGNED_INT:
                    {
                        // たぶん int で OK
                        var indices = src.Bytes.Reinterpret<int>(1);
                        for (int i = 0; i < src.Count; i += 3)
                        {
                            AddIndex(offset + indices[i + 2]);
                            AddIndex(offset + indices[i + 1]);
                            AddIndex(offset + indices[i]);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static (int VertexCapacity, int IndexCapacity) GetCapacity(GltfData data, glTFMesh gltfMesh)
        {
            var vertexCount = 0;
            var indexCount = 0;
            foreach (var primitive in gltfMesh.primitives)
            {
                var positions = data.GLTF.accessors[primitive.attributes.POSITION];
                vertexCount += positions.count;

                if (primitive.indices == -1)
                {
                    indexCount += positions.count;
                }
                else
                {
                    var accessor = data.GLTF.accessors[primitive.indices];
                    indexCount += accessor.count;
                }
            }
            return (vertexCount, indexCount);
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

        private void RenameBlendShape(glTFMesh gltfMesh)
        {
            if (!gltf_mesh_extras_targetNames.TryGet(gltfMesh, out var targetNames)) return;
            for (var i = 0; i < _blendShapes.Count; i++)
            {
                if (i >= targetNames.Count)
                {
                    Debug.LogWarning($"invalid primitive.extras.targetNames length");
                    break;
                }

                _blendShapes[i].Name = targetNames[i];
            }
        }

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/610
        ///
        /// VertexBuffer の後ろに未使用頂点がある場合に削除する
        /// </summary>
        private void DropUnusedVertices()
        {
            Profiler.BeginSample("MeshData.DropUnusedVertices");
            var maxIndex = Indices.Max();
            if (maxIndex + 1 < _currentVertexCount)
            {
                _currentVertexCount = maxIndex + 1;
            }
            if (maxIndex + 1 < _currentSkinCount)
            {
                _currentSkinCount = maxIndex + 1;
            }
            foreach (var blendShape in _blendShapes)
            {
                Truncate(blendShape.Positions, maxIndex);
                Truncate(blendShape.Normals, maxIndex);
                Truncate(blendShape.Tangents, maxIndex);
            }
            Profiler.EndSample();
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

        private void AddDefaultMaterial()
        {
            if (!_materialIndices.Any())
            {
                // add default material
                _materialIndices.Add(0);
            }
        }

        private void AddVertex(MeshVertex vertex)
        {
            _vertices[_currentVertexCount] = vertex;
            _currentVertexCount += 1;
        }

        private void AddSkin(SkinnedMeshVertex skin)
        {
            _skinnedMeshVertices[_currentSkinCount] = skin;
            _currentSkinCount += 1;
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
        private void ImportMeshIndependentVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            bool isOldVersion = data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16);

            foreach (var primitives in gltfMesh.primitives)
            {
                var vertexOffset = _currentVertexCount;
                var indexBufferCount = primitives.indices;

                // position は必ず存在する。normal, texCoords, colors, skinning は無いかもしれない
                var positions = primitives.GetPositions(data);
                var normals = primitives.GetNormals(data, positions.Length);
                if (normals.HasValue)
                {
                    HasNormal = true;
                }
                var texCoords0 = primitives.GetTexCoords0(data, positions.Length);
                var texCoords1 = primitives.GetTexCoords1(data, positions.Length);
                var colors = primitives.GetColors(data, positions.Length);
                var skinning = SkinningInfo.Create(data, gltfMesh, primitives);
                ShouldSetRendererNodeAsBone = skinning.ShouldSetRendererNodeAsBone;

                for (var i = 0; i < positions.Length; ++i)
                {
                    var position = inverter.InvertVector3(positions[i]);
                    var normal = normals != null ? inverter.InvertVector3(normals.Value[i]) : Vector3.zero;

                    var texCoord0 = Vector2.zero;
                    if (texCoords0 != null)
                    {
                        if (isOldVersion)
                        {
#pragma warning disable 0612
                            // backward compatibility
                            texCoord0 = texCoords0.Value[i].ReverseY();
#pragma warning restore 0612
                        }
                        else
                        {
                            texCoord0 = texCoords0.Value[i].ReverseUV();
                        }
                    }

                    var texCoord1 = texCoords1 != null ? texCoords1.Value[i].ReverseUV() : Vector2.zero;

                    var color = colors != null ? colors.Value[i] : Color.white;
                    AddVertex(
                        new MeshVertex(
                            position,
                            normal,
                            texCoord0,
                            texCoord1,
                            color
                        ));
                    var skin = skinning.GetSkinnedVertex(i);
                    if (skin.HasValue)
                    {
                        AddSkin(skin.Value);
                    }
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
                    var indexOffset = _currentIndexCount;
                    var dataIndices = data.GetIndicesFromAccessorIndex(indexBufferCount);
                    PushIndices(dataIndices, vertexOffset);
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, dataIndices.Count));
                }
                else
                {
                    var indexOffset = _currentIndexCount;
                    for (int i = 0; i < positions.Count(); i += 3)
                    {
                        // flip triangle
                        AddIndex(i + vertexOffset + 2);
                        AddIndex(i + vertexOffset + 1);
                        AddIndex(i + vertexOffset);
                    }
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, positions.Count()));
                }

                // material
                _materialIndices.Add(primitives.material);
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
        private void ImportMeshSharingVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            var isOldVersion = data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16);

            {
                // すべての primitives で連結済みの VertexBuffer を共有している。代表して先頭を使う                
                var primitives = gltfMesh.primitives.First();

                var positions = primitives.GetPositions(data);
                var normals = primitives.GetNormals(data, positions.Length);
                if (normals.HasValue)
                {
                    HasNormal = true;
                }
                var texCoords0 = primitives.GetTexCoords0(data, positions.Length);
                var texCoords1 = primitives.GetTexCoords1(data, positions.Length);
                var colors = primitives.GetColors(data, positions.Length);
                var skinning = SkinningInfo.Create(data, gltfMesh, primitives);
                ShouldSetRendererNodeAsBone = skinning.ShouldSetRendererNodeAsBone;

                for (var i = 0; i < positions.Length; ++i)
                {
                    var position = inverter.InvertVector3(positions[i]);
                    var normal = normals != null ? inverter.InvertVector3(normals.Value[i]) : Vector3.zero;
                    var texCoord0 = Vector2.zero;
                    if (texCoords0 != null)
                    {
                        if (isOldVersion)
                        {
#pragma warning disable 0612
                            texCoord0 = texCoords0.Value[i].ReverseY();
#pragma warning restore 0612
                        }
                        else
                        {
                            texCoord0 = texCoords0.Value[i].ReverseUV();
                        }
                    }

                    var texCoord1 = texCoords1 != null ? texCoords1.Value[i].ReverseUV() : Vector2.zero;
                    var color = colors != null ? colors.Value[i] : Color.white;

                    AddVertex(
                        new MeshVertex(
                            position,
                            normal,
                            texCoord0,
                            texCoord1,
                            color));
                    var skin =
                        skinning.GetSkinnedVertex(i);
                    if (skin.HasValue)
                    {
                        AddSkin(skin.Value);
                    }
                }

                // blendshape
                if (primitives.targets != null && primitives.targets.Count > 0)
                {
                    for (int i = 0; i < primitives.targets.Count; ++i)
                    {
                        var primTarget = primitives.targets[i];

                        var hasPosition = primTarget.POSITION != -1 && data.GLTF.accessors[primTarget.POSITION].count == positions.Length;
                        var hasNormal = primTarget.NORMAL != -1 && data.GLTF.accessors[primTarget.NORMAL].count == positions.Length;
                        var hasTangent = primTarget.TANGENT != -1 && data.GLTF.accessors[primTarget.TANGENT].count == positions.Length;

                        var blendShape = new BlendShape(i.ToString(), positions.Length, hasPosition, hasNormal, hasTangent);
                        _blendShapes.Add(blendShape);

                        if (hasPosition)
                        {
                            var morphPositions = data.GetArrayFromAccessor<Vector3>(primTarget.POSITION);
                            blendShape.Positions.Capacity = morphPositions.Length;
                            for (var j = 0; j < positions.Length; ++j)
                            {
                                blendShape.Positions.Add(inverter.InvertVector3(morphPositions[j]));
                            }
                        }

                        if (hasNormal)
                        {
                            var morphNormals = data.GetArrayFromAccessor<Vector3>(primTarget.NORMAL);
                            blendShape.Normals.Capacity = morphNormals.Length;
                            for (var j = 0; j < positions.Length; ++j)
                            {
                                blendShape.Normals.Add(inverter.InvertVector3(morphNormals[j]));
                            }

                        }

                        if (hasTangent)
                        {
                            var morphTangents = data.GetArrayFromAccessor<Vector3>(primTarget.TANGENT);
                            blendShape.Tangents.Capacity = morphTangents.Length;
                            for (var j = 0; j < positions.Length; ++j)
                            {
                                blendShape.Tangents.Add(inverter.InvertVector3(morphTangents[j]));
                            }
                        }
                    }
                }
            }

            foreach (var primitive in gltfMesh.primitives)
            {
                if (primitive.indices >= 0)
                {
                    var indexOffset = _currentIndexCount;
                    var indices = data.GetIndicesFromAccessorIndex(primitive.indices);
                    PushIndices(indices, 0);
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, indices.Count));
                }
                else
                {
                    var indexOffset = _currentIndexCount;
                    var positions = data.GLTF.accessors[primitive.attributes.POSITION];
                    for (int i = 0; i < positions.count; i += 3)
                    {
                        // flip triangle
                        AddIndex(i + 2);
                        AddIndex(i + 1);
                        AddIndex(i);
                    }
                    _subMeshes.Add(new SubMeshDescriptor(indexOffset, positions.count));
                }

                // material
                _materialIndices.Add(primitive.material);
            }
        }
    }
}