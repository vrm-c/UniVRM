using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace VrmLib
{
    public static class ModelExtensionsForNode
    {
        class NodeUsage
        {
            public bool HasMesh;
            public int WeightUsed;
            public HumanoidBones? HumanBone;
            public bool TreeHasHumanBone;

            /// <summary>
            /// 子階層に消さずに残すBoneが含まれるか
            /// </summary>
            public bool TreeHasUsedBone;

            public bool SpringUse;

            public override string ToString()
            {
                if (HumanBone.HasValue)
                {
                    return $"{HumanBone.Value}";
                }
                else
                {
                    return $"{Used}";
                }
            }

            public bool Used
            {
                get
                {
                    if (HasMesh) return true;
                    if (WeightUsed > 0) return true;
                    if (HumanBone.HasValue && HumanBone.Value != HumanoidBones.unknown) return true;
                    if (SpringUse) return true;
                    if (TreeHasHumanBone) return true;
                    if (TreeHasUsedBone) return true;
                    return false;
                }
            }
        }

        public static IEnumerable<Node> GetRemoveNodes(this Model model)
        {
            var nodeUsage = model.Nodes.Select(x =>
                new NodeUsage
                {
                    HasMesh = x.MeshGroup != null,
                    HumanBone = x.HumanoidBone,
                    TreeHasHumanBone = x.Traverse().Any(y => y.HumanoidBone.HasValue),
                })
                .ToArray();

            var bones = model.Nodes.Where(x => x.HumanoidBone.HasValue).ToArray();

            // joint use
            foreach (var meshGroup in model.MeshGroups)
            {
                var skin = meshGroup.Skin;
                if (skin != null)
                {
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        var joints = mesh.VertexBuffer.Joints;
                        var weights = mesh.VertexBuffer.Weights;
                        if (joints != null && weights != null)
                        {
                            var jointsSpan = SpanLike.Wrap<SkinJoints>(joints.Bytes);
                            var weightsSpan = SpanLike.Wrap<Vector4>(weights.Bytes);
                            for (int i = 0; i < jointsSpan.Length; ++i)
                            {
                                var w = weightsSpan[i];
                                var j = jointsSpan[i];
                                if (w.X > 0) nodeUsage[model.Nodes.IndexOf(skin.Joints[j.Joint0])].WeightUsed++;
                                if (w.Y > 0) nodeUsage[model.Nodes.IndexOf(skin.Joints[j.Joint1])].WeightUsed++;
                                if (w.Z > 0) nodeUsage[model.Nodes.IndexOf(skin.Joints[j.Joint2])].WeightUsed++;
                                if (w.W > 0) nodeUsage[model.Nodes.IndexOf(skin.Joints[j.Joint3])].WeightUsed++;
                            }
                        }
                    }
                }
            }

            // 削除されるNodeのうち、子階層に1つでも残るNodeがあればそのNodeも残す
            for (int i = 0; i < nodeUsage.Length; i++)
            {
                if (nodeUsage[i].Used) continue;

                var children = model.Nodes[i].Traverse();

                nodeUsage[i].TreeHasUsedBone = children.Where(x => nodeUsage[model.Nodes.IndexOf(x)].Used).Any();
            }


            var spring = model.Vrm?.SpringBone;
            if (spring != null)
            {
                foreach (var x in spring.Springs)
                {
                    foreach (var y in x.Joints)
                    {
                        nodeUsage[model.Nodes.IndexOf(y.Node)].SpringUse = true;
                    }
                    foreach (var y in x.Colliders)
                    {
                        nodeUsage[model.Nodes.IndexOf(y.Node)].SpringUse = true;
                    }
                }
            }

            var nodes = nodeUsage.Select((x, i) => (i, x))
                .Where(x => !x.Item2.Used)
                .Select(x => model.Nodes[x.Item1])
                .ToArray();
            return nodes;
        }
    }
}