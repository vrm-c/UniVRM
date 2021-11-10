using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public class MeshContext
    {
        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        [Obsolete] private readonly List<Vector4> _tangents = new List<Vector4>();
        private readonly List<Vector2> _uv = new List<Vector2>();
        private readonly List<Vector2> _uv2 = new List<Vector2>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<BoneWeight> _boneWeights = new List<BoneWeight>();
        private readonly List<int[]> _subMeshes = new List<int[]>();
        private readonly List<int> _materialIndices = new List<int>();
        private readonly List<BlendShape> _blendShapes = new List<BlendShape>();

        public IReadOnlyList<Vector3> Positions => _positions;
        public IReadOnlyList<Vector3> Normals => _normals;

        [Obsolete] public IReadOnlyList<Vector4> Tangetns => _tangents;

        public IReadOnlyList<Vector2> UV => _uv;

        public IReadOnlyList<Vector2> UV2 => _uv2;
        public IReadOnlyList<Color> Colors => _colors;

        public IReadOnlyList<BoneWeight> BoneWeights => _boneWeights;

        public IReadOnlyList<int[]> SubMeshes => _subMeshes;

        public IReadOnlyList<int> MaterialIndices => _materialIndices;

        public IReadOnlyList<BlendShape> BlendShapes => _blendShapes;

        public string Name { get; }

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

            this.Name = name;
        }

        /// <summary>
        /// Fill list with 0s with the specified length
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fillLength"></param>
        /// <typeparam name="T"></typeparam>
        private static void FillZero<T>(ICollection<T> list, int fillLength)
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

        private static BoneWeight NormalizeBoneWeight(BoneWeight src)
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
        public void ImportMeshIndependentVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            foreach (var prim in gltfMesh.primitives)
            {
                var indexOffset = _positions.Count;
                var indexBuffer = prim.indices;

                // position は必ずある
                var positions = data.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION);
                _positions.AddRange(positions.Select(inverter.InvertVector3));
                var fillLength = _positions.Count;

                // normal
                if (prim.attributes.NORMAL != -1)
                {
                    var normals = data.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL);
                    if (normals.Length != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    _normals.AddRange(normals.Select(inverter.InvertVector3));
                    FillZero(_normals, fillLength);
                }

                // uv
                if (prim.attributes.TEXCOORD_0 != -1)
                {
                    var uvs = data.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0);
                    if (uvs.Length != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    if (data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16))
                    {
#pragma warning disable 0612
                        // backward compatibility
                        _uv.AddRange(uvs.Select(x => x.ReverseY()));
                        FillZero(_uv, fillLength);
#pragma warning restore 0612
                    }
                    else
                    {
                        _uv.AddRange(uvs.Select(x => x.ReverseUV()));
                        FillZero(_uv, fillLength);
                    }
                }

                // uv2
                if (prim.attributes.TEXCOORD_1 != -1)
                {
                    var uvs = data.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1);
                    if (uvs.Length != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    _uv2.AddRange(uvs.Select(x => x.ReverseUV()));
                    FillZero(_uv2, fillLength);
                }

                // color
                if (prim.attributes.COLOR_0 != -1)
                {
                    var colors = data.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0);
                    if (colors.Length != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    _colors.AddRange(colors);
                    FillZero(_colors, fillLength);
                }

                // skin
                if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                {
                    var (joints0, jointsLength) = JointsAccessor.GetAccessor(data, prim.attributes.JOINTS_0);
                    var (weights0, weightsLength) = WeightsAccessor.GetAccessor(data, prim.attributes.WEIGHTS_0);
                    if (jointsLength != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    if (weightsLength != positions.Length)
                    {
                        throw new Exception("different length");
                    }

                    for (var j = 0; j < jointsLength; ++j)
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

                        _boneWeights.Add(bw);
                    }

                    FillZero(_boneWeights, fillLength);
                }

                // blendshape
                if (prim.targets != null && prim.targets.Count > 0)
                {
                    for (var i = 0; i < prim.targets.Count; ++i)
                    {
                        var primTarget = prim.targets[i];
                        var blendShape = GetOrCreateBlendShape(i);
                        if (primTarget.POSITION != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.POSITION);
                            if (array.Length != positions.Length)
                            {
                                throw new Exception("different length");
                            }

                            blendShape.Positions.AddRange(array.Select(inverter.InvertVector3).ToArray());
                            FillZero(blendShape.Positions, fillLength);
                        }

                        if (primTarget.NORMAL != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.NORMAL);
                            if (array.Length != positions.Length)
                            {
                                throw new Exception("different length");
                            }

                            blendShape.Normals.AddRange(array.Select(inverter.InvertVector3).ToArray());
                            FillZero(blendShape.Normals, fillLength);
                        }

                        if (primTarget.TANGENT != -1)
                        {
                            var array = data.GetArrayFromAccessor<Vector3>(primTarget.TANGENT);
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
                            ? data.GetIndices(indexBuffer)
                            : TriangleUtil.FlipTriangle(Enumerable.Range(0, _positions.Count))
                                .ToArray() // without index array
                    ;
                for (var i = 0; i < indices.Length; ++i)
                {
                    indices[i] += indexOffset;
                }

                _subMeshes.Add(indices);

                // material
                _materialIndices.Add(prim.material);
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
        public void ImportMeshSharingVertexBuffer(GltfData data, glTFMesh gltfMesh, IAxisInverter inverter)
        {
            {
                //  同じVertexBufferを共有しているので先頭のモノを使う
                var prim = gltfMesh.primitives.First();
                _positions.AddRange(data.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION)
                    .SelectInplace(inverter.InvertVector3));

                // normal
                if (prim.attributes.NORMAL != -1)
                {
                    _normals.AddRange(data.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL)
                        .SelectInplace(inverter.InvertVector3));
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
                    if (data.GLTF.IsGeneratedUniGLTFAndOlder(1, 16))
                    {
#pragma warning disable 0612
                        // backward compatibility
                        _uv.AddRange(data.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0)
                            .SelectInplace(x => x.ReverseY()));
#pragma warning restore 0612
                    }
                    else
                    {
                        _uv.AddRange(data.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0)
                            .SelectInplace(x => x.ReverseUV()));
                    }
                }

                // uv2
                if (prim.attributes.TEXCOORD_1 != -1)
                {
                    _uv2.AddRange(data.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_1)
                        .SelectInplace(x => x.ReverseUV()));
                }

                // color
                if (prim.attributes.COLOR_0 != -1)
                {
                    switch (data.GLTF.accessors[prim.attributes.COLOR_0].TypeCount)
                    {
                        case 3:
                        {
                            var vec3Color = data.GetArrayFromAccessor<Vector3>(prim.attributes.COLOR_0);
                            _colors.AddRange(new Color[vec3Color.Length]);

                            for (var i = 0; i < vec3Color.Length; i++)
                            {
                                var color = vec3Color[i];
                                _colors[i] = new Color(color.x, color.y, color.z);
                            }

                            break;
                        }
                        case 4:
                            _colors.AddRange(data.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                            break;
                        default:
                            throw new NotImplementedException(
                                $"unknown color type {data.GLTF.accessors[prim.attributes.COLOR_0].type}");
                    }
                }

                // skin
                if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                {
                    var (joints0, jointsLength) = JointsAccessor.GetAccessor(data, prim.attributes.JOINTS_0);
                    var (weights0, weightsLength) = WeightsAccessor.GetAccessor(data, prim.attributes.WEIGHTS_0);

                    for (var j = 0; j < jointsLength; ++j)
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

                        _boneWeights.Add(bw);
                    }
                }

                // blendshape
                if (prim.targets != null && prim.targets.Count > 0)
                {
                    _blendShapes.AddRange(prim.targets.Select((x, i) => new BlendShape(i.ToString())));
                    for (int i = 0; i < prim.targets.Count; ++i)
                    {
                        //var name = string.Format("target{0}", i++);
                        var primTarget = prim.targets[i];
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

            foreach (var prim in gltfMesh.primitives)
            {
                if (prim.indices == -1)
                {
                    _subMeshes.Add(TriangleUtil.FlipTriangle(Enumerable.Range(0, _positions.Count)).ToArray());
                }
                else
                {
                    var indices = data.GetIndices(prim.indices);
                    _subMeshes.Add(indices);
                }

                // material
                _materialIndices.Add(prim.material);
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

        //
        // https://github.com/vrm-c/UniVRM/issues/610
        //
        // VertexBuffer の後ろに未使用頂点がある場合に削除する
        //
        public void DropUnusedVertices()
        {
            var maxIndex = _subMeshes.SelectMany(x => x).Max();
            Truncate(_positions, maxIndex);
            Truncate(_normals, maxIndex);
            Truncate(_uv, maxIndex);
            Truncate(_uv2, maxIndex);
            Truncate(_colors, maxIndex);
            Truncate(_boneWeights, maxIndex);
#if false
                Truncate(m_tangents, maxIndex);
#endif
            foreach (var blendshape in _blendShapes)
            {
                Truncate(blendshape.Positions, maxIndex);
                Truncate(blendshape.Normals, maxIndex);
                Truncate(blendshape.Tangents, maxIndex);
            }
        }
    }
}