using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelModifierExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue addValue)
        {
            bool canAdd = !dict.ContainsKey(key);

            if (canAdd)
                dict.Add(key, addValue);

            return canAdd;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this System.Collections.Generic.IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        static void ReplaceMorphTargetAnimationNode(IEnumerable<Animation> animations, Node dst)
        {
            foreach (var animation in animations)
            {
                var dstAnimation = animation.GetOrCreateNodeAnimation(dst);
                foreach (var (node, nodeAnimation) in animation.NodeMap)
                {
                    if (nodeAnimation.Curves.TryGetValue(AnimationPathType.Weights, out CurveSampler curve))
                    {
                        // remove
                        nodeAnimation.Curves.Remove(AnimationPathType.Weights);

                        // add
                        if (!dstAnimation.Curves.TryAdd(AnimationPathType.Weights, curve))
                        {
                            Console.Error.WriteLine($"already exists. skip: {node.Name}: {nodeAnimation}");
                        }
                    }
                }
            }
        }

        /// Expression
        /// FirstPersonの置き換え
        public static void MeshNodeReplace(this ModelModifier modifier, Node src, Node dst)
        {
            var vrm = modifier.Model.Vrm;
            if (vrm is null)
            {
                return;
            }

            if (vrm.ExpressionManager != null)
            {
                foreach (var b in vrm.ExpressionManager.ExpressionList)
                {
                    foreach (var v in b.MorphTargetBinds)
                    {
                        if (v.Node == src)
                        {
                            v.Node = dst;
                        }
                    }
                }
            }
            if (vrm.FirstPerson != null)
            {
                foreach (var a in vrm.FirstPerson.Annotations)
                {
                    if (a.Node == src)
                    {
                        a.Node = dst;
                    }
                }
            }
        }

        public static string SingleMesh(this ModelModifier modifier, string name)
        {
            var count = modifier.Model.MeshGroups.Sum(x => x.Meshes.Count);
            var meshes = modifier.Model.Root.Traverse()
                .Select(x => x.MeshGroup)
                .Where(x => x != null)
                .Select(x => $"[{x.Name}]")
                .ToArray();
            if (meshes.Length == 0)
            {
                return "SingleMesh: no mesh. do nothing";
            }
            if (meshes.Length <= 1)
            {
                return "SingleMesh: one mesh. do nothing";
            }

            var mesh = modifier.Model.CreateSingleMesh(name);
            var meshNode = new Node(mesh.Name)
            {
                MeshGroup = mesh,
            };
            mesh.Skin.Root = meshNode;

            // fix bone weight (0, x, 0, 0) => (x, 0, 0, 0)
            // mesh.Meshes[0].VertexBuffer.FixBoneWeight();

            // replace morphAnimation reference
            ReplaceMorphTargetAnimationNode(modifier.Model.Animations, meshNode);

            // update Model
            foreach (var x in modifier.Model.MeshGroups.ToArray())
            {
                modifier.MeshReplace(x, mesh);
            }
            foreach (var node in modifier.Model.Nodes)
            {
                if (node.MeshGroup != null)
                {
                    node.MeshGroup = null;
                    modifier.MeshNodeReplace(node, meshNode);
                }

            }
            modifier.NodeAdd(meshNode);

            var names = string.Join("", meshes);
            // return $"SingleMesh: {names}";
            return $"SingleMesh: {count} => {modifier.Model.MeshGroups.Sum(x => x.Meshes.Count)}";
        }

        public static void SepareteByMorphTarget(this ModelModifier modifier, MeshGroup mesh)
        {
            var (with, without) = mesh.SepareteByMorphTarget();
            var list = new List<MeshGroup>();
            if (with != null) list.Add(with);
            if (without != null) list.Add(without);

            // 分割モデルで置き換え
            if (list.Any())
            {
                modifier.MeshReplace(mesh, list[0]);
                // rename node
                modifier.Model.Nodes.Find(x => x.MeshGroup == list[0]).Name = list[0].Name;
            }

            if (list.Count > 1)
            {
                // morph無しと有り両方存在する場合に2つ目を追加する
                modifier.MeshReplace(null, list[1]);
                modifier.NodeAdd(new Node(list[1].Name)
                {
                    MeshGroup = list[1]
                });
            }
        }

        public static void SepareteByHeadBone(this ModelModifier modifier, MeshGroup mesh, HashSet<int> boneIndices)
        {
            var (with, without) = mesh.SepareteByHeadBone(boneIndices);
            var list = new List<MeshGroup>();
            if (with != null) list.Add(with);
            if (without != null) list.Add(without);

            // 分割モデルで置き換え
            if (list.Any())
            {
                modifier.MeshReplace(mesh, list[0]);
                // rename node
                modifier.Model.Nodes.Find(x => x.MeshGroup == list[0]).Name = list[0].Name;
            }

            if (list.Count > 1)
            {
                // 頭と胴体で分割後2つ以上ある場合、2つ目を追加する
                modifier.MeshReplace(null, list[1]);
                modifier.NodeAdd(new Node(list[1].Name)
                {
                    MeshGroup = list[1]
                });
            }
        }

        public static string NodeReduce(this ModelModifier modifier)
        {
            var count = modifier.Model.Nodes.Count;
            var removeNames = new List<string>();

            // ノードを削除する
            foreach (var node in modifier.Model.GetRemoveNodes())
            {
                modifier.NodeRemove(node);
                removeNames.Add($"[{node.Name}]");
                foreach (var skin in modifier.Model.Skins)
                {
                    var index = skin.Joints.IndexOf(node);
                    if (index != -1)
                    {
                        // remove
                        skin.Joints[index] = null;
                    }
                }
            }

            // 削除されたノードを参照する頂点バッファを修正する
            foreach (var meshGroup in modifier.Model.MeshGroups)
            {
                var skin = meshGroup.Skin;
                if (skin != null && skin.Joints.Contains(null))
                {
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        skin.FixBoneWeight(mesh.VertexBuffer.Joints, mesh.VertexBuffer.Weights);
                    }
                }
            }

            var joined = string.Join("", removeNames);

            return $"NodeReduce: {count} => {modifier.Model.Nodes.Count}";
            // return $"NodeReduce: {joined}";
        }

        public static string SkinningBake(this ModelModifier modifier)
        {
            foreach (var node in modifier.Model.Nodes)
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
                            meshGroup.Skin.Skinning(mesh.VertexBuffer);
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
            modifier.Model.ApplyRotationAndScale();

            // inverse matrix の再計算
            foreach (var node in modifier.Model.Nodes)
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
                        meshGroup.Skin.CalcInverseMatrices();
                    }
                }
            }

            return "SkinningBake";
        }

        public static string CloneSharedMesh(this ModelModifier modifier)
        {
            Dictionary<MeshGroup, int> m_useMap = new Dictionary<MeshGroup, int>();

            var cloned = new List<string>();

            foreach (var node in modifier.Model.Nodes)
            {
                if (node.MeshGroup == null)
                {
                    continue;
                }

                var n = m_useMap.GetValueOrDefault(node.MeshGroup);
                if (n > 0)
                {
                    // copy
                    node.MeshGroup = node.MeshGroup.Clone();
                    cloned.Add($"[{node.MeshGroup.Name}]");
                }
                m_useMap[node.MeshGroup] = n + 1;
            }

            if (!cloned.Any())
            {
                return "CloneSharedMesh: no shared mesh. do nothing";
            }
            else
            {
                var joined = string.Join("", cloned);
                return $"CloneSharedMesh: copy {joined}";
            }
        }

        public static string MaterialIntegrate(this ModelModifier modifier)
        {
            var sb = new System.Text.StringBuilder();
            var materials = new List<Material>();

            foreach (var material in modifier.Model.Materials.ToArray())
            {
                var found = materials.FirstOrDefault(x => x.CanIntegrate(material));
                if (found != null)
                {
                    // merge
                    modifier.MaterialReplace(material, found);
                }
                else
                {
                    // add
                    materials.Add(material);
                }
            }

            sb.Append($"MaterialIntegrate: {modifier.Model.Materials.Count} => {materials.Count}");

            modifier.Model.Materials.Clear();
            modifier.Model.Materials.AddRange(materials);

            return sb.ToString();
        }
    }
}