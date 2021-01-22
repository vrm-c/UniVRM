using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using VrmLib;
using UniJSON;


namespace UniVRM10
{
    public class Vrm10Storage : IVrmStorage
    {
        public ArraySegment<Byte> OriginalJson { get; private set; }
        public UniGLTF.glTF Gltf
        {
            get;
            private set;
        }

        public readonly List<ArrayByteBuffer10> Buffers;

        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm gltfVrm;

        public UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone;

        /// <summary>
        /// for export
        /// </summary>
        public Vrm10Storage()
        {
            Gltf = new UniGLTF.glTF()
            {
                extensionsUsed = new List<string>(),
            };
            Buffers = new List<ArrayByteBuffer10>()
            {
                new ArrayByteBuffer10()
            };
        }

        /// <summary>
        /// for import
        /// </summary>
        /// <param name="json"></param>
        /// <param name="bin"></param>
        public Vrm10Storage(ArraySegment<byte> json, ArraySegment<byte> bin)
        {
            OriginalJson = json;
            Gltf = UniGLTF.GltfDeserializer.Deserialize(json.ParseAsJson());

            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(Gltf.extensions,
                out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                gltfVrm = vrm;
            }

            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Gltf.extensions,
                out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                gltfVrmSpringBone = springBone;
            }

            var array = bin.ToArray();
            Buffers = new List<ArrayByteBuffer10>()
            {
                new ArrayByteBuffer10(array, bin.Count)
            };
        }

        public void Reserve(int bytesLength)
        {
            Buffers[0].ExtendCapacity(bytesLength);
        }

        public int AppendToBuffer(int bufferIndex, ArraySegment<byte> segment, int stride)
        {
            Buffers[bufferIndex].Extend(segment, stride, out int offset, out int length);
            var viewIndex = Gltf.bufferViews.Count;
            Gltf.bufferViews.Add(new UniGLTF.glTFBufferView
            {
                buffer = 0,
                byteOffset = offset,
                byteLength = length,
            });
            return viewIndex;
        }

        static ArraySegment<byte> RestoreSparseAccessorUInt16<T>(ArraySegment<byte> bytes, int accessorCount, ArraySegment<byte> indicesBytes, ArraySegment<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Count == 0)
            {
                bytes = new ArraySegment<byte>(new byte[accessorCount * stride]);
            }
            var dst = SpanLike.Wrap<T>(bytes);

            var indices = SpanLike.Wrap<UInt16>(indicesBytes);
            var values = SpanLike.Wrap<T>(valuesBytes);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        static ArraySegment<byte> RestoreSparseAccessorUInt32<T>(ArraySegment<byte> bytes, int accessorCount, ArraySegment<byte> indicesBytes, ArraySegment<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Count == 0)
            {
                bytes = new ArraySegment<byte>(new byte[accessorCount * stride]);
            }
            var dst = SpanLike.Wrap<T>(bytes);

