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

        // gltf/images
        public readonly List<Image> Images = new List<Image>();

        // gltf/textures
        public readonly List<Texture> Textures = new List<Texture>();

        // gltf/materials
        public readonly List<Material> Materials = new List<Material>();

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

        public Vrm Vrm;

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

            for (int i = 0; i < Images.Count; ++i)
            {
                var x = Images[i];
                sb.Append($"[Image#{i:00}] {x}\n");
            }
            // for (int i = 0; i < Textures.Count; ++i)
            // {
            //     var t = Textures[i];
            //     sb.Append($"[Texture#{i:00}] {t}\n");
            // }
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

            //
            // VRM
            //
            if (Vrm != null)
            {
                sb.Append($"[VRM] export: {Vrm.ExporterVersion}, spec: {Vrm.SpecVersion}\n");
                sb.Append($"[VRM][meta] {Vrm.Meta}\n");
                var boneMap = GetBoneMap();
                if (boneMap.Any())
                {
                    sb.Append($"[VRM][humanoid] {boneMap.Count}/{Enum.GetValues(typeof(HumanoidBones)).Length - 1}\n");
                    if (boneMap.Keys.Contains(HumanoidBones.unknown))
                    {
                        sb.Append($"[VRM][humanoid] {boneMap.Count} contains 'unknown'\n");
                    }
                    if (boneMap.TryGetValue(HumanoidBones.jaw, out Node jaw))
                    {
                        sb.Append($"[VRM][humanoid] contains 'jaw' => {jaw.Name}\n");
                    }
                }
                if (Vrm.ExpressionManager != null
                    && Vrm.ExpressionManager.ExpressionList != null
                    && Vrm.ExpressionManager.ExpressionList.Any())
                {
                    sb.Append("[VRM][expression] ");
                    foreach (var ex in Vrm.ExpressionManager.ExpressionList)
                    {
                        sb.Append($"[{ex.Preset}]");
                    }
                    sb.Append($"\n");
                }
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
