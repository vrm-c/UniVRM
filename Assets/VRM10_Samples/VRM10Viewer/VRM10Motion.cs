using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Utils;
using UniHumanoid;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Motion
    {
        public IControlRigGetter ControlRig;

        UniHumanoid.BvhImporterContext m_context;
        UniGLTF.RuntimeGltfInstance m_instance;

        public Transform Root => m_context?.Root.transform;

        public VRM10Motion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            ControlRig = new NormalizedRigGetter(m_context.Root.GetComponent<Animator>());
        }

        public VRM10Motion(UniGLTF.RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }
            ControlRig = new InitRotationGetter(); //instance.GetComponent<Animator>());
        }

        public void ShowBoxMan(bool showBoxMan)
        {
            if (m_context != null)
            {
                m_context.Root.GetComponent<SkinnedMeshRenderer>().enabled = showBoxMan;
            }
        }

        public static VRM10Motion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new VRM10Motion(context);
        }

        public static VRM10Motion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        static IEnumerable<Transform> Traverse(Transform t)
        {
            yield return t;
            foreach (Transform child in t)
            {
                foreach (var x in Traverse(child))
                {
                    yield return x;
                }
            }
        }

        static Dictionary<string, HumanBodyBones> s_nameMap = new Dictionary<string, HumanBodyBones>()
        {
            {"mixamorigHips", HumanBodyBones.Hips},
            {"mixamorigSpine", HumanBodyBones.Spine},
            {"mixamorigSpine1", HumanBodyBones.Chest},
            {"mixamorigSpine2", HumanBodyBones.UpperChest},
            {"mixamorigNeck", HumanBodyBones.Neck},
            {"mixamorigHead", HumanBodyBones.Head},

            {"mixamorigLeftShoulder", HumanBodyBones.LeftShoulder},
            {"mixamorigLeftArm", HumanBodyBones.LeftUpperArm},
            {"mixamorigLeftForeArm", HumanBodyBones.LeftLowerArm},
            {"mixamorigLeftHand", HumanBodyBones.LeftHand},

            {"mixamorigRightShoulder", HumanBodyBones.RightShoulder},
            {"mixamorigRightArm", HumanBodyBones.RightUpperArm},
            {"mixamorigRightForeArm", HumanBodyBones.RightLowerArm},
            {"mixamorigRightHand", HumanBodyBones.RightHand},

            {"mixamorigLeftUpLeg", HumanBodyBones.LeftUpperLeg},
            {"mixamorigLeftLeg", HumanBodyBones.LeftLowerLeg},
            {"mixamorigLeftFoot", HumanBodyBones.LeftFoot},
            {"mixamorigLeftToeBase", HumanBodyBones.LeftToes},

            {"mixamorigRightUpLeg", HumanBodyBones.RightUpperLeg},
            {"mixamorigRightLeg", HumanBodyBones.RightLowerLeg},
            {"mixamorigRightFoot", HumanBodyBones.RightFoot},
            {"mixamorigRightToeBase", HumanBodyBones.RightToes},
        };

        static float ForceMeterScale(Dictionary<HumanBodyBones, Transform> map)
        {
            var positionMap = map.ToDictionary(kv => kv.Key, kv => kv.Value.position);
            var hipsHeight = positionMap[HumanBodyBones.Hips].y;
            float scaling = 0.01f;
            // foreach (var t in Traverse(map[HumanBodyBones.Hips]))
            // {
            //     t.position = t.position * scaling;
            // }
            return scaling;
        }

        // TODO: vrm-animation
        // https://github.com/vrm-c/vrm-animation
        public static async Task<VRM10Motion> LoadVrmAnimationFromPathAsync(string path)
        {
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data))
            {
                loader.InvertAxis = Axes.X;
                loader.PositionScaling = 0.01f;
                var instance = await loader.LoadAsync(new ImmediateCaller());

                // GetHumanoid Mapping
                var humanMap = new Dictionary<HumanBodyBones, Transform>();
                foreach (var t in Traverse(instance.transform))
                {
                    if (s_nameMap.TryGetValue(t.name, out var bone))
                    {
                        humanMap[bone] = t;
                    }
                }
                var scaling = ForceMeterScale(humanMap);
                // instance.transform.localScale = new Vector3(scaling, scaling, scaling);
                var description = AvatarDescription.Create(humanMap);

                //
                // avatar
                //
                var avatar = description.CreateAvatar(instance.Root.transform);
                avatar.name = "Avatar";
                // AvatarDescription = description;
                var animator = instance.gameObject.AddComponent<Animator>();
                animator.avatar = avatar;

                // create SkinnedMesh for bone visualize
                var renderer = SkeletonMeshUtility.CreateRenderer(animator);
                var material = new Material(Shader.Find("Standard"));
                renderer.sharedMaterial = material;
                var mesh = renderer.sharedMesh;
                mesh.name = "box-man";

                var humanoid = instance.gameObject.AddComponent<Humanoid>();
                humanoid.AssignBonesFromAnimator();
                var motion = new VRM10Motion(instance);
                motion.ControlRig = new Vrm10InitRotationGetter(); //humanoid, instance.transform, humanMap.ToDictionary(kv => kv.Key, kv => kv.Value.rotation));
                return motion;
            }
        }

        public void Retarget(IControlRigSetter dst)
        {
            ControlRigUtil.Retarget(ControlRig, dst);
        }

        // /// <summary>
        // /// from v0.104
        // /// </summary>
        // public static void UpdateControlRigImplicit(Animator src, Animator dst)
        // {
        //     // var dst = m_controller.GetComponent<Animator>();

        //     foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
        //     {
        //         if (bone == HumanBodyBones.LastBone)
        //         {
        //             continue;
        //         }

        //         var boneTransform = dst.GetBoneTransform(bone);
        //         if (boneTransform == null)
        //         {
        //             continue;
        //         }

        //         var bvhBone = src.GetBoneTransform(bone);
        //         if (bvhBone != null)
        //         {
        //             // set normalized pose
        //             boneTransform.localRotation = bvhBone.localRotation;
        //         }

        //         if (bone == HumanBodyBones.Hips)
        //         {
        //             // TODO: hips position scaling ?
        //             boneTransform.localPosition = bvhBone.localPosition;
        //         }
        //     }
        // }

        // /// <summary>
        // /// from v0.108
        // /// </summary>
        // public static void UpdateControlRigImplicit(UniHumanoid.Humanoid src, Animator dst)
        // {
        //     foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
        //     {
        //         if (bone == HumanBodyBones.LastBone)
        //         {
        //             continue;
        //         }

        //         var boneTransform = dst.GetBoneTransform(bone);
        //         if (boneTransform == null)
        //         {
        //             continue;
        //         }

        //         var bvhBone = src.GetBoneTransform(bone);
        //         if (bvhBone != null)
        //         {
        //             // set normalized pose
        //             boneTransform.localRotation = bvhBone.localRotation;
        //             if (bone == HumanBodyBones.Hips)
        //             {
        //                 // TODO: hips position scaling ?
        //                 boneTransform.localPosition = bvhBone.localPosition;
        //             }
        //         }
        //     }
        // }

        // public static void UpdateControlRig(Vrm10RuntimeControlRig src, Animator dst)
        // {
        //     foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
        //     {
        //         if (bone == HumanBodyBones.LastBone)
        //         {
        //             continue;
        //         }

        //         var boneTransform = dst.GetBoneTransform(bone);
        //         if (boneTransform == null)
        //         {
        //             continue;
        //         }

        //         if (src.TryGetRigBone(bone, out var bvhBone))
        //         {
        //             // set normalized pose
        //             bvhBone.ControlBone.localRotation = bvhBone.ControlTarget.localRotation;
        //             boneTransform.localRotation = bvhBone.NormalizedLocalRotation;
        //             if (bone == HumanBodyBones.Hips)
        //             {
        //                 // TODO: hips position scaling ?
        //                 boneTransform.localPosition = bvhBone.ControlTarget.localPosition;
        //             }
        //         }
        //     }
        // }

    }
}
