using System.Collections.Generic;
using System.Linq;
using UniVRM10.ClothWarp.Components;
using UnityEngine;
using UniGLTF;


namespace UniVRM10.Cloth.Viewer
{
    public static class ClothGuess
    {
        public enum StrandConnectionType
        {
            Cloth,
            ClothLoop,
            Strand,
        }

        public static void Guess(Animator animator)
        {
            // skirt
            {
                if (TryAddGroup(animator, HumanBodyBones.Hips,
                    new[] { "skirt", "ｽｶｰﾄ", "スカート" }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                    c.LoopIsClosed = true;
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.Hips,
                    new[] { "skirt", "ｽｶｰﾄ", "スカート" }, new string[] { }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                    c.LoopIsClosed = true;
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Head,
                    new[] { "髪", "hair" }, out var g))
                {
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Hips,
                    new[] { "裾" }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.LeftUpperArm,
                    new[] { "袖" }, new[] { "ひじ袖" }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                    c.LoopIsClosed = true;
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.LeftLowerArm,
                    new[] { "袖" }, new string[] { }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                    c.LoopIsClosed = true;
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.RightUpperArm,
                    new[] { "袖" }, new[] { "ひじ袖" }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                    c.LoopIsClosed = true;
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.RightLowerArm,
                    new[] { "袖" }, new string[] { }, out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Chest, new[] { "マント" },
                    out var g))
                {
                    var c = g[0].gameObject.AddComponent<ClothGrid>();
                    c.Warps = g;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="name"></param>
        /// <param name="animator"></param>
        /// <param name="humanBone"></param>
        /// <param name="targets"></param>
        /// <param name="excludes"></param>
        /// <param name="type"></param>
        /// <param name="sort"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        static bool TryAddGroupChildChild(
            Animator animator, HumanBodyBones humanBone,
            string[] targets, string[] excludes,
            out List<ClothWarpRoot> group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                UniGLTFLogger.Warning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<ClothWarpRoot> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (Transform childchild in child)
                {
                    if (excludes.Any(x => childchild.name.ToLower().Contains(x.ToLower())))
                    {
                        continue;
                    }

                    foreach (var target in targets)
                    {
                        if (childchild.name.ToLower().Contains(target.ToLower()))
                        {
                            var warp = childchild.gameObject.AddComponent<ClothWarpRoot>();
                            //     Name = name,
                            //     CollisionMask = mask,
                            warp.BaseSettings.Radius = 0.02f;
                            //     Connection = type
                            transforms.Add(warp);
                            break;
                        }
                    }
                }
            }
            if (transforms.Count == 0)
            {
                group = default;
                return false;
            }

            group = transforms;
            return true;
        }

        static bool TryAddGroup(Animator animator, HumanBodyBones humanBone, string[] targets,
            out List<ClothWarpRoot> group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                UniGLTFLogger.Warning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<ClothWarpRoot> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (var target in targets)
                {
                    if (child.name.ToLower().Contains(target.ToLower()))
                    {
                        var warp = child.gameObject.AddComponent<ClothWarpRoot>();
                        if (warp != null)
                        {
                            // CollisionMask = mask,
                            warp.BaseSettings.Radius = 0.02f;
                            // Connection = type
                            transforms.Add(warp);
                        }
                        break;
                    }
                }
            }
            if (transforms.Count == 0)
            {
                group = default;
                return false;
            }

            group = transforms;
            return true;
        }
    }
}