            var indices = SpanLike.Wrap<Int32>(indicesBytes);
            var values = SpanLike.Wrap<T>(valuesBytes);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        public ArraySegment<byte> GetAccessorBytes(int accessorIndex)
        {
            var accessor = Gltf.accessors[accessorIndex];
            var sparse = accessor.sparse;

            ArraySegment<byte> bytes = default(ArraySegment<byte>);

            if (accessor.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int bufferViewIndex))
            {
                var view = Gltf.bufferViews[bufferViewIndex];
                if (view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferIndex))
                {
                    var buffer = Buffers[bufferIndex];
                    var bin = buffer.Bytes;
                    var byteSize = accessor.CalcByteSize();
                    bytes = bin.Slice(view.byteOffset, view.byteLength).Slice(accessor.byteOffset, byteSize);
                }
            }

            if (sparse != null)
            {
                if (!sparse.indices.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int sparseIndicesBufferViewIndex))
                {
                    throw new Exception();
                }
                var sparseIndexView = Gltf.bufferViews[sparseIndicesBufferViewIndex];
                var sparseIndexBin = GetBufferBytes(sparseIndexView);
                var sparseIndexBytes = sparseIndexBin
                    .Slice(sparseIndexView.byteOffset, sparseIndexView.byteLength)
                    .Slice(sparse.indices.byteOffset, ((AccessorValueType)sparse.indices.componentType).ByteSize() * sparse.count)
                    ;

                if (!sparse.values.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int sparseValuesBufferViewIndex))
                {
                    throw new Exception();
                }
                var sparseValueView = Gltf.bufferViews[sparseValuesBufferViewIndex];
                var sparseValueBin = GetBufferBytes(sparseValueView);
                var sparseValueBytes = sparseValueBin
                    .Slice(sparseValueView.byteOffset, sparseValueView.byteLength)
                    .Slice(sparse.values.byteOffset, accessor.GetStride() * sparse.count);
                ;

                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_SHORT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt16<Vector3>(bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_INT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt32<Vector3>(bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (bytes.Count == 0)
                {
                    // sparse and all value is zero
                    return new ArraySegment<byte>(new byte[accessor.GetStride() * accessor.count]);
                }

                return bytes;
            }
        }

        /// <summary>
        /// submeshのindexが連続した領域に格納されているかを確認する
        /// </summary>
        bool AccessorsIsContinuous(int[] accessorIndices)
        {
            var firstAccessor = Gltf.accessors[accessorIndices[0]];
            var firstView = Gltf.bufferViews[firstAccessor.bufferView];
            var start = firstView.byteOffset + firstAccessor.byteOffset;
            var pos = start;
            foreach (var i in accessorIndices)
            {
                var current = Gltf.accessors[i];
                if (current.type != "SCALAR")
                {
                    throw new ArgumentException($"accessor.type: {current.type}");
                }
                if (firstAccessor.componentType != current.componentType)
                {
                    return false;
                }

                var view = Gltf.bufferViews[current.bufferView];
                if (pos != view.byteOffset + current.byteOffset)
                {
                    return false;
                }

                var begin = view.byteOffset + current.byteOffset;
                var byteLength = current.CalcByteSize();

                pos += byteLength;
            }

            return true;
        }

        /// <summary>
        /// Gltfの Primitive[] の indices をひとまとめにした
        /// IndexBuffer を返す。
        /// </summary>
        public BufferAccessor CreateAccessor(int[] accessorIndices)
        {
            var totalCount = accessorIndices.Sum(x => Gltf.accessors[x].count);
            if (AccessorsIsContinuous(accessorIndices))
            {
                // IndexBufferが連続して格納されている => Slice でいける
                var firstAccessor = Gltf.accessors[accessorIndices[0]];
                var firstView = Gltf.bufferViews[firstAccessor.bufferView];
                var start = firstView.byteOffset + firstAccessor.byteOffset;
                if (!firstView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int firstViewBufferIndex))
                {
                    throw new Exception();
                }
                var buffer = Gltf.buffers[firstViewBufferIndex];
                var bin = GetBufferBytes(buffer);
                var bytes = bin.Slice(start, totalCount * firstAccessor.GetStride());
                return new BufferAccessor(bytes,
                    (AccessorValueType)firstAccessor.componentType,
                    EnumUtil.Parse<AccessorVectorType>(firstAccessor.type),
                    totalCount);
            }
            else
            {
                // IndexBufferが連続して格納されていない => Int[] を作り直す
                var indices = new byte[totalCount * Marshal.SizeOf(typeof(int))];
                var span = SpanLike.Wrap<Int32>(new ArraySegment<byte>(indices));
                var offset = 0;
                foreach (var accessorIndex in accessorIndices)
                {
                    var accessor = Gltf.accessors[accessorIndex];
                    if (accessor.type != "SCALAR")
                    {
                        throw new ArgumentException($"accessor.type: {accessor.type}");
                    }
                    var view = Gltf.bufferViews[accessor.bufferView];
                    if (!view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int viewBufferIndex))
                    {
                        throw new Exception();
                    }
                    var buffer = Gltf.buffers[viewBufferIndex];
                    var bin = GetBufferBytes(buffer);
                    var start = view.byteOffset + accessor.byteOffset;
                    var bytes = bin.Slice(start, accessor.count * accessor.GetStride());
                    var dst = SpanLike.Wrap<Int32>(new ArraySegment<byte>(indices)).Slice(offset, accessor.count);
                    offset += accessor.count;
                    switch ((AccessorValueType)accessor.componentType)
                    {
                        case AccessorValueType.UNSIGNED_BYTE:
                            {
                                var src = SpanLike.Wrap<Byte>(bytes);
                                for (int i = 0; i < src.Length; ++i)
                                {
                                    // byte to int
                                    dst[i] = src[i];
                                }
                            }
                            break;

                        case AccessorValueType.UNSIGNED_SHORT:
                            {
                                var src = SpanLike.Wrap<UInt16>(bytes);
                                for (int i = 0; i < src.Length; ++i)
                                {
                                    // ushort to int
                                    dst[i] = src[i];
                                }
                            }
                            break;

                        case AccessorValueType.UNSIGNED_INT:
                            {
                                Buffer.BlockCopy(bytes.Array, bytes.Offset, dst.Bytes.Array, dst.Bytes.Offset, bytes.Count);
                            }
                            break;

                        default:
                            throw new NotImplementedException($"accessor.componentType: {accessor.componentType}");
                    }
                }
                return new BufferAccessor(new ArraySegment<byte>(indices), AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, totalCount);
            }
        }

        public void CreateBufferAccessorAndAdd(int? accessorIndex, VertexBuffer b, string key)
        {
            if (accessorIndex.HasValue)
            {
                CreateBufferAccessorAndAdd(accessorIndex.Value, b, key);
            }
        }

        public void CreateBufferAccessorAndAdd(int accessorIndex, VertexBuffer b, string key)
        {
            var a = CreateAccessor(accessorIndex);
            if (a != null)
            {
                b.Add(key, a);
            }
        }

        public BufferAccessor CreateAccessor(int accessorIndex)
        {
            if (TryCreateAccessor(accessorIndex, out BufferAccessor ba))
            {
                return ba;
            }
            else
            {
                return null;
            }
        }

        public bool TryCreateAccessor(int accessorIndex, out BufferAccessor ba)
        {
            if (accessorIndex < 0 || accessorIndex >= Gltf.accessors.Count)
            {
                ba = default;
                return false;
            }
            var accessor = Gltf.accessors[accessorIndex];
            var bytes = GetAccessorBytes(accessorIndex);
            var vectorType = EnumUtil.Parse<AccessorVectorType>(accessor.type);
            ba = new BufferAccessor(bytes,
                (AccessorValueType)accessor.componentType, vectorType, accessor.count);
            return true;
        }

        public string AssetVersion => Gltf.asset.version;

        public string AssetMinVersion => Gltf.asset.minVersion;

        public string AssetGenerator => Gltf.asset.generator;

        public string AssetCopyright => Gltf.asset.copyright;

        public int NodeCount => Gltf.nodes.Count;

        public int ImageCount => Gltf.images.Count;

        public int TextureCount => Gltf.textures.Count;

        public int MaterialCount => Gltf.materials.Count;

        public int SkinCount => Gltf.skins.Count;

        public int MeshCount => Gltf.meshes.Count;

        // TODO:
        public int AnimationCount => 0;

        public string VrmExporterVersion => Gltf.asset.generator;

        public bool HasVrm => gltfVrm != null;

        public string VrmSpecVersion => gltfVrm?.SpecVersion;

        public Node CreateNode(int index)
        {
            var x = Gltf.nodes[index];
            var node = new Node(x.name);
            if (x.matrix != null)
            {
                if (x.matrix.Length != 16) throw new Exception("matrix member is not 16");
                if (x.translation != null && x.translation.Length > 0) throw new Exception("matrix with translation");
                if (x.rotation != null && x.rotation.Length > 0) throw new Exception("matrix with rotation");
                if (x.scale != null && x.scale.Length > 0) throw new Exception("matrix with scale");
                var m = new Matrix4x4(
                    x.matrix[0], x.matrix[1], x.matrix[2], x.matrix[3],
                    x.matrix[4], x.matrix[5], x.matrix[6], x.matrix[7],
                    x.matrix[8], x.matrix[9], x.matrix[10], x.matrix[11],
                    x.matrix[12], x.matrix[13], x.matrix[14], x.matrix[15]
                    );

                node.SetLocalMatrix(m, true);
            }
            else
            {
                node.LocalTranslation = x.translation.ToVector3();
                node.LocalRotation = x.rotation.ToQuaternion();
                node.LocalScaling = x.scale.ToVector3(Vector3.One);
            }
            return node;
        }

        public IEnumerable<int> GetChildNodeIndices(int i)
        {
            var gltfNode = Gltf.nodes[i];
            if (gltfNode.children != null)
            {
                foreach (var j in gltfNode.children)
                {
                    yield return j;
                }
            }
        }

        public Image CreateImage(int index)
        {
            return Gltf.images[index].FromGltf(this);
        }

        /// <summary>
        /// sRGB でないテクスチャーを検出する
        /// </summary>
        /// <param name="textureIndex"></param>
        /// <returns></returns>
        private (Texture.TextureTypes, UniGLTF.glTFMaterial) GetTextureType(int textureIndex)
        {
            foreach (var material in Gltf.materials)
            {
                if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(material.extensions,
                    out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
                {
                    if (material.normalTexture?.index == textureIndex) return (Texture.TextureTypes.NormalMap, material);
                }
                else if (UniGLTF.glTF_KHR_materials_unlit.IsEnable(material))
                {
                }
                else
                {
                    if (material.pbrMetallicRoughness?.baseColorTexture?.index == textureIndex) return (Texture.TextureTypes.Default, material);
                    if (material.pbrMetallicRoughness?.metallicRoughnessTexture?.index == textureIndex) return (Texture.TextureTypes.MetallicRoughness, material);
                    if (material.occlusionTexture?.index == textureIndex) return (Texture.TextureTypes.Occlusion, material);
                    if (material.emissiveTexture?.index == textureIndex) return (Texture.TextureTypes.Emissive, material);
                    if (material.normalTexture?.index == textureIndex) return (Texture.TextureTypes.NormalMap, material);
                }
            }

            return (Texture.TextureTypes.Default, null);
        }

        private Texture.ColorSpaceTypes GetTextureColorSpaceType(int textureIndex)
        {
            foreach (var material in Gltf.materials)
            {
                if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(material.extensions,
                    out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
                {
                    // mtoon
                    if (material.pbrMetallicRoughness.baseColorTexture.index == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (mtoon.ShadeMultiplyTexture == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (material.emissiveTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (mtoon.RimMultiplyTexture == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (mtoon.AdditiveTexture == textureIndex) return Texture.ColorSpaceTypes.Srgb;

                    if (mtoon.OutlineWidthMultiplyTexture == textureIndex) return Texture.ColorSpaceTypes.Linear;
                    if (mtoon.UvAnimationMaskTexture == textureIndex) return Texture.ColorSpaceTypes.Linear;

                    if (material.normalTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Linear;
                }
                else if (UniGLTF.glTF_KHR_materials_unlit.IsEnable(material))
                {
                    // unlit
                    if (material.pbrMetallicRoughness.baseColorTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                }
                else
                {
                    // Pbr
                    if (material.pbrMetallicRoughness?.baseColorTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (material.pbrMetallicRoughness?.metallicRoughnessTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Linear;
                    if (material.occlusionTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Linear;
                    if (material.emissiveTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Srgb;
                    if (material.normalTexture?.index == textureIndex) return Texture.ColorSpaceTypes.Linear;
                }
            }

            return Texture.ColorSpaceTypes.Srgb;
        }

        public Texture CreateTexture(int index, List<Image> images)
        {
            var texture = Gltf.textures[index];
            var textureType = GetTextureType(index);
            var colorSpace = GetTextureColorSpaceType(index);

            var sampler = (texture.sampler >= 0 && texture.sampler < Gltf.samplers.Count)
            ? Gltf.samplers[texture.sampler]
            : new UniGLTF.glTFTextureSampler()
            ;

            if (textureType.Item1 == Texture.TextureTypes.MetallicRoughness && textureType.Item2.pbrMetallicRoughness != null)
            {
                var roughnessFactor = textureType.Item2.pbrMetallicRoughness.roughnessFactor;
                var name = !string.IsNullOrEmpty(texture.name) ? texture.name : images[texture.source].Name;
                return new MetallicRoughnessImageTexture(
                    name,
                    sampler.FromGltf(),
                    images[texture.source],
                    roughnessFactor,
                    colorSpace,
                    textureType.Item1);
            }
            else
            {
                return texture.FromGltf(sampler, images, colorSpace, textureType.Item1);
            }
        }

        public Material CreateMaterial(int index, List<Texture> textures)
        {
            var x = Gltf.materials[index];
            return x.FromGltf(textures);
        }

        public Skin CreateSkin(int index, List<Node> nodes)
        {
            var x = Gltf.skins[index];
            BufferAccessor inverseMatrices = null;
            if (x.inverseBindMatrices != -1)
            {
                inverseMatrices = CreateAccessor(x.inverseBindMatrices);
            }
            var skin = new Skin
            {
                InverseMatrices = inverseMatrices,
                Joints = x.joints.Select(y => nodes[y]).ToList(),
            };
            if (x.skeleton != -1) // TODO: proto to int
            {
                skin.Root = nodes[x.skeleton];
            }
            return skin;
        }

        public MeshGroup CreateMesh(int index, List<Material> materials)
        {
            var x = Gltf.meshes[index];
            var group = x.FromGltf(this, materials);
            return group;
        }

        public (int, int) GetNodeMeshSkin(int index)
        {
            var x = Gltf.nodes[index];

            int meshIndex = -1;
            if (x.mesh.TryGetValidIndex(Gltf.meshes.Count, out int mi))
            {
                meshIndex = mi;
            }

            int skinIndex = -1;
            if (x.skin.TryGetValidIndex(Gltf.skins.Count, out int si))
            {
                skinIndex = si;
            }

            return (meshIndex, skinIndex);
        }

        public Animation CreateAnimation(int index, List<Node> nodes)
        {
            throw new NotImplementedException();
        }

        public Meta CreateVrmMeta(List<Texture> textures)
        {
            return gltfVrm.Meta.FromGltf(textures);
        }

        static void AssignHumanoid(List<Node> nodes, UniGLTF.Extensions.VRMC_vrm.HumanBone humanBone, VrmLib.HumanoidBones key)
        {
            if (humanBone != null && humanBone.Node.HasValue)
            {
                nodes[humanBone.Node.Value].HumanoidBone = key;
            }
        }

        public void LoadVrmHumanoid(List<Node> nodes)
        {
            if (gltfVrm.Humanoid != null)
            {
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Hips, HumanoidBones.hips);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftUpperLeg, HumanoidBones.leftUpperLeg);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightUpperLeg, HumanoidBones.rightUpperLeg);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftLowerLeg, HumanoidBones.leftLowerLeg);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightLowerLeg, HumanoidBones.rightLowerLeg);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftFoot, HumanoidBones.leftFoot);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightFoot, HumanoidBones.rightFoot);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Spine, HumanoidBones.spine);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Chest, HumanoidBones.chest);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Neck, HumanoidBones.neck);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Head, HumanoidBones.head);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftShoulder, HumanoidBones.leftShoulder);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightShoulder, HumanoidBones.rightShoulder);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftUpperArm, HumanoidBones.leftUpperArm);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightUpperArm, HumanoidBones.rightUpperArm);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftLowerArm, HumanoidBones.leftLowerArm);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightLowerArm, HumanoidBones.rightLowerArm);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftHand, HumanoidBones.leftHand);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightHand, HumanoidBones.rightHand);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftToes, HumanoidBones.leftToes);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightToes, HumanoidBones.rightToes);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftEye, HumanoidBones.leftEye);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightEye, HumanoidBones.rightEye);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.Jaw, HumanoidBones.jaw);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftThumbProximal, HumanoidBones.leftThumbProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftThumbIntermediate, HumanoidBones.leftThumbIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftThumbDistal, HumanoidBones.leftThumbDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftIndexProximal, HumanoidBones.leftIndexProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftIndexIntermediate, HumanoidBones.leftIndexIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftIndexDistal, HumanoidBones.leftIndexDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftMiddleProximal, HumanoidBones.leftMiddleProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftMiddleIntermediate, HumanoidBones.leftMiddleIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftMiddleDistal, HumanoidBones.leftMiddleDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftRingProximal, HumanoidBones.leftRingProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftRingIntermediate, HumanoidBones.leftRingIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftRingDistal, HumanoidBones.leftRingDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftLittleProximal, HumanoidBones.leftLittleProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftLittleIntermediate, HumanoidBones.leftLittleIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.LeftLittleDistal, HumanoidBones.leftLittleDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightThumbProximal, HumanoidBones.rightThumbProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightThumbIntermediate, HumanoidBones.rightThumbIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightThumbDistal, HumanoidBones.rightThumbDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightIndexProximal, HumanoidBones.rightIndexProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightIndexIntermediate, HumanoidBones.rightIndexIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightIndexDistal, HumanoidBones.rightIndexDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightMiddleProximal, HumanoidBones.rightMiddleProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightMiddleIntermediate, HumanoidBones.rightMiddleIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightMiddleDistal, HumanoidBones.rightMiddleDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightRingProximal, HumanoidBones.rightRingProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightRingIntermediate, HumanoidBones.rightRingIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightRingDistal, HumanoidBones.rightRingDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightLittleProximal, HumanoidBones.rightLittleProximal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightLittleIntermediate, HumanoidBones.rightLittleIntermediate);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.RightLittleDistal, HumanoidBones.rightLittleDistal);
                AssignHumanoid(nodes, gltfVrm.Humanoid.HumanBones.UpperChest, HumanoidBones.upperChest);
            }
        }

        public ExpressionManager CreateVrmExpression(List<MeshGroup> _, List<Material> materials, List<Node> nodes)
        {
            if (gltfVrm.Expressions != null)
            {
                var expressionManager = new ExpressionManager();
                foreach (var x in gltfVrm.Expressions)
                {
                    expressionManager.ExpressionList.Add(x.FromGltf(nodes, materials));
                }
                return expressionManager;
            }

            return null;
        }

        static VrmSpringBoneCollider CreateCollider(UniGLTF.Extensions.VRMC_node_collider.ColliderShape z)
        {
            if (z.Sphere != null)
            {
                return VrmSpringBoneCollider.CreateSphere(z.Sphere.Offset.ToVector3(), z.Sphere.Radius.Value);
            }
            if (z.Capsule != null)
            {
                return VrmSpringBoneCollider.CreateCapsule(z.Capsule.Offset.ToVector3(), z.Capsule.Radius.Value, z.Capsule.Tail.ToVector3());
            }
            throw new NotImplementedException();
        }

        public SpringBoneManager CreateVrmSpringBone(List<Node> nodes)
        {
            if ((gltfVrmSpringBone is null))
            {
                return null;
            }

            var springBoneManager = new SpringBoneManager();

            // springs
            if (gltfVrmSpringBone.Springs != null)
            {
                foreach (var gltfSpring in gltfVrmSpringBone.Springs)
                {
                    var springBone = new SpringBone();
                    springBone.Comment = gltfSpring.Name;

                    // joint
                    foreach (var gltfJoint in gltfSpring.Joints)
                    {
                        var joint = new SpringJoint(nodes[gltfJoint.Node.Value]);
                        joint.HitRadius = gltfJoint.HitRadius.Value;
                        joint.DragForce = gltfJoint.DragForce.Value;
                        joint.GravityDir = gltfJoint.GravityDir.ToVector3();
                        joint.GravityPower = gltfJoint.GravityPower.Value;
                        joint.Stiffness = gltfJoint.Stiffness.Value;
                        springBone.Joints.Add(joint);
                    }

                    // collider
                    springBone.Colliders.AddRange(gltfSpring.Colliders.Select(colliderNode =>
                    {
                        if (UniGLTF.Extensions.VRMC_node_collider.GltfDeserializer.TryGet(Gltf.nodes[colliderNode].extensions,
                            out UniGLTF.Extensions.VRMC_node_collider.VRMC_node_collider extension))
                        {
                            var collider = new SpringBoneColliderGroup(nodes[colliderNode], extension.Shapes.Select(x =>
                            {
                                if (x.Sphere != null)
                                {
                                    return VrmSpringBoneCollider.CreateSphere(x.Sphere.Offset.ToVector3(), x.Sphere.Radius.Value);
                                }
                                else if (x.Capsule != null)
                                {
                                    return VrmSpringBoneCollider.CreateCapsule(x.Capsule.Offset.ToVector3(), x.Capsule.Radius.Value, x.Capsule.Tail.ToVector3());
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }));
                            return collider;
                        }
                        else
                        {
                            return null;
                        }
                    }).Where(x => x != null));

                    springBoneManager.Springs.Add(springBone);
                }
            }

            return springBoneManager;
        }

        public FirstPerson CreateVrmFirstPerson(List<Node> nodes, List<MeshGroup> meshGroups)
        {
            if (gltfVrm.FirstPerson == null)
            {
                return null;
            }
            return gltfVrm.FirstPerson.FromGltf(nodes);
        }

        public LookAt CreateVrmLookAt()
        {
            if (gltfVrm.LookAt == null)
            {
                return null;
            }
            return gltfVrm.LookAt.FromGltf();
        }

        public ArraySegment<byte> GetBufferBytes(UniGLTF.glTFBufferView bufferView)
        {
            if (!bufferView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferViewBufferIndex))
            {
                throw new Exception();
            }
            return GetBufferBytes(Gltf.buffers[bufferViewBufferIndex]);
        }

        public ArraySegment<byte> GetBufferBytes(UniGLTF.glTFBuffer buffer)
        {
            int index = Gltf.buffers.IndexOf(buffer);
            return Buffers[index].Bytes;
        }
    }
}