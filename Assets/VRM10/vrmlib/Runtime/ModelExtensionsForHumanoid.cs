using System;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelExtensionsForHumanoid
    {
        static ValueTuple<Node, Node> GetUpperLower(Node root)
        {
            var legL = root.Traverse().FirstOrDefault(x => x.HumanoidBone == HumanoidBones.leftUpperLeg);
            var legR = root.Traverse().FirstOrDefault(x => x.HumanoidBone == HumanoidBones.rightUpperLeg);
            var head = root.Traverse().FirstOrDefault(x => x.HumanoidBone == HumanoidBones.head);

            var parentL = legL.Parent;
            var parentR = legR.Parent;
            if (parentL != parentR)
            {
                throw new Exception("different leftLeg parent and rightLeg parent");
            }
            var lower = parentL;

            var upperAncestors = head.Ancestors().ToList();
            if (upperAncestors.Any(x => x == lower))
            {
                throw new Exception("lower is ancestor of head");
            }

            var lowerAncestors = legL.Ancestors().ToList();

            while (true)
            {
                if (upperAncestors.Last() != lowerAncestors.Last())
                {
                    break;
                }
                upperAncestors.RemoveAt(upperAncestors.Count - 1);
                lowerAncestors.RemoveAt(lowerAncestors.Count - 1);
            }

            return (upperAncestors.Last(), lowerAncestors.Last());
        }

        /// <summary>
        /// 
        /// root
        ///   upper
        ///   lower
        ///     legL
        ///     legR
        ///     
        /// ↓
        /// 
        /// ①上半身をchestにする例
        /// 
        /// root(hips)
        ///   legL
        ///   legR
        ///   lower(spine: 上下反転するため頭の位置が変わる)
        ///     upper(chest)
        /// 
        /// ②上半身をspineにする例もありえる。その場合は、下半身とその親を近接させて 
        /// 下半身にはhumanoidボーンを割り当てない(hipsとみなす)
        /// 
        /// root(hips)
        ///   legL
        ///   legR
        ///   lower(上下反転するため頭の位置が変わる)
        ///     upper(spine)
        ///
        /// ③もしくは下半身をhipsに繰り上げる
        /// 
        /// lower(hips: 上下反転するため頭の位置が変わる)
        ///   legL
        ///   legR
        ///   upper(spine)
        ///
        /// </summary>
        public static string FixInvertedPelvis(this Model model)
        {
            var (upper, lower) = GetUpperLower(model.Root);
            if (upper == null)
            {
                return "FixInvertedPelvis: upper not found. this is not humanoid ? do nothing";
            }
            if (lower == null)
            {
                return "FixInvertedPelvis: lower not found. this is model's pelvis is not inverted. do nothing";
            }

            // found lower. fix inverted pelvis...

            var hips = model.Root.FindBone(HumanoidBones.hips);
            {
                hips.HumanoidBone = null;
            }
            var spine = model.Root.FindBone(HumanoidBones.spine);
            {
                spine.HumanoidBone = null;
            }
            var chest = model.Root.FindBone(HumanoidBones.chest);
            if (chest != null)
            {
                chest.HumanoidBone = null;
            }
            var legL = model.Root.FindBone(HumanoidBones.leftUpperLeg);
            var legR = model.Root.FindBone(HumanoidBones.rightUpperLeg);

            // [chest]
            // 上半身を下半身の子にしてchestとなす
            lower.Add(upper);
            upper.HumanoidBone = HumanoidBones.chest;

            // [hips]
            // lowerの親をhipsとして両足の間に配置する
            var newHips = lower.Parent;
            newHips.Translation = new Vector3(0, legL.Translation.Y, legL.Translation.Z);
            newHips.HumanoidBone = HumanoidBones.hips;

            // [spine]
            // 下半身をspineとして
            lower.HumanoidBone = HumanoidBones.spine;
            // hips と chest の中間に配置する
            lower.Translation = (upper.Translation + hips.Translation) * 0.5f;

            // [legs]
            // 足の親を下半身からrootに変える
            hips.Add(legL);
            hips.Add(legR);

            return $"FixInvertedPelvis: lower: {lower.Name}";
        }

        static void StringBuilder(System.Text.StringBuilder sb, Node n, string indent = "")
        {
            sb.Append($"{indent}{n}\n");

            foreach (var child in n.Children)
            {
                StringBuilder(sb, child, indent + "  ");
            }
        }

        public static string HumanoidBoneEstimate(this Model model)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("HumanoidBoneEstimate: ");

            // estimate skeleton
            var skeleton = SkeletonEstimator.Detect(model.Root);
            if (skeleton == null)
            {
                return "fail to estimate skeleton";
            }

            // rename bone
            foreach (var kv in skeleton)
            {
                kv.Value.Name = kv.Key.ToString();
            }

            if (model.Vrm == null)
            {
                sb.Append("add vrm humanoid");
                model.Vrm = new Vrm(new Meta
                {
                }, "UniVRM-0.51.0", "0.0");
            }
            else
            {

            }

            StringBuilder(sb, skeleton[HumanoidBones.hips]);

            foreach (var skin in model.Skins)
            {
                if (skin.Root == null)
                {
                    skin.Root = (Node)skeleton[HumanoidBones.hips].Parent;
                    sb.Append($"{skin}: set {skin.Root}\n");
                }
                else
                {
                    sb.Append($"{skin}: {skin.Root}\n");
                }
            }

            return sb.ToString(); ;
        }

    }
}