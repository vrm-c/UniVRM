using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public string AssetGenerator;
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
            private set
            {

            }
        }
        public void SetRoot(Node root)
        {
            m_root = root;

            Nodes.Clear();
            Nodes.AddRange(root.Traverse().Skip(1));
        }

        // gltf/nodes
        public List<Node> Nodes = new List<Node>();

        // gltf/animations
        public List<Animation> Animations = new List<Animation>();

        /// <summary>
        /// アニメーションに時間を指定するインターフェース
        /// </summary>
        public void SetTime(int index, TimeSpan elapsed)
        {
            if (index < 0 || index >= Animations.Count)
            {
                return;
            }
            Animations[index].SetTime(elapsed);
            Root.CalcWorldMatrix();
        }

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
            for (int i = 0; i < Animations.Count; ++i)
            {
                var a = Animations[i];
                sb.Append($"[Animation#{i:00}] {a}\n");
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
                = Enum.GetValues(typeof(HumanoidBones)).Cast<HumanoidBones>()
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
    }
}
