using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UniJSON;
using UnityEngine;
using UniVRM10;
using VRMShaders;

namespace UniVRM10
{

    public class VrmAnimationImporter : UniGLTF.ImporterContext
    {
        public VrmAnimationImporter(GltfData data,
                IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
                ITextureDeserializer textureDeserializer = null,
                IMaterialDescriptorGenerator materialGenerator = null)
            : base(data, externalObjectMap, textureDeserializer, materialGenerator)
        {
            InvertAxis = Axes.X;
        }

        private static int? GetNodeIndex(UniGLTF.Extensions.VRMC_vrm_animation.Humanoid humanoid, HumanBodyBones bone)
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

        public Dictionary<HumanBodyBones, Transform> GetHumanMap()
        {
            var humanMap = new Dictionary<HumanBodyBones, Transform>();
            if (Data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        var animation = UniGLTF.Extensions.VRMC_vrm_animation.GltfDeserializer.Deserialize(kv.Value);
                        if (animation.Humanoid != null)
                        {
                            foreach (HumanBodyBones bone in UniGLTF.Utils.CachedEnum.GetValues<HumanBodyBones>())
                            {
                                // Debug.Log($"{bone} => {index}");
                                var node = GetNodeIndex(animation.Humanoid, bone);
                                if (node.HasValue)
                                {
                                    humanMap.Add(bone, Nodes[node.Value]);
                                }
                            }
                        }
                    }
                }
            }
            return humanMap;
        }

        public List<(ExpressionKey, Transform)> GetExpressions()
        {
            var expressions = new List<(ExpressionKey, Transform)>();
            if (Data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        var animation = UniGLTF.Extensions.VRMC_vrm_animation.GltfDeserializer.Deserialize(kv.Value);
                        if (animation.Expressions != null)
                        {
                            foreach (ExpressionPreset preset in UniGLTF.Utils.CachedEnum.GetValues<ExpressionPreset>())
                            {
                                var node = GetNodeIndex(animation.Expressions, preset);
                                if (node.HasValue)
                                {
                                    expressions.Add((ExpressionKey.CreateFromPreset(preset), Nodes[node.Value]));
                                }
                            }
                        }
                    }
                }
            }
            return expressions;
        }

        static int? GetNodeIndex(UniGLTF.Extensions.VRMC_vrm_animation.Expressions expressions, ExpressionPreset preset)
        {
            switch (preset)
            {
                case ExpressionPreset.happy: return expressions.Preset?.Happy?.Node;
                case ExpressionPreset.angry: return expressions.Preset?.Angry?.Node;
                case ExpressionPreset.sad: return expressions.Preset?.Sad?.Node;
                case ExpressionPreset.relaxed: return expressions.Preset?.Relaxed?.Node;
                case ExpressionPreset.surprised: return expressions.Preset?.Surprised?.Node;
                case ExpressionPreset.aa: return expressions.Preset?.Aa?.Node;
                case ExpressionPreset.ih: return expressions.Preset?.Ih?.Node;
                case ExpressionPreset.ou: return expressions.Preset?.Ou?.Node;
                case ExpressionPreset.ee: return expressions.Preset?.Ee?.Node;
                case ExpressionPreset.oh: return expressions.Preset?.Oh?.Node;
                case ExpressionPreset.blink: return expressions.Preset?.Blink?.Node;
                case ExpressionPreset.blinkLeft: return expressions.Preset?.BlinkLeft?.Node;
                case ExpressionPreset.blinkRight: return expressions.Preset?.BlinkRight?.Node;
                case ExpressionPreset.lookUp: return expressions.Preset?.LookUp?.Node;
                case ExpressionPreset.lookDown: return expressions.Preset?.LookDown?.Node;
                case ExpressionPreset.lookLeft: return expressions.Preset?.LookLeft?.Node;
                case ExpressionPreset.lookRight: return expressions.Preset?.LookRight?.Node;
                case ExpressionPreset.neutral: return expressions.Preset?.Neutral?.Node;
            }
            return default;
        }

        public override async Task<RuntimeGltfInstance> LoadAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> measureTime = null)
        {
            var instance = await base.LoadAsync(awaitCaller, measureTime);

            // setup humanoid
            var humanMap = GetHumanMap();
            if (humanMap.Count > 0)
            {
                var description = AvatarDescription.Create(humanMap);
                //
                // avatar
                //
                var avatar = description.CreateAvatar(instance.Root.transform);
                avatar.name = "Avatar";
                // AvatarDescription = description;
                var animator = instance.gameObject.AddComponent<Animator>();
                animator.avatar = avatar;
            }

            // VRMA-animation solver
            var animationInstance = instance.gameObject.AddComponent<VrmAnimationInstance>();
            animationInstance.Initialize(GetExpressions());

            return instance;
        }
    }
}
