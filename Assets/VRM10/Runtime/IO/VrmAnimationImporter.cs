using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm_animation;
using UniHumanoid;
using UniJSON;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public class VrmAnimationImporter : UniGLTF.ImporterContext
    {
        VRMC_vrm_animation m_vrma;
        ExpressionInfo[] m_expressions;
        Material m_defaultMaterial;

        public VrmAnimationImporter(GltfData data,
                IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
                ITextureDeserializer textureDeserializer = null,
                IMaterialDescriptorGenerator materialGenerator = null)
            : base(data, externalObjectMap, textureDeserializer, materialGenerator, new ImporterContextSettings(invertAxis: Axes.X))
        {
            m_vrma = GetExtension(Data);
        }

        private static VRMC_vrm_animation GetExtension(GltfData data)
        {
            if (data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        return UniGLTF.Extensions.VRMC_vrm_animation.GltfDeserializer.Deserialize(kv.Value);
                    }
                }
            }
            return null;
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
            if (m_vrma is UniGLTF.Extensions.VRMC_vrm_animation.VRMC_vrm_animation animation && animation.Humanoid != null)
            {
                foreach (HumanBodyBones bone in UniGLTF.Utils.CachedEnum.GetValues<HumanBodyBones>())
                {
                    var node = GetNodeIndex(animation.Humanoid, bone);
                    if (node.HasValue)
                    {
                        humanMap.Add(bone, Nodes[node.Value]);
                    }
                }
            }
            return humanMap;
        }

        class ExpressionInfo
        {
            public ExpressionKey Key;
            public int ChannelIndex;
            public string PropertyName;
            public glTFAnimationChannel Channel;
        }

        ExpressionInfo GetExpression(glTFAnimation animation, ExpressionKey key, string propertyName, Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            if (!expression.Node.HasValue)
            {
                return null;
            }

            for (int i = 0; i < animation.channels.Count; ++i)
            {
                var channel = animation.channels[i];
                if (channel.target.node == expression.Node.Value)
                {
                    return new ExpressionInfo
                    {
                        Key = key,
                        ChannelIndex = i,
                        // 全部小文字
                        PropertyName = propertyName.ToLower(),
                        Channel = channel,
                    };
                }
            }

            return null;
        }

        IEnumerable<ExpressionInfo> IterateExpressions()
        {
            if (m_vrma is UniGLTF.Extensions.VRMC_vrm_animation.VRMC_vrm_animation animation && animation.Expressions != null)
            {
                var gltfAnimation = Data.GLTF.animations[0];
                if (animation.Expressions.Preset is Preset preset)
                {
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.happy), nameof(Vrm10AnimationInstance.preset_happy), preset.Happy) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.angry), nameof(Vrm10AnimationInstance.preset_angry), preset.Angry) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.sad), nameof(Vrm10AnimationInstance.preset_sad), preset.Sad) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.relaxed), nameof(Vrm10AnimationInstance.preset_relaxed), preset.Relaxed) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.surprised), nameof(Vrm10AnimationInstance.preset_surprised), preset.Surprised) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.aa), nameof(Vrm10AnimationInstance.preset_aa), preset.Aa) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.ih), nameof(Vrm10AnimationInstance.preset_ih), preset.Ih) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.ou), nameof(Vrm10AnimationInstance.preset_ou), preset.Ou) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.ee), nameof(Vrm10AnimationInstance.preset_ee), preset.Ee) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.oh), nameof(Vrm10AnimationInstance.preset_oh), preset.Oh) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.blink), nameof(Vrm10AnimationInstance.preset_blink), preset.Blink) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.blinkLeft), nameof(Vrm10AnimationInstance.preset_blinkleft), preset.BlinkLeft) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.blinkRight), nameof(Vrm10AnimationInstance.preset_blinkright), preset.BlinkRight) is ExpressionInfo info) yield return info; }
                    { if (GetExpression(gltfAnimation, ExpressionKey.CreateFromPreset(ExpressionPreset.neutral), nameof(Vrm10AnimationInstance.preset_neutral), preset.Neutral) is ExpressionInfo info) yield return info; }
                }
                if (animation.Expressions.Custom != null)
                {
                    int customIndex = 0;
                    foreach (var (k, v) in animation.Expressions.Custom.OrderBy(kv => kv.Value.Node))
                    {
                        var info = GetExpression(gltfAnimation, ExpressionKey.CreateCustom(k), $"custom_{customIndex:D2}", v);
                        ++customIndex;
                        if (info != null)
                        {
                            yield return info;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// xyz translation カーブから x だけの カーブを生成する
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        static AnimationCurve CreateCurve(
            float[] input,
            float[] output)
        {
            var keyframes = new List<Keyframe>();
            for (var inputIndex = 0; inputIndex < input.Length; ++inputIndex)
            {
                var time = input[inputIndex];
                // translation
                var value = output[inputIndex * 3];
                keyframes.Add(new Keyframe(time, value, 0, 0));
                if (keyframes.Count > 0)
                {
                    AnimationImporterUtil.CalculateTangent(keyframes, keyframes.Count - 1);
                }
            }

            var curve = new AnimationCurve();
            foreach (var keyFrame in keyframes)
            {
                curve.AddKey(keyFrame);
            }
            return curve;
        }

        public override async Task<RuntimeGltfInstance> LoadAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> measureTime = null)
        {
            // Expression は AnimationClip を分ける。
            // glTFData から関連 Animation を取り除いて、取っておく。
            m_expressions = IterateExpressions().ToArray();
            foreach (var channelIndex in m_expressions.Select(x => x.ChannelIndex).OrderByDescending(x => x))
            {
                var nodeIndex = Data.GLTF.animations[0].channels[channelIndex].target.node;
                Data.GLTF.nodes.RemoveAt(nodeIndex);
                // 後ろから順に channel を除去
                Data.GLTF.animations[0].channels.RemoveAt(channelIndex);

                UniGLTFLogger.Log($"remove: {channelIndex}");
            }
            Data.GLTF.scenes[0].nodes = Data.GLTF.scenes[0].nodes.Take(1).ToArray();

            // 可視化メッシュ用マテリアル。base.LoadAsync を呼ぶ前に生成する。
            m_defaultMaterial = await MaterialFactory.GetDefaultMaterialAsync(awaitCaller);

            // Humanoid Animation が Gltf アニメーションとしてロードされる
            var instance = await base.LoadAsync(awaitCaller, measureTime);

            return instance;
        }

        protected override Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            // setup humanoid
            var humanMap = GetHumanMap();
            if (humanMap.Count > 0)
            {
                var description = AvatarDescription.Create(humanMap);
                //
                // avatar
                //
                var avatar = description.CreateAvatar(Root.transform);
                avatar.name = "Avatar";
                // AvatarDescription = description;
                var animator = Root.AddComponent<Animator>();
                animator.avatar = avatar;
            }

            if (m_expressions.Length > 0)
            {
                var animation = Root.GetComponentOrThrow<Animation>();
                var clip = animation.clip;
                // m_expressionClip.name = "__expression__";

                // Expression の float カーブを追加する
                // VrmAnimationInstance の "preset_xx" field に連動する
                var gltfAnimation = Data.GLTF.animations[0];
                foreach (var expression in m_expressions)
                {
                    var channel = expression.Channel;
                    var sampler = gltfAnimation.samplers[channel.sampler];
                    var input = Data.GetArrayFromAccessor<float>(sampler.input);
                    var output = Data.FlatternFloatArrayFromAccessor(sampler.output);
                    var curve = CreateCurve(
                        input.ToArray(),
                        output.ToArray());
                    clip.SetCurve("", typeof(Vrm10AnimationInstance), expression.PropertyName, curve);
                }
            }

            var animationInstance = Root.AddComponent<Vrm10AnimationInstance>();

            animationInstance.Initialize(m_expressions.Select(x => x.Key), m_defaultMaterial);

            return Task.CompletedTask;
        }

        public override void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            var animationInstance = Root.GetComponent<Vrm10AnimationInstance>();
            take(SubAssetKey.Create(animationInstance.BoxMan.sharedMesh), animationInstance.BoxMan.sharedMesh);

            var animator = Root.GetComponent<Animator>();
            take(SubAssetKey.Create(animator.avatar), animator.avatar);

            base.TransferOwnership(take);
        }
    }
}