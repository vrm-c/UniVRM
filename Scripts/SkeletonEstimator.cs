using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniHumanoid
{
    public interface ISkeletonDetector
    {
        Skeleton Detect(IList<IBone> bones);
    }


    public class BvhSkeletonEstimator : ISkeletonDetector
    {
        static IBone GetRoot(IList<IBone> bones)
        {
            var hips = bones.Where(x => x.Parent == null).ToArray();
            if (hips.Length != 1)
            {
                throw new System.Exception("Require unique root");
            }
            return hips[0];
        }

        static IBone SelectBone(Func<IBone, IBone, IBone> selector, IList<IBone> bones)
        {
            if (bones == null || bones.Count == 0) throw new Exception("no bones");
            var current = bones[0];
            for (var i = 1; i < bones.Count; ++i)
            {
                current = selector(current, bones[i]);
            }
            return current;
        }

        static void GetSpineAndHips(IBone hips, out IBone spine, out IBone leg_L, out IBone leg_R)
        {
            if (hips.Children.Count != 3) throw new System.Exception("Hips require 3 children");
            spine = SelectBone((l, r) => l.CenterOfDescendant().y > r.CenterOfDescendant().y ? l : r, hips.Children);
            leg_L = SelectBone((l, r) => l.CenterOfDescendant().x < r.CenterOfDescendant().x ? l : r, hips.Children);
            leg_R = SelectBone((l, r) => l.CenterOfDescendant().x > r.CenterOfDescendant().x ? l : r, hips.Children);
        }

        static void GetNeckAndArms(IBone chest, out IBone neck, out IBone arm_L, out IBone arm_R)
        {
            if (chest.Children.Count != 3) throw new System.Exception("Chest require 3 children");
            neck = SelectBone((l, r) => l.CenterOfDescendant().y > r.CenterOfDescendant().y ? l : r, chest.Children);
            arm_L = SelectBone((l, r) => l.CenterOfDescendant().x < r.CenterOfDescendant().x ? l : r, chest.Children);
            arm_R = SelectBone((l, r) => l.CenterOfDescendant().x > r.CenterOfDescendant().x ? l : r, chest.Children);
        }

        struct Arm
        {
            public IBone Shoulder;
            public IBone UpperArm;
            public IBone LowerArm;
            public IBone Hand;
        }

        Arm GetArm(IBone shoulder)
        {
            var bones = shoulder.Traverse().ToArray();
            switch (bones.Length)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    throw new NotImplementedException();

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
            public IBone UpperLeg;
            public IBone LowerLeg;
            public IBone Foot;
            public IBone Toes;
        }

        Leg GetLeg(IBone leg)
        {
            var bones = leg.Traverse().Where(x => !x.Name.ToLower().Contains("buttock")).ToArray();
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

        public Skeleton Detect(IList<IBone> bones)
        {
            //
            // search bones
            //
            var root = GetRoot(bones);
            var hips = root.Traverse().First(x => x.Children.Count == 3);

            IBone spine, hip_L, hip_R;
            GetSpineAndHips(hips, out spine, out hip_L, out hip_R);
            var legLeft = GetLeg(hip_L);
            var legRight = GetLeg(hip_R);

            var spineToChest = new List<IBone>();
            foreach(var x in spine.Traverse())
            {
                spineToChest.Add(x);
                if (x.Children.Count == 3) break;
            }

            IBone neck, shoulder_L, shoulder_R;
            GetNeckAndArms(spineToChest.Last(), out neck, out shoulder_L, out shoulder_R);
            var armLeft = GetArm(shoulder_L);
            var armRight = GetArm(shoulder_R);

            var neckToHead = neck.Traverse().ToArray();

            //
            //  set result
            //
            var skeleton = new Skeleton();
            skeleton.Set(HumanBodyBones.Hips, bones, hips);

            switch (spineToChest.Count)
            {
                case 0:
                    throw new Exception();

                case 1:
                    skeleton.Set(HumanBodyBones.Spine, bones, spineToChest[0]);
                    break;

                case 2:
                    skeleton.Set(HumanBodyBones.Spine, bones, spineToChest[0]);
                    skeleton.Set(HumanBodyBones.Chest, bones, spineToChest[1]);
                    break;

#if UNITY_5_6_OR_NEWER
                case 3:
                    skeleton.Set(HumanBodyBones.Spine, bones, spineToChest[0]);
                    skeleton.Set(HumanBodyBones.Chest, bones, spineToChest[1]);
                    skeleton.Set(HumanBodyBones.UpperChest, bones, spineToChest[2]);
                    break;
#endif

                default:
                    skeleton.Set(HumanBodyBones.Spine, bones, spineToChest[0]);
#if UNITY_5_6_OR_NEWER
                    skeleton.Set(HumanBodyBones.Chest, bones, spineToChest[1]);
                    skeleton.Set(HumanBodyBones.UpperChest, bones, spineToChest.Last());
#else
                    skeleton.Set(HumanBodyBones.Chest, bones, spineToChest.Last());
#endif
                    break;
            }

            switch (neckToHead.Length)
            {
                case 0:
                    throw new Exception();

                case 1:
                    skeleton.Set(HumanBodyBones.Head, bones, neckToHead[0]);
                    break;

                case 2:
                    skeleton.Set(HumanBodyBones.Neck, bones, neckToHead[0]);
                    skeleton.Set(HumanBodyBones.Head, bones, neckToHead[1]);
                    break;

                default:
                    skeleton.Set(HumanBodyBones.Neck, bones, neckToHead[0]);
                    skeleton.Set(HumanBodyBones.Head, bones, neckToHead.Where(x => x.Parent.Children.Count==1).Last());
                    break;
            }

            skeleton.Set(HumanBodyBones.LeftUpperLeg, bones, legLeft.UpperLeg);
            skeleton.Set(HumanBodyBones.LeftLowerLeg, bones, legLeft.LowerLeg);
            skeleton.Set(HumanBodyBones.LeftFoot, bones, legLeft.Foot);
            skeleton.Set(HumanBodyBones.LeftToes, bones, legLeft.Toes);

            skeleton.Set(HumanBodyBones.RightUpperLeg, bones, legRight.UpperLeg);
            skeleton.Set(HumanBodyBones.RightLowerLeg, bones, legRight.LowerLeg);
            skeleton.Set(HumanBodyBones.RightFoot, bones, legRight.Foot);
            skeleton.Set(HumanBodyBones.RightToes, bones, legRight.Toes);

            skeleton.Set(HumanBodyBones.LeftShoulder, bones, armLeft.Shoulder);
            skeleton.Set(HumanBodyBones.LeftUpperArm, bones, armLeft.UpperArm);
            skeleton.Set(HumanBodyBones.LeftLowerArm, bones, armLeft.LowerArm);
            skeleton.Set(HumanBodyBones.LeftHand, bones, armLeft.Hand);

            skeleton.Set(HumanBodyBones.RightShoulder, bones, armRight.Shoulder);
            skeleton.Set(HumanBodyBones.RightUpperArm, bones, armRight.UpperArm);
            skeleton.Set(HumanBodyBones.RightLowerArm, bones, armRight.LowerArm);
            skeleton.Set(HumanBodyBones.RightHand, bones, armRight.Hand);

            return skeleton;
        }

        public Skeleton Detect(Bvh bvh)
        {
            var root = new BvhBone(bvh.Root.Name, Vector3.zero);
            root.Build(bvh.Root);
            return Detect(root.Traverse().Select(x => (IBone)x).ToList());
        }

        public Skeleton Detect(Transform t)
        {
            var root = new BvhBone(t.name, Vector3.zero);
            root.Build(t);
            return Detect(root.Traverse().Select(x => (IBone)x).ToList());
        }
    }
}
