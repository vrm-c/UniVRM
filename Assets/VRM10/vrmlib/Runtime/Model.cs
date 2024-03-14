using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniGLTF;
using UniGLTF.Utils;
using Unity.Collections;
using UnityEngine;

namespace VrmLib
{
    /// <summary>
    /// 処理しやすいようにした中間形式
    /// * index 参照は実参照
    /// * accessor, bufferView は実バイト列(ArraySegment<byte>)
    /// * meshは、subMesh方式(indexが offset + length)
    /// </summary>
    public class Model
    {
        public Model(Coordinates coordinates)
        {
            Coordinates = coordinates;
        }

        public Coordinates Coordinates;

        public string AssetVersion = "2.0";
        public string AssetGenerator = $"UniVRM-{PackageVersion.VERSION}";
        public string AssetCopyright;
        public string AssetMinVersion;

        // gltf/materials
        public readonly List<object> Materials = new List<object>();

        // gltf/skins
        public readonly List<Skin> Skins = new List<Skin>();

        // gltf/meshes
        public readonly List<MeshGroup> MeshGroups = new List<MeshGroup>();

        // gltf の nodes に含まれない。sceneに相当
        Node m_root = new Node("__root__");

        public Node Root
        {
            get => m_root;
        }
        public void SetRoot(Node root)
        {
            m_root = root;

            Nodes.Clear();
            Nodes.AddRange(root.Traverse().Skip(1));
        }

        // gltf/nodes
        public List<Node> Nodes = new List<Node>();


