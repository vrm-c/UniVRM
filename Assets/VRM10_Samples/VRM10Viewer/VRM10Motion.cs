using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UniJSON;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Motion
    {
        public (INormalizedPoseProvider, ITPoseProvider) ControlRig;

        UniHumanoid.BvhImporterContext m_context;
        UniGLTF.RuntimeGltfInstance m_instance;

        public Transform Root => m_context?.Root.transform;

        public VRM10Motion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            var provider = new AnimatorPoseProvider(m_context.Root.transform, m_context.Root.GetComponent<Animator>());
            ControlRig = (provider, provider);
        }

        public VRM10Motion(UniGLTF.RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }
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

        static int? GetNodeIndex(UniGLTF.Extensions.VRMC_vrm_animation.Humanoid humanoid, HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips: return humanoid.Hips?.Node;
                case HumanBodyBones.LeftUpperLeg: return humanoid.LeftUpperLeg?.Node;
                case HumanBodyBones.RightUpperLeg: return humanoid.RightUpperLeg?.Node;
                case HumanBodyBones.LeftLowerLeg: return humanoid.LeftLowerLeg?.Node;
                case HumanBodyBones.RightLowerLeg: return humanoid.RightLowerLeg?.Node;
                case HumanBodyBones.LeftFoot: return humanoid.LeftFoot?.Node;
                case HumanBodyBones.RightFoot: return humanoid.RightFoot?.Node;
                case HumanBodyBones.Spine: return humanoid.Spine?.Node;
                case HumanBodyBones.Chest: return humanoid.Chest?.Node;
                case HumanBodyBones.Neck: return humanoid.Neck?.Node;
                case HumanBodyBones.Head: return humanoid.Head?.Node;
                case HumanBodyBones.LeftShoulder: return humanoid.LeftShoulder?.Node;
                case HumanBodyBones.RightShoulder: return humanoid.RightShoulder?.Node;
                case HumanBodyBones.LeftUpperArm: return humanoid.LeftUpperArm?.Node;
                case HumanBodyBones.RightUpperArm: return humanoid.RightUpperArm?.Node;
                case HumanBodyBones.LeftLowerArm: return humanoid.LeftLowerArm?.Node;
                case HumanBodyBones.RightLowerArm: return humanoid.RightLowerArm?.Node;
                case HumanBodyBones.LeftHand: return humanoid.LeftHand?.Node;
                case HumanBodyBones.RightHand: return humanoid.RightHand?.Node;
                case HumanBodyBones.LeftToes: return humanoid.LeftToes?.Node;
                case HumanBodyBones.RightToes: return humanoid.RightToes?.Node;
                // case HumanBodyBones.LeftEye: return humanoid.LeftEye?.Node;
                // case HumanBodyBones.RightEye: return humanoid.RightEye?.Node;
                case HumanBodyBones.Jaw: return humanoid.Jaw?.Node;
                case HumanBodyBones.LeftThumbProximal: return humanoid.LeftThumbMetacarpal?.Node; // Metacarpal
                case HumanBodyBones.LeftThumbIntermediate: return humanoid.LeftThumbProximal?.Node; // Proximal
                case HumanBodyBones.LeftThumbDistal: return humanoid.LeftThumbDistal?.Node;
                case HumanBodyBones.LeftIndexProximal: return humanoid.LeftIndexProximal?.Node;
                case HumanBodyBones.LeftIndexIntermediate: return humanoid.LeftIndexIntermediate?.Node;
                case HumanBodyBones.LeftIndexDistal: return humanoid.LeftIndexDistal?.Node;
                case HumanBodyBones.LeftMiddleProximal: return humanoid.LeftMiddleProximal?.Node;
                case HumanBodyBones.LeftMiddleIntermediate: return humanoid.LeftMiddleIntermediate?.Node;
                case HumanBodyBones.LeftMiddleDistal: return humanoid.LeftMiddleDistal?.Node;
                case HumanBodyBones.LeftRingProximal: return humanoid.LeftRingProximal?.Node;
                case HumanBodyBones.LeftRingIntermediate: return humanoid.LeftRingIntermediate?.Node;
                case HumanBodyBones.LeftRingDistal: return humanoid.LeftRingDistal?.Node;
                case HumanBodyBones.LeftLittleProximal: return humanoid.LeftLittleProximal?.Node;
                case HumanBodyBones.LeftLittleIntermediate: return humanoid.LeftLittleIntermediate?.Node;
                case HumanBodyBones.LeftLittleDistal: return humanoid.LeftLittleDistal?.Node;
                case HumanBodyBones.RightThumbProximal: return humanoid.RightThumbMetacarpal?.Node; // Metacarpal
                case HumanBodyBones.RightThumbIntermediate: return humanoid.RightThumbProximal?.Node; // Proximal
                case HumanBodyBones.RightThumbDistal: return humanoid.RightThumbDistal?.Node;
                case HumanBodyBones.RightIndexProximal: return humanoid.RightIndexProximal?.Node;
                case HumanBodyBones.RightIndexIntermediate: return humanoid.RightIndexIntermediate?.Node;
                case HumanBodyBones.RightIndexDistal: return humanoid.RightIndexDistal?.Node;
                case HumanBodyBones.RightMiddleProximal: return humanoid.RightMiddleProximal?.Node;
                case HumanBodyBones.RightMiddleIntermediate: return humanoid.RightMiddleIntermediate?.Node;
                case HumanBodyBones.RightMiddleDistal: return humanoid.RightMiddleDistal?.Node;
                case HumanBodyBones.RightRingProximal: return humanoid.RightRingProximal?.Node;
                case HumanBodyBones.RightRingIntermediate: return humanoid.RightRingIntermediate?.Node;
                case HumanBodyBones.RightRingDistal: return humanoid.RightRingDistal?.Node;
                case HumanBodyBones.RightLittleProximal: return humanoid.RightLittleProximal?.Node;
                case HumanBodyBones.RightLittleIntermediate: return humanoid.RightLittleIntermediate?.Node;
                case HumanBodyBones.RightLittleDistal: return humanoid.RightLittleDistal?.Node;
                case HumanBodyBones.UpperChest: return humanoid.UpperChest?.Node;
            }
            return default;
        }

        static Dictionary<HumanBodyBones, Transform> GetHumanMap(GltfData data, IReadOnlyList<Transform> nodes)
        {
            var humanMap = new Dictionary<HumanBodyBones, Transform>();


            if (data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        var animation = UniGLTF.Extensions.VRMC_vrm_animation.GltfDeserializer.Deserialize(kv.Value);
                        foreach (HumanBodyBones bone in UniGLTF.Utils.CachedEnum.GetValues<HumanBodyBones>())
                        {
                            // Debug.Log($"{bone} => {index}");
                            var node = GetNodeIndex(animation.Humanoid, bone);
                            if (node.HasValue)
                            {
                                humanMap.Add(bone, nodes[node.Value]);
                            }
                        }
                    }
                }
            }
            return humanMap;
        }

        public static async Task<VRM10Motion> LoadVrmAnimationFromPathAsync(string path)
        {
            //
            // GetHumanoid Mapping
            //
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data))
            {
                loader.InvertAxis = Axes.X;
                // loader.PositionScaling = 0.01f;
                var instance = await loader.LoadAsync(new ImmediateCaller());
                var humanMap = GetHumanMap(data, loader.Nodes);
                if (humanMap.Count == 0)
                {
                    throw new ArgumentException("fail to load VRMC_vrm_animation");
                }

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
                var provider = new InitRotationPoseProvider(instance.transform, humanoid);
                motion.ControlRig = (provider, provider);
                return motion;
            }
        }
    }
}
