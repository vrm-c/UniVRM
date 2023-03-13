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
    public interface IMotion : IDisposable
    {
        (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; }
        void ShowBoxMan(bool enable);
    }

    public class BvhMotion : IMotion
    {
        UniHumanoid.BvhImporterContext m_context;
        public Transform Root => m_context?.Root.transform;
        public SkinnedMeshRenderer m_boxMan;
        public SkinnedMeshRenderer BoxMan => m_boxMan;
        (INormalizedPoseProvider, ITPoseProvider) m_controlRig;
        (INormalizedPoseProvider, ITPoseProvider) IMotion.ControlRig => m_controlRig;
        public BvhMotion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            var provider = new AnimatorPoseProvider(m_context.Root.transform, m_context.Root.GetComponent<Animator>());
            m_controlRig = (provider, provider);

            // create SkinnedMesh for bone visualize
            var animator = m_context.Root.GetComponent<Animator>();
            m_boxMan = SkeletonMeshUtility.CreateRenderer(animator);
            var material = new Material(Shader.Find("Standard"));
            BoxMan.sharedMaterial = material;
            var mesh = BoxMan.sharedMesh;
            mesh.name = "box-man";
        }

        public static BvhMotion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new BvhMotion(context);
        }
        public static BvhMotion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        public void ShowBoxMan(bool enable)
        {
            m_boxMan.enabled = enable;
        }

        public void Dispose()
        {
            GameObject.Destroy(m_context.Root);
        }
    }

    public class VrmAnimation : IMotion
    {
        UniGLTF.RuntimeGltfInstance m_instance;
        public SkinnedMeshRenderer m_boxMan;
        public SkinnedMeshRenderer BoxMan => m_boxMan;
        (INormalizedPoseProvider, ITPoseProvider) m_controlRig;
        (INormalizedPoseProvider, ITPoseProvider) IMotion.ControlRig => m_controlRig;
        public VrmAnimation(UniGLTF.RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }

            var humanoid = instance.gameObject.AddComponent<Humanoid>();
            humanoid.AssignBonesFromAnimator();

            var provider = new InitRotationPoseProvider(instance.transform, humanoid);
            m_controlRig = (provider, provider);

            // create SkinnedMesh for bone visualize
            var animator = instance.GetComponent<Animator>();
            m_boxMan = SkeletonMeshUtility.CreateRenderer(animator);
            var material = new Material(Shader.Find("Standard"));
            BoxMan.sharedMaterial = material;
            var mesh = BoxMan.sharedMesh;
            mesh.name = "box-man";
        }

        public void ShowBoxMan(bool enable)
        {
            m_boxMan.enabled = enable;
        }

        public void Dispose()
        {
            GameObject.Destroy(m_boxMan.gameObject);
        }

        static int? GetNodeIndex(UniGLTF.Extensions.VRMC_vrm_animation.Humanoid humanoid, HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips: return humanoid.HumanBones.Hips?.Node;
                case HumanBodyBones.LeftUpperLeg: return humanoid.HumanBones.LeftUpperLeg?.Node;
                case HumanBodyBones.RightUpperLeg: return humanoid.HumanBones.RightUpperLeg?.Node;
                case HumanBodyBones.LeftLowerLeg: return humanoid.HumanBones.LeftLowerLeg?.Node;
                case HumanBodyBones.RightLowerLeg: return humanoid.HumanBones.RightLowerLeg?.Node;
                case HumanBodyBones.LeftFoot: return humanoid.HumanBones.LeftFoot?.Node;
                case HumanBodyBones.RightFoot: return humanoid.HumanBones.RightFoot?.Node;
                case HumanBodyBones.Spine: return humanoid.HumanBones.Spine?.Node;
                case HumanBodyBones.Chest: return humanoid.HumanBones.Chest?.Node;
                case HumanBodyBones.Neck: return humanoid.HumanBones.Neck?.Node;
                case HumanBodyBones.Head: return humanoid.HumanBones.Head?.Node;
                case HumanBodyBones.LeftShoulder: return humanoid.HumanBones.LeftShoulder?.Node;
                case HumanBodyBones.RightShoulder: return humanoid.HumanBones.RightShoulder?.Node;
                case HumanBodyBones.LeftUpperArm: return humanoid.HumanBones.LeftUpperArm?.Node;
                case HumanBodyBones.RightUpperArm: return humanoid.HumanBones.RightUpperArm?.Node;
                case HumanBodyBones.LeftLowerArm: return humanoid.HumanBones.LeftLowerArm?.Node;
                case HumanBodyBones.RightLowerArm: return humanoid.HumanBones.RightLowerArm?.Node;
                case HumanBodyBones.LeftHand: return humanoid.HumanBones.LeftHand?.Node;
                case HumanBodyBones.RightHand: return humanoid.HumanBones.RightHand?.Node;
                case HumanBodyBones.LeftToes: return humanoid.HumanBones.LeftToes?.Node;
                case HumanBodyBones.RightToes: return humanoid.HumanBones.RightToes?.Node;
                // case HumanBodyBones.LeftEye: return humanoid.HumanBones.LeftEye?.Node;
                // case HumanBodyBones.RightEye: return humanoid.HumanBones.RightEye?.Node;
                case HumanBodyBones.Jaw: return humanoid.HumanBones.Jaw?.Node;
                case HumanBodyBones.LeftThumbProximal: return humanoid.HumanBones.LeftThumbMetacarpal?.Node; // Metacarpal
                case HumanBodyBones.LeftThumbIntermediate: return humanoid.HumanBones.LeftThumbProximal?.Node; // Proximal
                case HumanBodyBones.LeftThumbDistal: return humanoid.HumanBones.LeftThumbDistal?.Node;
                case HumanBodyBones.LeftIndexProximal: return humanoid.HumanBones.LeftIndexProximal?.Node;
                case HumanBodyBones.LeftIndexIntermediate: return humanoid.HumanBones.LeftIndexIntermediate?.Node;
                case HumanBodyBones.LeftIndexDistal: return humanoid.HumanBones.LeftIndexDistal?.Node;
                case HumanBodyBones.LeftMiddleProximal: return humanoid.HumanBones.LeftMiddleProximal?.Node;
                case HumanBodyBones.LeftMiddleIntermediate: return humanoid.HumanBones.LeftMiddleIntermediate?.Node;
                case HumanBodyBones.LeftMiddleDistal: return humanoid.HumanBones.LeftMiddleDistal?.Node;
                case HumanBodyBones.LeftRingProximal: return humanoid.HumanBones.LeftRingProximal?.Node;
                case HumanBodyBones.LeftRingIntermediate: return humanoid.HumanBones.LeftRingIntermediate?.Node;
                case HumanBodyBones.LeftRingDistal: return humanoid.HumanBones.LeftRingDistal?.Node;
                case HumanBodyBones.LeftLittleProximal: return humanoid.HumanBones.LeftLittleProximal?.Node;
                case HumanBodyBones.LeftLittleIntermediate: return humanoid.HumanBones.LeftLittleIntermediate?.Node;
                case HumanBodyBones.LeftLittleDistal: return humanoid.HumanBones.LeftLittleDistal?.Node;
                case HumanBodyBones.RightThumbProximal: return humanoid.HumanBones.RightThumbMetacarpal?.Node; // Metacarpal
                case HumanBodyBones.RightThumbIntermediate: return humanoid.HumanBones.RightThumbProximal?.Node; // Proximal
                case HumanBodyBones.RightThumbDistal: return humanoid.HumanBones.RightThumbDistal?.Node;
                case HumanBodyBones.RightIndexProximal: return humanoid.HumanBones.RightIndexProximal?.Node;
                case HumanBodyBones.RightIndexIntermediate: return humanoid.HumanBones.RightIndexIntermediate?.Node;
                case HumanBodyBones.RightIndexDistal: return humanoid.HumanBones.RightIndexDistal?.Node;
                case HumanBodyBones.RightMiddleProximal: return humanoid.HumanBones.RightMiddleProximal?.Node;
                case HumanBodyBones.RightMiddleIntermediate: return humanoid.HumanBones.RightMiddleIntermediate?.Node;
                case HumanBodyBones.RightMiddleDistal: return humanoid.HumanBones.RightMiddleDistal?.Node;
                case HumanBodyBones.RightRingProximal: return humanoid.HumanBones.RightRingProximal?.Node;
                case HumanBodyBones.RightRingIntermediate: return humanoid.HumanBones.RightRingIntermediate?.Node;
                case HumanBodyBones.RightRingDistal: return humanoid.HumanBones.RightRingDistal?.Node;
                case HumanBodyBones.RightLittleProximal: return humanoid.HumanBones.RightLittleProximal?.Node;
                case HumanBodyBones.RightLittleIntermediate: return humanoid.HumanBones.RightLittleIntermediate?.Node;
                case HumanBodyBones.RightLittleDistal: return humanoid.HumanBones.RightLittleDistal?.Node;
                case HumanBodyBones.UpperChest: return humanoid.HumanBones.UpperChest?.Node;
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

        public static async Task<VrmAnimation> LoadVrmAnimationFromPathAsync(string path)
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

                return new VrmAnimation(instance);
            }
        }
    }
}