        public Dictionary<HumanoidBones, Node> GetBoneMap()
        {
            return Root.Traverse()
                .Where(x => x.HumanoidBone.HasValue)
                .ToDictionary(x => x.HumanoidBone.Value, x => x);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[GLTF] generator: {AssetGenerator}\n");

            for (int i = 0; i < Materials.Count; ++i)
            {
                var m = Materials[i];
                sb.Append($"[Material#{i:00}] {m}\n");
            }
            for (int i = 0; i < MeshGroups.Count; ++i)
            {
                var m = MeshGroups[i];
                sb.Append($"[Mesh#{i:00}] {m}\n");
            }
            sb.Append($"[Node] {Nodes.Count} nodes\n");

            foreach (var skin in Skins)
            {
                sb.Append($"[Skin] {skin}\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// HumanoidBonesの構成チェック
        /// </summary>
        /// <returns></returns>
        public bool CheckVrmHumanoid()
        {
            var vrmhumanoids = new HashSet<HumanoidBones>();

            // HumanoidBonesの重複チェック
            foreach (var node in Nodes)
            {
                if (node.HumanoidBone.HasValue)
                {
                    if (vrmhumanoids.Contains(node.HumanoidBone.Value))
                    {
                        return false;
                    }
                    else
                    {
                        vrmhumanoids.Add(node.HumanoidBone.Value);
                    }
                }
            }

            // HumanoidBonesでBoneRequiredAttributeが定義されているものすべてが使われているかどうかを判断

            var boneattributes
                = CachedEnum.GetValues<HumanoidBones>()
                    .Select(bone => bone.GetType().GetField(bone.ToString()))
                    .Select(info => info.GetCustomAttributes(typeof(BoneRequiredAttribute), false) as BoneRequiredAttribute[])
                    .Where(attributes => attributes.Length > 0);

            var nodeHumanoids
                = vrmhumanoids
                    .Select(humanoid => humanoid.GetType().GetField(humanoid.ToString()))
                    .Select(info => info.GetCustomAttributes(typeof(BoneRequiredAttribute), false) as BoneRequiredAttribute[])
                    .Where(attributes => attributes.Length > 0);

            if (nodeHumanoids.Count() != boneattributes.Count()) return false;

            return true;
        }

        public static Node GetNode(Node root, string path)
        {
            var splitted = path.Split('/');
            var it = splitted.Select(x => x).GetEnumerator();

            var current = root;
            while (it.MoveNext())
            {
                current = current.Children.First(x => x.Name == it.Current);
            }

            return current;
        }

        /// <summary>
        /// Node Transform の Rotation, Scaling 成分を除去する
        /// </summary>
        public void ApplyRotationAndScale()
        {
            // worldPositionを記録する
            var m_positionMap = Nodes.ToDictionary(x => x, x => x.Translation);

            // 回転・拡縮を除去する
            // 木構造の根元から実行する
            // Rootは編集対象外
            foreach (var node in Root.Traverse().Skip(1))
            {
                // 回転・拡縮を除去
                if (m_positionMap.TryGetValue(node, out Vector3 pos))
                {
                    var t = Matrix4x4.Translate(pos);
                    node.SetMatrix(t, false);
                }
            }
        }

        #region Node
        public void NodeAdd(Node node, Node parent = null)
        {
            if (parent is null)
            {
                parent = this.Root;
            }
            parent.Add(node);
            if (this.Nodes.Contains(node))
            {
                throw new ArgumentException($"Nodes contain {node}");
            }
            this.Nodes.Add(node);
        }

        public void NodeRemove(Node remove)
        {
            foreach (var node in this.Nodes)
            {
                if (node.Parent == remove)
                {
                    remove.Remove(node);
                }
                if (remove.Parent == node)
                {
                    node.Remove(remove);
                }
            }
            if (this.Root.Children.Contains(remove))
            {
                this.Root.Remove(remove);
            }

            this.Nodes.Remove(remove);
        }
        #endregion

        /// <summary>
        /// ボーンを含む Node Transform の Rotation, Scaling 成分を除去し、SkinnedMesh の Bind Matrix も再計算する。
        /// </summary>
        public string SkinningBake(INativeArrayManager arrayManager)
        {
            foreach (var node in this.Nodes)
            {
                var meshGroup = node.MeshGroup;
                if (meshGroup == null)
                {
                    continue;
                }

                if (meshGroup.Skin != null)
                {
                    // 正規化されていれば1つしかない
                    // されていないと Primitive の数だけある
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        {
                            // Skinningの出力先を自身にすることでBakeする
                            meshGroup.Skin.Skinning(arrayManager, mesh.VertexBuffer);
                        }

                        //　morphのPositionは相対値が入っているはずなので、手を加えない（正規化されていない場合、二重に補正が掛かる）
                        /*
                                                foreach (var morph in mesh.MorphTargets)
                                                {
                                                    if (morph.VertexBuffer.Positions != null)
                                                    {
                                                        meshGroup.Skin.Skinning(morph.VertexBuffer);
                                                    }
                                                }
                                                */
                    }

                    meshGroup.Skin.Root = null;
                    meshGroup.Skin.InverseMatrices = null;
                }
                else
                {
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        // nodeに対して疑似的にSkinningする
                        // 回転と拡縮を適用し位置は適用しない
                        mesh.ApplyRotationAndScaling(node.Matrix);
                    }
                }
            }

            // 回転・拡縮を除去する
            this.ApplyRotationAndScale();

            // inverse matrix の再計算
            foreach (var node in this.Nodes)
            {
                var meshGroup = node.MeshGroup;
                if (meshGroup == null)
                {
                    continue;
                }

                foreach (var mesh in meshGroup.Meshes)
                {
                    if (meshGroup.Skin != null)
                    {
                        meshGroup.Skin.CalcInverseMatrices(arrayManager);
                    }
                }
            }

            return "SkinningBake";
        }

        static void ReverseX(BufferAccessor ba)
        {
            if (ba.ComponentType != AccessorValueType.FLOAT)
            {
                throw new Exception();
            }
            if (ba.AccessorType == AccessorVectorType.VEC3)
            {
                var span = ba.Bytes.Reinterpret<Vector3>(1);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseX();
                }
            }
            else if (ba.AccessorType == AccessorVectorType.MAT4)
            {
                var span = ba.Bytes.Reinterpret<Matrix4x4>(1);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseX();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static void ReverseZ(BufferAccessor ba)
        {
            if (ba.ComponentType != AccessorValueType.FLOAT)
            {
                throw new Exception();
            }
            if (ba.AccessorType == AccessorVectorType.VEC3)
            {
                var span = ba.Bytes.Reinterpret<Vector3>(1);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseZ();
                }
            }
            else if (ba.AccessorType == AccessorVectorType.MAT4)
            {
                var span = ba.Bytes.Reinterpret<Matrix4x4>(1);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].ReverseZ();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        struct Reverser
        {
            public Action<BufferAccessor> ReverseBuffer;
            public Func<Vector3, Vector3> ReverseVector3;
            public Func<Matrix4x4, Matrix4x4> ReverseMatrix;
        }

        static Reverser ZReverser => new Reverser
        {
            ReverseBuffer = ReverseZ,
            ReverseVector3 = v => v.ReverseZ(),
            ReverseMatrix = m => m.ReverseZ(),
        };

        static Reverser XReverser => new Reverser
        {
            ReverseBuffer = ReverseX,
            ReverseVector3 = v => v.ReverseX(),
            ReverseMatrix = m => m.ReverseX(),
        };

        /// <summary>
        /// ignoreVrm: VRM-0.XX では無変換で入出力してた。VRM-1.0 では変換する。
        /// </summary>
        public void ConvertCoordinate(Coordinates coordinates, bool ignoreVrm = false)
        {
            if (Coordinates.Equals(coordinates))
            {
                return;
            }

            if (Coordinates.IsVrm0 && coordinates.IsUnity)
            {
                ReverseAxisAndFlipTriangle(ZReverser, ignoreVrm);
                UVVerticalFlip();
                Coordinates = coordinates;
            }
            else if (Coordinates.IsUnity && coordinates.IsVrm0)
            {
                ReverseAxisAndFlipTriangle(ZReverser, ignoreVrm);
                UVVerticalFlip();
                Coordinates = coordinates;
            }
            else if (Coordinates.IsVrm1 && coordinates.IsUnity)
            {
                ReverseAxisAndFlipTriangle(XReverser, ignoreVrm);
                UVVerticalFlip();
                Coordinates = coordinates;
            }
            else if (Coordinates.IsUnity && coordinates.IsVrm1)
            {
                ReverseAxisAndFlipTriangle(XReverser, ignoreVrm);
                UVVerticalFlip();
                Coordinates = coordinates;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// UVのVを反転する。 => V = 1.0 - V
        /// </summary>
        void UVVerticalFlip()
        {
            foreach (var g in MeshGroups)
            {
                foreach (var m in g.Meshes)
                {
                    var uv = m.VertexBuffer.TexCoords;
                    if (uv != null)
                    {
                        var span = uv.Bytes.Reinterpret<Vector2>(1);
                        for (int i = 0; i < span.Length; ++i)
                        {
                            span[i] = span[i].UVVerticalFlip();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// * Position, Normal の Z座標に -1 を乗算する
        /// * Rotation => Axis Angle に分解 => Axis の Z座標に -1 を乗算。Angle に -1 を乗算
        /// * Triangle の index を 0, 1, 2 から 2, 1, 0 に反転する
        /// </summary>
        void ReverseAxisAndFlipTriangle(Reverser reverser, bool ignoreVrm)
        {
            // 複数の gltf.accessor が別の要素間で共有されている場合に、２回処理されることを防ぐ
            // edgecase: InverseBindMatrices で遭遇
            var unique = new HashSet<NativeArray<byte>>();

            foreach (var g in MeshGroups)
            {
                foreach (var m in g.Meshes)
                {
                    foreach (var kv in m.VertexBuffer)
                    {
                        var k = kv.Key;
                        var v = kv.Value;
                        if (k == VertexBuffer.PositionKey || k == VertexBuffer.NormalKey)
                        {
                            if (unique.Add(v.Bytes))
                            {
                                reverser.ReverseBuffer(v);
                            }
                        }
                        else if (k == VertexBuffer.TangentKey)
                        {
                            // I don't know
                        }
                    }

                    if (unique.Add(m.IndexBuffer.Bytes))
                    {
                        switch (m.IndexBuffer.ComponentType)
                        {
                            case AccessorValueType.UNSIGNED_BYTE:
                                FlipTriangle(m.IndexBuffer.Bytes);
                                break;
                            case AccessorValueType.UNSIGNED_SHORT:
                                FlipTriangle(m.IndexBuffer.Bytes.Reinterpret<UInt16>(1));
                                break;
                            case AccessorValueType.UNSIGNED_INT:
                                FlipTriangle(m.IndexBuffer.Bytes.Reinterpret<UInt32>(1));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    foreach (var mt in m.MorphTargets)
                    {
                        foreach (var kv in mt.VertexBuffer)
                        {
                            var k = kv.Key;
                            var v = kv.Value;
                            if (k == VertexBuffer.PositionKey || k == VertexBuffer.NormalKey)
                            {
                                if (unique.Add(v.Bytes))
                                {
                                    reverser.ReverseBuffer(v);
                                }
                            }
                            if (k == VertexBuffer.TangentKey)
                            {
                                // I don't know
                            }
                        }
                    }
                }
            }

            // 親から順に処理する
            // Rootは原点決め打ちのノード(GLTFに含まれない)
            foreach (var n in Root.Traverse().Skip(1))
            {
                n.SetMatrix(reverser.ReverseMatrix(n.Matrix), false);
            }
            // 親から順に処理したので不要
            // Root.CalcWorldMatrix();

            foreach (var s in Skins)
            {
                if (s.InverseMatrices != null)
                {
                    if (unique.Add(s.InverseMatrices.Bytes))
                    {
                        reverser.ReverseBuffer(s.InverseMatrices);
                    }
                }
            }
        }

        static void FlipTriangle(NativeArray<byte> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }

        static void FlipTriangle(NativeArray<ushort> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }

        static void FlipTriangle(NativeArray<uint> indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                // 0, 1, 2 to 2, 1, 0
                var tmp = indices[i + 2];
                indices[i + 2] = indices[i];
                indices[i] = tmp;
            }
        }

    }
}
