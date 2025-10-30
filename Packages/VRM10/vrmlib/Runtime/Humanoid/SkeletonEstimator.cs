using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VrmLib
{
    public static class SkeletonEstimator
    {
        static Node GetRoot(IReadOnlyList<Node> bones)
        {
            var hips = bones.Where(x => x.Parent == null).ToArray();
            if (hips.Length != 1)
            {
                throw new System.Exception("Require unique root");
            }
            return hips[0];
        }

        static Node SelectBone(Func<Node, Node, bool> pred, Node parent)
        {
            var bones = parent.Children;
            if (bones == null || bones.Count == 0) throw new Exception("no bones");
            foreach (var bone in bones)
            {
                if (pred(bone, parent))
                {
                    return bone;
                }
            }

            throw new Exception("not found");
        }

        static void GetSpineAndHips(Node hips, out Node spine, out Node leg_L, out Node leg_R)
        {
            if (hips.Children.Count != 3) throw new System.Exception("Hips require 3 children");
            spine = SelectBone((l, r) => l.CenterOfDescendant().y > r.SkeletonLocalPosition.y, hips);
            var s = spine;
            try
            {
                leg_L = SelectBone((l, r) => !l.Equals(s) && l.CenterOfDescendant().x < r.SkeletonLocalPosition.x, hips);
                leg_R = SelectBone((l, r) => !l.Equals(s) && l.CenterOfDescendant().x > r.SkeletonLocalPosition.x, hips);
            }
            catch (Exception)
            {
                // Z軸で左右を代用
                leg_L = SelectBone((l, r) => !l.Equals(s) && l.CenterOfDescendant().z < r.SkeletonLocalPosition.z, hips);
                leg_R = SelectBone((l, r) => !l.Equals(s) && l.CenterOfDescendant().z > r.SkeletonLocalPosition.z, hips);
            }
        }

        static void GetNeckAndArms(Node chest, out Node neck, out Node arm_L, out Node arm_R, Func<Vector3, Vector3, bool> isLeft)
        {
            if (chest.Children.Count != 3) throw new System.Exception("Chest require 3 children");
            neck = SelectBone((l, r) => l.CenterOfDescendant().y > r.SkeletonLocalPosition.y, chest);
            var n = neck;
            arm_L = SelectBone((l, r) => !l.Equals(n) && isLeft(l.CenterOfDescendant(), r.SkeletonLocalPosition), chest);
            arm_R = SelectBone((l, r) => !l.Equals(n) && !isLeft(l.CenterOfDescendant(), r.SkeletonLocalPosition), chest);
        }

        struct Arm
        {
            public Node Shoulder;
            public Node UpperArm;
            public Node LowerArm;
            public Node Hand;
        }

        static Arm GetArm(Node shoulder)
        {
            var bones = shoulder.Traverse().ToArray();
            switch (bones.Length)
            {
                case 0:
                case 1:
                case 2:
                    throw new NotImplementedException();

                case 3:
                    return new Arm
                    {
                        UpperArm = bones[0],
                        LowerArm = bones[1],
                        Hand = bones[2],
                    };

                default:
                    return new Arm
                    {
                        Shoulder = bones[0],
                        UpperArm = bones[1],
                        LowerArm = bones[2],
                        Hand = bones[3],
                    };
            }
        }

        struct Leg
        {
            public Node UpperLeg;
            public Node LowerLeg;
            public Node Foot;
            public Node Toes;
        }

        static Leg GetLeg(Node leg)
        {
            var bones = leg.Traverse().Where(x => string.IsNullOrEmpty(x.Name) || !x.Name.ToLower().Contains("buttock")).ToArray();
            switch (bones.Length)
            {
                case 0:
                case 1:
                case 2:
                    throw new NotImplementedException();

                case 3:
                    return new Leg
                    {
                        UpperLeg = bones[0],
                        LowerLeg = bones[1],
                        Foot = bones[2],
                    };

                default:
                    return new Leg
                    {
                        UpperLeg = bones[bones.Length - 4],
                        LowerLeg = bones[bones.Length - 3],
                        Foot = bones[bones.Length - 2],
                        Toes = bones[bones.Length - 1],
                    };
            }
        }

        static public Dictionary<HumanoidBones, Node> DetectByName(Node root, Dictionary<string, HumanoidBones> map)
        {
            var dictionary = new Dictionary<HumanoidBones, Node>();
            foreach (var bone in root.Traverse())
            {
                if (map.TryGetValue(bone.Name, out HumanoidBones humanbone))
                {
                    if (humanbone != HumanoidBones.unknown)
                    {
                        dictionary.Add(humanbone, bone);
                    }
                }
                else if (Enum.TryParse<HumanoidBones>(bone.Name, true, out HumanoidBones result))
                {
                    humanbone = (HumanoidBones)result;
                    dictionary.Add(humanbone, bone);
                }
                else
                {
                    // throw new NotImplementedException();
                }
            }
            return dictionary;
        }

        static public Dictionary<HumanoidBones, Node> DetectByPosition(Node root)
        {
            var hips = root.Traverse().First(x =>
            {
                // 3分岐以上で
                //
                // 子孫が以下の構成持ちうるもの
                //
                // spine, head, (upper, lower, hand) x 2
                // (upper, lower, foot)
                // (upper, lower, foot)
                return x.Children.Where(y => y.Traverse().Count() >= 3).Count() >= 3;
            });

            Node spine, hip_L, hip_R;
            GetSpineAndHips(hips, out spine, out hip_L, out hip_R);
            if (hip_L.Equals(hip_R))
            {
                throw new Exception();
            }
            var legLeft = GetLeg(hip_L);
            var legRight = GetLeg(hip_R);

            var spineToChest = new List<Node>();
            foreach (var x in spine.Traverse())
            {
                spineToChest.Add(x);
                if (x.Children.Count == 3) break;
            }

            Func<Vector3, Vector3, bool> isLeft = default(Func<Vector3, Vector3, bool>);
            if (legLeft.UpperLeg.SkeletonLocalPosition.z == legRight.UpperLeg.SkeletonLocalPosition.z)
            {
                isLeft = (l, r) => l.x < r.x;
            }
            else
            {
                isLeft = (l, r) => l.z < r.z;
            }

            Node neck, shoulder_L, shoulder_R;
            GetNeckAndArms(spineToChest.Last(), out neck, out shoulder_L, out shoulder_R, isLeft);
            var armLeft = GetArm(shoulder_L);
            var armRight = GetArm(shoulder_R);

            var neckToHead = neck.Traverse().ToArray();

            //
            //  set result
            //
            var skeleton = new Dictionary<HumanoidBones, Node>();
            Action<HumanoidBones, Node> AddBoneToSkeleton = (b, t) =>
            {
                if (t != null)
                {
                    t.HumanoidBone = b;
                    skeleton[b] = t;
                }
            };

            AddBoneToSkeleton(HumanoidBones.hips, hips);

            switch (spineToChest.Count)
            {
                case 0:
                    throw new Exception();

                case 1:
                    AddBoneToSkeleton(HumanoidBones.spine, spineToChest[0]);
                    break;

                case 2:
                    AddBoneToSkeleton(HumanoidBones.spine, spineToChest[0]);
                    AddBoneToSkeleton(HumanoidBones.chest, spineToChest[1]);
                    break;

                case 3:
                    AddBoneToSkeleton(HumanoidBones.spine, spineToChest[0]);
                    AddBoneToSkeleton(HumanoidBones.chest, spineToChest[1]);
                    AddBoneToSkeleton(HumanoidBones.upperChest, spineToChest[2]);
                    break;

                default:
                    AddBoneToSkeleton(HumanoidBones.spine, spineToChest[0]);
                    AddBoneToSkeleton(HumanoidBones.chest, spineToChest[1]);
                    AddBoneToSkeleton(HumanoidBones.upperChest, spineToChest.Last());
                    break;
            }

            switch (neckToHead.Length)
            {
                case 0:
                    throw new Exception();

                case 1:
                    AddBoneToSkeleton(HumanoidBones.head, neckToHead[0]);
                    break;

                case 2:
                    AddBoneToSkeleton(HumanoidBones.neck, neckToHead[0]);
                    AddBoneToSkeleton(HumanoidBones.head, neckToHead[1]);
                    break;

                default:
                    AddBoneToSkeleton(HumanoidBones.neck, neckToHead[0]);
                    AddBoneToSkeleton(HumanoidBones.head, neckToHead.Where(x => x.Parent.Children.Count == 1).Last());
                    break;
            }

            AddBoneToSkeleton(HumanoidBones.leftUpperLeg, legLeft.UpperLeg);
            AddBoneToSkeleton(HumanoidBones.leftLowerLeg, legLeft.LowerLeg);
            AddBoneToSkeleton(HumanoidBones.leftFoot, legLeft.Foot);
            AddBoneToSkeleton(HumanoidBones.leftToes, legLeft.Toes);

            AddBoneToSkeleton(HumanoidBones.rightUpperLeg, legRight.UpperLeg);
            AddBoneToSkeleton(HumanoidBones.rightLowerLeg, legRight.LowerLeg);
            AddBoneToSkeleton(HumanoidBones.rightFoot, legRight.Foot);
            AddBoneToSkeleton(HumanoidBones.rightToes, legRight.Toes);

            AddBoneToSkeleton(HumanoidBones.leftShoulder, armLeft.Shoulder);
            AddBoneToSkeleton(HumanoidBones.leftUpperArm, armLeft.UpperArm);
            AddBoneToSkeleton(HumanoidBones.leftLowerArm, armLeft.LowerArm);
            AddBoneToSkeleton(HumanoidBones.leftHand, armLeft.Hand);

            AddBoneToSkeleton(HumanoidBones.rightShoulder, armRight.Shoulder);
            AddBoneToSkeleton(HumanoidBones.rightUpperArm, armRight.UpperArm);
            AddBoneToSkeleton(HumanoidBones.rightLowerArm, armRight.LowerArm);
            AddBoneToSkeleton(HumanoidBones.rightHand, armRight.Hand);

            return skeleton;
        }

        // MVN
        static Dictionary<string, HumanoidBones> s_nameBoneMap = new Dictionary<string, HumanoidBones>
        {
            {"Spine1", HumanoidBones.chest},
            {"Spine2", HumanoidBones.upperChest},
            {"LeftShoulder", HumanoidBones.leftShoulder},
            {"LeftArm", HumanoidBones.leftUpperArm},
            {"LeftForeArm", HumanoidBones.leftLowerArm},
            {"RightShoulder", HumanoidBones.rightShoulder},
            {"RightArm", HumanoidBones.rightUpperArm},
            {"RightForeArm", HumanoidBones.rightLowerArm},
            {"LeftUpLeg", HumanoidBones.leftUpperLeg},
            {"LeftLeg", HumanoidBones.leftLowerLeg},
            {"LeftToeBase", HumanoidBones.leftToes},
            {"RightUpLeg", HumanoidBones.rightUpperLeg},
            {"RightLeg", HumanoidBones.rightLowerLeg},
            {"RightToeBase", HumanoidBones.rightToes},
        };

        static Dictionary<string, HumanoidBones> s_nameBoneMapLA = new Dictionary<string, HumanoidBones>
        {
            { "Chest", HumanoidBones.spine},
            { "Chest2", HumanoidBones.chest},
            { "LeftCollar", HumanoidBones.leftShoulder},
            { "LeftShoulder", HumanoidBones.leftUpperArm},
            { "LeftElbow", HumanoidBones.leftLowerArm},
            { "LeftWrist", HumanoidBones.leftHand},
            { "RightCollar", HumanoidBones.rightShoulder},
            { "RightShoulder", HumanoidBones.rightUpperArm},
            { "RightElbow", HumanoidBones.rightLowerArm},
            { "RightWrist", HumanoidBones.rightHand},
            { "LeftHip", HumanoidBones.leftUpperLeg},
            { "LeftKnee", HumanoidBones.leftLowerLeg},
            { "LeftAnkle", HumanoidBones.leftFoot},
            { "RightHip", HumanoidBones.rightUpperLeg},
            { "RightKnee", HumanoidBones.rightLowerLeg},
            { "RightAnkle", HumanoidBones.rightFoot},
        };

        static Dictionary<string, HumanoidBones> s_nameBoneMapAccad = new Dictionary<string, HumanoidBones>
        {
            { "root", HumanoidBones.hips},
            { "lowerback", HumanoidBones.spine},
            { "upperback", HumanoidBones.chest},
            { "thorax", HumanoidBones.upperChest},
            { "neck", HumanoidBones.neck},
            { "head", HumanoidBones.head},
            { "lshoulderjoint", HumanoidBones.leftShoulder},
            { "lhumerus", HumanoidBones.leftUpperArm},
            { "lradius", HumanoidBones.leftLowerArm},
            { "lhand", HumanoidBones.leftHand},
            { "rshoulderjoint", HumanoidBones.rightShoulder},
            { "rhumerus", HumanoidBones.rightUpperArm},
            { "rradius", HumanoidBones.rightLowerArm},
            { "rhand", HumanoidBones.rightHand},
            { "rfemur", HumanoidBones.rightUpperLeg},
            { "rtibia", HumanoidBones.rightLowerLeg},
            { "rfoot", HumanoidBones.rightFoot},
            { "rtoes", HumanoidBones.rightToes},
            { "lfemur", HumanoidBones.leftUpperLeg},
            { "ltibia", HumanoidBones.leftLowerLeg},
            { "lfoot", HumanoidBones.leftFoot},
            { "ltoes", HumanoidBones.leftToes},
        };

        static HumanoidBones[] RequiredBones = new HumanoidBones[]
        {
            HumanoidBones.hips,
            HumanoidBones.spine,
            HumanoidBones.head,
            HumanoidBones.leftUpperArm,
            HumanoidBones.leftLowerArm,
            HumanoidBones.leftHand,
            HumanoidBones.leftUpperLeg,
            HumanoidBones.leftLowerLeg,
            HumanoidBones.leftFoot,
            HumanoidBones.rightUpperArm,
            HumanoidBones.rightLowerArm,
            HumanoidBones.rightHand,
            HumanoidBones.rightUpperLeg,
            HumanoidBones.rightLowerLeg,
            HumanoidBones.rightFoot,
         };

        static bool HasAllHumanRequiredBone(Dictionary<HumanoidBones, Node> dict)
        {
            foreach (var bone in RequiredBones)
            {
                if (!dict.ContainsKey(bone))
                {
                    return false;
                }
            }
            return true;
        }

        static Dictionary<HumanoidBones, Node> _Detect(Node root)
        {
            var list = new List<Dictionary<HumanoidBones, Node>>();
            foreach (var map in new[] { s_nameBoneMap, s_nameBoneMapLA, s_nameBoneMapAccad })
            {
                try
                {
                    var result = DetectByName(root, map);
                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
                catch (Exception)
                { }
            }

            foreach (var map in list.OrderByDescending(x => x.Count))
            {
                if (HasAllHumanRequiredBone(map))
                {
                    return map;
                }
            }

            return DetectByPosition(root);
        }

        static public Dictionary<HumanoidBones, Node> Detect(Node root)
        {
            try
            {
                var dict = _Detect(root);
                foreach (var kv in dict)
                {
                    kv.Value.HumanoidBone = kv.Key;
                }
                return dict;
            }
            catch
            {
                return null;
            }
        }
    }
}
