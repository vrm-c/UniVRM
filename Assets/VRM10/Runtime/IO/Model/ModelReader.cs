using System;
using System.IO;
using System.Linq;
using VrmLib;

namespace UniVRM10
{
    /// <summary>
    /// GLTF => VrmLib.Model
    /// </summary>
    public static class ModelReader
    {
        static Model Load(Vrm10ImportData storage, string rootName, Coordinates coords)
        {
            if (storage == null)
            {
                return null;
            }

            var model = new Model(coords)
            {
                AssetVersion = storage.AssetVersion,
                AssetGenerator = storage.AssetGenerator,
                AssetCopyright = storage.AssetCopyright,
                AssetMinVersion = storage.AssetMinVersion,
                Coordinates = coords,
            };

            // node
            model.Root.Name = rootName;
            for (var i = 0; i < storage.NodeCount; ++i)
            {
                var node = storage.CreateNode(i);
                model.Nodes.Add(node);
            }
            for (var i = 0; i < model.Nodes.Count; ++i)
            {
                var parent = model.Nodes[i];
                foreach (var j in storage.GetChildNodeIndices(i))
                {
                    var child = model.Nodes[j];
                    parent.Add(child);
                }
            }
            foreach (var x in model.Nodes)
            {
                if (x.Parent == null)
                {
                    model.Root.Add(x);
                }
            }

            // skin
            model.Skins.AddRange(Enumerable.Range(0, storage.SkinCount).Select(x => storage.CreateSkin(x, model.Nodes)));

            // mesh
            model.MeshGroups.AddRange(Enumerable.Range(0, storage.MeshCount).Select(x => storage.CreateMesh(x)));

            // skin
            for (int i = 0; i < storage.NodeCount; ++i)
            {
                var (meshIndex, skinIndex) = storage.GetNodeMeshSkin(i);
                if (meshIndex >= 0 && meshIndex < model.MeshGroups.Count)
                {
                    var node = model.Nodes[i];
                    var mesh = model.MeshGroups[meshIndex];
                    node.MeshGroup = mesh;
                    if (skinIndex >= 0 && skinIndex < model.Skins.Count)
                    {
                        var skin = model.Skins[skinIndex];
                        mesh.Skin = skin;
                    }
                }
            }

            // boneWeight の付与
            foreach (var meshGroup in model.MeshGroups)
            {
                if (meshGroup.Skin == null && meshGroup.Meshes.Any(mesh => mesh.MorphTargets.Count > 0))
                {
                    // Skinning が無くて MorphTarget が有る場合に実施する
                    var nodes = model.Nodes.Where(node => node.MeshGroup == meshGroup).ToArray();
                    if (nodes.Length != 1)
                    {
                        // throw new NotImplementedException();
                        // このメッシュが複数のノードから共有されている場合は、
                        // 個別に Skin を作成する必要があり、
                        // 異なる bindPoses が必要になるため mesh の複製が必要になる！
                        // 稀にあるかもしれない。
                        continue;
                    }
                    var node = nodes[0];

                    // add bone weight
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        // all (1, 0, 0, 0)
                        var weights = storage.Data.NativeArrayManager.CreateNativeArray<UnityEngine.Vector4>(mesh.VertexBuffer.Count);
                        for (int i = 0; i < weights.Length; ++i)
                        {
                            weights[i] = new UnityEngine.Vector4(1, 0, 0, 0);
                        }
                        mesh.VertexBuffer.Add(VertexBuffer.WeightKey, new UniGLTF.BufferAccessor(storage.Data.NativeArrayManager, weights.Reinterpret<byte>(16), UniGLTF.AccessorValueType.FLOAT, UniGLTF.AccessorVectorType.VEC4, weights.Length));

                        // all zero
                        var joints = storage.Data.NativeArrayManager.CreateNativeArray<UniGLTF.UShort4>(mesh.VertexBuffer.Count);
                        mesh.VertexBuffer.Add(VertexBuffer.JointKey, new UniGLTF.BufferAccessor(storage.Data.NativeArrayManager, joints.Reinterpret<byte>(8), UniGLTF.AccessorValueType.UNSIGNED_SHORT, UniGLTF.AccessorVectorType.VEC4, joints.Length));
                    }

                    // bind matrix
                    var bindMatrices = storage.Data.NativeArrayManager.CreateNativeArray<UnityEngine.Matrix4x4>(1);
                    bindMatrices[0] = node.Matrix.inverse;

                    // skinning
                    meshGroup.Skin = new Skin
                    {
                        GltfIndex = -1,
                        Joints = new System.Collections.Generic.List<Node> { node },
                        Root = node,
                        InverseMatrices = new UniGLTF.BufferAccessor(storage.Data.NativeArrayManager, bindMatrices.Reinterpret<byte>(64), UniGLTF.AccessorValueType.FLOAT, UniGLTF.AccessorVectorType.MAT4, bindMatrices.Length),
                    };
                }
            }

            return model;
        }

        public static Model Read(UniGLTF.GltfData data, Coordinates? coords = default)
        {
            var storage = new Vrm10ImportData(data);
            var model = Load(storage, Path.GetFileName(data.TargetPath), coords.GetValueOrDefault(Coordinates.Vrm1));
            model.ConvertCoordinate(Coordinates.Unity);
            return model;
        }
    }
}
