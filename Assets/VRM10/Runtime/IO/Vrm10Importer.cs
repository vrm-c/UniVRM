using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Extensions.VRMC_springBone_limit;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VrmLib.Model から UnityPrefab を構築する
    /// </summary>
    public class Vrm10Importer : UniGLTF.ImporterContext
    {
        private readonly Vrm10Data m_vrm;
        /// VrmLib.Model の オブジェクトと UnityEngine.Object のマッピングを記録する
        // private readonly ModelMap m_map = new ModelMap();
        private readonly bool m_useControlRig;

        // private VrmLib.Model m_model;
        private IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> m_externalMap;
        private Avatar m_humanoid;
        private VRM10Object m_vrmObject;
        private List<(ExpressionPreset Preset, VRM10Expression Clip)> m_expressions = new List<(ExpressionPreset, VRM10Expression)>();

        private IVrm10SpringBoneRuntime m_springboneRuntime;

        public Vrm10Importer(
            Vrm10Data vrm,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            bool useControlRig = false,
            ImporterContextSettings settings = null,
            IVrm10SpringBoneRuntime springboneRuntime = null,
            bool isAssetImport = false
            )
            : base(vrm.Data, externalObjectMap, textureDeserializer,
                settings: new ImporterContextSettings(false, Axes.X),
                isAssetImport: isAssetImport)
        {
            if (vrm == null)
            {
                throw new ArgumentNullException("vrm");
            }
            m_vrm = vrm;
            m_useControlRig = useControlRig;

            TextureDescriptorGenerator = new Vrm10TextureDescriptorGenerator(Data);
            MaterialDescriptorGenerator = materialGenerator ?? Vrm10MaterialDescriptorGeneratorUtility.GetValidVrm10MaterialDescriptorGenerator();

            m_externalMap = externalObjectMap;
            if (m_externalMap == null)
            {
                m_externalMap = new Dictionary<SubAssetKey, UnityEngine.Object>();
            }

            if (springboneRuntime == null)
            {
                if (isAssetImport)
                {
                    // 何もしない dummy
                    springboneRuntime = new Vrm10NopSpringboneRuntime();
                }
                else
                {
                    if (!Application.isPlaying)
                    {
                        // play中でない。test 対策
                        springboneRuntime = new Vrm10FastSpringboneRuntimeStandalone();
                    }
                    else
                    {
                        springboneRuntime = new Vrm10FastSpringboneRuntime();
                    }
                }
            }
            m_springboneRuntime = springboneRuntime;
        }

        static void AssignHumanoid(List<VrmLib.Node> nodes, UniGLTF.Extensions.VRMC_vrm.HumanBone humanBone, VrmLib.HumanoidBones key)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }
            if (humanBone != null && humanBone.Node.HasValue)
            {
                var index = humanBone.Node.Value;
                if (index >= 0 && index < nodes.Count)
                {
                    nodes[index].HumanoidBone = key;
                }
                else
                {
                    throw new IndexOutOfRangeException("AssignHumanoid");
                }
            }
        }

        static IEnumerable<(HumanBodyBones, Transform)> EnumerateHumanbones(List<Transform> nodes, UniGLTF.Extensions.VRMC_vrm.HumanBones bones)
        {
            { if (bones.Hips != null && bones.Hips.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Hips, nodes[index]); }
            { if (bones.LeftUpperLeg != null && bones.LeftUpperLeg.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftUpperLeg, nodes[index]); }
            { if (bones.RightUpperLeg != null && bones.RightUpperLeg.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightUpperLeg, nodes[index]); }
            { if (bones.LeftLowerLeg != null && bones.LeftLowerLeg.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftLowerLeg, nodes[index]); }
            { if (bones.RightLowerLeg != null && bones.RightLowerLeg.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightLowerLeg, nodes[index]); }
            { if (bones.LeftFoot != null && bones.LeftFoot.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftFoot, nodes[index]); }
            { if (bones.RightFoot != null && bones.RightFoot.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightFoot, nodes[index]); }
            { if (bones.Spine != null && bones.Spine.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Spine, nodes[index]); }
            { if (bones.Chest != null && bones.Chest.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Chest, nodes[index]); }
            { if (bones.Neck != null && bones.Neck.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Neck, nodes[index]); }
            { if (bones.Head != null && bones.Head.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Head, nodes[index]); }
            { if (bones.LeftShoulder != null && bones.LeftShoulder.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftShoulder, nodes[index]); }
            { if (bones.RightShoulder != null && bones.RightShoulder.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightShoulder, nodes[index]); }
            { if (bones.LeftUpperArm != null && bones.LeftUpperArm.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftUpperArm, nodes[index]); }
            { if (bones.RightUpperArm != null && bones.RightUpperArm.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightUpperArm, nodes[index]); }
            { if (bones.LeftLowerArm != null && bones.LeftLowerArm.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftLowerArm, nodes[index]); }
            { if (bones.RightLowerArm != null && bones.RightLowerArm.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightLowerArm, nodes[index]); }
            { if (bones.LeftHand != null && bones.LeftHand.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftHand, nodes[index]); }
            { if (bones.RightHand != null && bones.RightHand.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightHand, nodes[index]); }
            { if (bones.LeftToes != null && bones.LeftToes.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftToes, nodes[index]); }
            { if (bones.RightToes != null && bones.RightToes.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightToes, nodes[index]); }
            { if (bones.LeftEye != null && bones.LeftEye.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftEye, nodes[index]); }
            { if (bones.RightEye != null && bones.RightEye.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightEye, nodes[index]); }
            { if (bones.Jaw != null && bones.Jaw.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.Jaw, nodes[index]); }
            { if (bones.LeftThumbMetacarpal != null && bones.LeftThumbMetacarpal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftThumbProximal, nodes[index]); }
            { if (bones.LeftThumbProximal != null && bones.LeftThumbProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftThumbIntermediate, nodes[index]); }
            { if (bones.LeftThumbDistal != null && bones.LeftThumbDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftThumbDistal, nodes[index]); }
            { if (bones.LeftIndexProximal != null && bones.LeftIndexProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftIndexProximal, nodes[index]); }
            { if (bones.LeftIndexIntermediate != null && bones.LeftIndexIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftIndexIntermediate, nodes[index]); }
            { if (bones.LeftIndexDistal != null && bones.LeftIndexDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftIndexDistal, nodes[index]); }
            { if (bones.LeftMiddleProximal != null && bones.LeftMiddleProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftMiddleProximal, nodes[index]); }
            { if (bones.LeftMiddleIntermediate != null && bones.LeftMiddleIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftMiddleIntermediate, nodes[index]); }
            { if (bones.LeftMiddleDistal != null && bones.LeftMiddleDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftMiddleDistal, nodes[index]); }
            { if (bones.LeftRingProximal != null && bones.LeftRingProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftRingProximal, nodes[index]); }
            { if (bones.LeftRingIntermediate != null && bones.LeftRingIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftRingIntermediate, nodes[index]); }
            { if (bones.LeftRingDistal != null && bones.LeftRingDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftRingDistal, nodes[index]); }
            { if (bones.LeftLittleProximal != null && bones.LeftLittleProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftLittleProximal, nodes[index]); }
            { if (bones.LeftLittleIntermediate != null && bones.LeftLittleIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftLittleIntermediate, nodes[index]); }
            { if (bones.LeftLittleDistal != null && bones.LeftLittleDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.LeftLittleDistal, nodes[index]); }
            { if (bones.RightThumbMetacarpal != null && bones.RightThumbMetacarpal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightThumbProximal, nodes[index]); }
            { if (bones.RightThumbProximal != null && bones.RightThumbProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightThumbIntermediate, nodes[index]); }
            { if (bones.RightThumbDistal != null && bones.RightThumbDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightThumbDistal, nodes[index]); }
            { if (bones.RightIndexProximal != null && bones.RightIndexProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightIndexProximal, nodes[index]); }
            { if (bones.RightIndexIntermediate != null && bones.RightIndexIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightIndexIntermediate, nodes[index]); }
            { if (bones.RightIndexDistal != null && bones.RightIndexDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightIndexDistal, nodes[index]); }
            { if (bones.RightMiddleProximal != null && bones.RightMiddleProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightMiddleProximal, nodes[index]); }
            { if (bones.RightMiddleIntermediate != null && bones.RightMiddleIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightMiddleIntermediate, nodes[index]); }
            { if (bones.RightMiddleDistal != null && bones.RightMiddleDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightMiddleDistal, nodes[index]); }
            { if (bones.RightRingProximal != null && bones.RightRingProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightRingProximal, nodes[index]); }
            { if (bones.RightRingIntermediate != null && bones.RightRingIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightRingIntermediate, nodes[index]); }
            { if (bones.RightRingDistal != null && bones.RightRingDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightRingDistal, nodes[index]); }
            { if (bones.RightLittleProximal != null && bones.RightLittleProximal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightLittleProximal, nodes[index]); }
            { if (bones.RightLittleIntermediate != null && bones.RightLittleIntermediate.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightLittleIntermediate, nodes[index]); }
            { if (bones.RightLittleDistal != null && bones.RightLittleDistal.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.RightLittleDistal, nodes[index]); }
            { if (bones.UpperChest != null && bones.UpperChest.Node.TryGetValidIndex(nodes.Count, out var index)) yield return (HumanBodyBones.UpperChest, nodes[index]); }
        }

        /// <summary>
        /// RuntimeGltfInstance 移譲するリソースの作成をする初期化
        /// </summary>
        protected override async Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            Root.name = "VRM1";

            // humanoid
            var humanoid = Root.AddComponent<UniHumanoid.Humanoid>();
            humanoid.AssignBones(EnumerateHumanbones(Nodes, m_vrm.VrmExtension.Humanoid.HumanBones));
            m_humanoid = humanoid.CreateAvatar();
            m_humanoid.name = "humanoid";
            var animator = Root.AddComponent<Animator>();
            animator.avatar = m_humanoid;

            // VrmController
            var controller = Root.AddComponent<Vrm10Instance>();
            controller.InitializeAtRuntime(m_useControlRig, m_springboneRuntime);
            controller.enabled = false;

            // vrm
            controller.Vrm = await LoadVrmAsync(awaitCaller, m_vrm.VrmExtension);
        }

        /// <summary>
        /// RuntimeGltfInstance アタッチよりあとの初期化
        /// 
        /// RuntimeGltfInstance.InitialTransformStates にアクセスするなど
        /// </summary>
        protected override async Task FinalizeAsync(IAwaitCaller awaitCaller)
        {
            var controller = Root.GetComponent<Vrm10Instance>();

            // springBone
            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Data.GLTF.extensions, out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                await LoadSpringBoneAsync(awaitCaller, controller, springBone);
            }

            if (IsAssetImport)
            {
                // ScriptedImpoter から発動された。
                // SpringBone のリソース確保を回避する。
                // Application.isPlaying == true がありえる。
            }
            else
            {
                // ScriptedImpoter 経由でない。
                // Vrm10Runtime で初期化していたが、 async にするためこちらに移動 v0.127
                // RuntimeGltfInstance にアクセスしたいのだが OnLoadHierarchy ではまだ attach されてなかった v0.128
                // VRMC_springBone が無くても初期化する v0.127.2
                await m_springboneRuntime.InitializeAsync(controller, awaitCaller);
            }

            // constraint
            await LoadConstraintAsync(awaitCaller, controller);

            // Hierarchyの構築が終わるまで遅延させる
            // TODO: springbone の startup 問題なら、springbone はデフォルト pause 状態でいいかもしれない
            controller.enabled = true;
        }

        VRM10Expression GetOrLoadExpression(in SubAssetKey key, ExpressionPreset preset, UniGLTF.Extensions.VRMC_vrm.Expression expression)
        {
            VRM10Expression clip = default;
            if (m_externalMap.TryGetValue(key, out UnityEngine.Object expressionObj))
            {
                clip = expressionObj as VRM10Expression;
            }
            else
            {
                if (expression == null)
                {
                    // default empty expression
                    expression = new UniGLTF.Extensions.VRMC_vrm.Expression
                    {
                        IsBinary = false,
                        OverrideBlink = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none,
                        OverrideLookAt = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none,
                        OverrideMouth = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none,
                    };
                }
                clip = ScriptableObject.CreateInstance<UniVRM10.VRM10Expression>();
                clip.name = key.Name;
                clip.IsBinary = expression.IsBinary.GetValueOrDefault();
                clip.OverrideBlink = expression.OverrideBlink;
                clip.OverrideLookAt = expression.OverrideLookAt;
                clip.OverrideMouth = expression.OverrideMouth;

                if (expression.MorphTargetBinds != null)
                {
                    clip.MorphTargetBindings = expression.MorphTargetBinds?
                        .Select(x => x.Build10(Root, this))
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToArray();
                }
                else
                {
                    clip.MorphTargetBindings = new MorphTargetBinding[] { };
                }

                if (expression.MaterialColorBinds != null)
                {
                    clip.MaterialColorBindings = expression.MaterialColorBinds.Select(x => x.Build10(MaterialFactory.Materials))
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToArray();
                }
                else
                {
                    clip.MaterialColorBindings = new MaterialColorBinding[] { };
                }

                if (expression.TextureTransformBinds != null)
                {
                    clip.MaterialUVBindings = expression?.TextureTransformBinds?.Select(x => x.Build10(MaterialFactory.Materials))
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToArray();
                }
                else
                {
                    clip.MaterialUVBindings = new MaterialUVBinding[] { };
                }

                m_expressions.Add((preset, clip));
            }
            return clip;
        }

        public async Task<Texture2D> LoadVrmThumbnailAsync(IAwaitCaller awaitCaller = null)
        {
            if (awaitCaller == null)
            {
                awaitCaller = new ImmediateCaller();
            }

            if (Vrm10TextureDescriptorGenerator.TryGetMetaThumbnailTextureImportParam(Data, m_vrm.VrmExtension, out (SubAssetKey, TextureDescriptor Param) kv))
            {
                var texture = await TextureFactory.GetTextureAsync(kv.Param, awaitCaller);
                return texture as Texture2D;
            }
            else
            {
                return null;
            }
        }

        async Task<VRM10Object> LoadVrmAsync(IAwaitCaller awaitCaller, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrmExtension)
        {
            if (m_externalMap.TryGetValue(VRM10Object.SubAssetKey, out UnityEngine.Object obj) && obj is VRM10Object vrm)
            {
                // use external object map
                return vrm;
            }

            // create new object
            m_vrmObject = vrm = ScriptableObject.CreateInstance<VRM10Object>();
            vrm.name = VRM10Object.SubAssetKey.Name;

            // meta
            if (vrmExtension.Meta != null)
            {
                var src = vrmExtension.Meta;
                var meta = new VRM10ObjectMeta();
                vrm.Meta = meta;
                meta.Name = src.Name;
                meta.Version = src.Version;
                meta.CopyrightInformation = src.CopyrightInformation;
                meta.ContactInformation = src.ContactInformation;
                meta.ThirdPartyLicenses = src.ThirdPartyLicenses;
                // avatar
                meta.AvatarPermission = src.AvatarPermission;
                meta.ViolentUsage = src.AllowExcessivelyViolentUsage.GetValueOrDefault();
                meta.SexualUsage = src.AllowExcessivelySexualUsage.GetValueOrDefault();
                meta.CommercialUsage = src.CommercialUsage;
                meta.PoliticalOrReligiousUsage = src.AllowPoliticalOrReligiousUsage.GetValueOrDefault();
                meta.AntisocialOrHateUsage = src.AllowAntisocialOrHateUsage.GetValueOrDefault();
                // redistribution
                meta.CreditNotation = src.CreditNotation;
                meta.Redistribution = src.AllowRedistribution.GetValueOrDefault();

                meta.Modification = src.Modification;
                meta.OtherLicenseUrl = src.OtherLicenseUrl;
                //
                if (src.References != null)
                {
                    meta.References.AddRange(src.References);
                }
                if (src.Authors != null)
                {
                    meta.Authors.AddRange(src.Authors);
                }

                var tex2D = await LoadVrmThumbnailAsync(awaitCaller);
                if (tex2D != null)
                {
                    meta.Thumbnail = tex2D;
                }
            }

            // expression
            {
                vrm.Expression.Happy = GetOrLoadExpression(ExpressionKey.Happy.SubAssetKey, ExpressionPreset.happy, vrmExtension.Expressions?.Preset?.Happy);
                vrm.Expression.Angry = GetOrLoadExpression(ExpressionKey.Angry.SubAssetKey, ExpressionPreset.angry, vrmExtension.Expressions?.Preset?.Angry);
                vrm.Expression.Sad = GetOrLoadExpression(ExpressionKey.Sad.SubAssetKey, ExpressionPreset.sad, vrmExtension.Expressions?.Preset?.Sad);
                vrm.Expression.Relaxed = GetOrLoadExpression(ExpressionKey.Relaxed.SubAssetKey, ExpressionPreset.relaxed, vrmExtension.Expressions?.Preset?.Relaxed);
                vrm.Expression.Surprised = GetOrLoadExpression(ExpressionKey.Surprised.SubAssetKey, ExpressionPreset.surprised, vrmExtension.Expressions?.Preset?.Surprised);
                vrm.Expression.Aa = GetOrLoadExpression(ExpressionKey.Aa.SubAssetKey, ExpressionPreset.aa, vrmExtension.Expressions?.Preset?.Aa);
                vrm.Expression.Ih = GetOrLoadExpression(ExpressionKey.Ih.SubAssetKey, ExpressionPreset.ih, vrmExtension.Expressions?.Preset?.Ih);
                vrm.Expression.Ou = GetOrLoadExpression(ExpressionKey.Ou.SubAssetKey, ExpressionPreset.ou, vrmExtension.Expressions?.Preset?.Ou);
                vrm.Expression.Ee = GetOrLoadExpression(ExpressionKey.Ee.SubAssetKey, ExpressionPreset.ee, vrmExtension.Expressions?.Preset?.Ee);
                vrm.Expression.Oh = GetOrLoadExpression(ExpressionKey.Oh.SubAssetKey, ExpressionPreset.oh, vrmExtension.Expressions?.Preset?.Oh);
                vrm.Expression.Blink = GetOrLoadExpression(ExpressionKey.Blink.SubAssetKey, ExpressionPreset.blink, vrmExtension.Expressions?.Preset?.Blink);
                vrm.Expression.BlinkLeft = GetOrLoadExpression(ExpressionKey.BlinkLeft.SubAssetKey, ExpressionPreset.blinkLeft, vrmExtension.Expressions?.Preset?.BlinkLeft);
                vrm.Expression.BlinkRight = GetOrLoadExpression(ExpressionKey.BlinkRight.SubAssetKey, ExpressionPreset.blinkRight, vrmExtension.Expressions?.Preset?.BlinkRight);
                vrm.Expression.LookUp = GetOrLoadExpression(ExpressionKey.LookUp.SubAssetKey, ExpressionPreset.lookUp, vrmExtension.Expressions?.Preset?.LookUp);
                vrm.Expression.LookDown = GetOrLoadExpression(ExpressionKey.LookDown.SubAssetKey, ExpressionPreset.lookDown, vrmExtension.Expressions?.Preset?.LookDown);
                vrm.Expression.LookLeft = GetOrLoadExpression(ExpressionKey.LookLeft.SubAssetKey, ExpressionPreset.lookLeft, vrmExtension.Expressions?.Preset?.LookLeft);
                vrm.Expression.LookRight = GetOrLoadExpression(ExpressionKey.LookRight.SubAssetKey, ExpressionPreset.lookRight, vrmExtension.Expressions?.Preset?.LookRight);
                vrm.Expression.Neutral = GetOrLoadExpression(ExpressionKey.Neutral.SubAssetKey, ExpressionPreset.neutral, vrmExtension.Expressions?.Preset?.Neutral);
                if (vrmExtension?.Expressions?.Custom != null)
                {
                    foreach (var (name, expression) in vrmExtension.Expressions.Custom.Select(kv => (kv.Key, kv.Value)))
                    {
                        var key = ExpressionKey.CreateCustom(name);
                        var preset = ExpressionPreset.custom;
                        var clip = GetOrLoadExpression(key.SubAssetKey, preset, expression);
                        if (clip != null)
                        {
                            vrm.Expression.AddClip(preset, clip);
                        }
                    }
                }
            }

            // lookat
            if (vrmExtension.LookAt != null)
            {
                var src = vrmExtension.LookAt;
                vrm.LookAt.LookAtType = src.Type;
                if (src.OffsetFromHeadBone != null)
                {
                    vrm.LookAt.OffsetFromHead = new Vector3(src.OffsetFromHeadBone[0], src.OffsetFromHeadBone[1], src.OffsetFromHeadBone[2]).ReverseX();
                }
                if (src.RangeMapHorizontalInner != null)
                {
                    vrm.LookAt.HorizontalInner = new CurveMapper(src.RangeMapHorizontalInner.InputMaxValue.Value, src.RangeMapHorizontalInner.OutputScale.Value);
                }
                if (src.RangeMapHorizontalOuter != null)
                {
                    vrm.LookAt.HorizontalOuter = new CurveMapper(src.RangeMapHorizontalOuter.InputMaxValue.Value, src.RangeMapHorizontalOuter.OutputScale.Value);
                }
                if (src.RangeMapVerticalUp != null)
                {
                    vrm.LookAt.VerticalUp = new CurveMapper(src.RangeMapVerticalUp.InputMaxValue.Value, src.RangeMapVerticalUp.OutputScale.Value);
                }
                if (src.RangeMapVerticalDown != null)
                {
                    vrm.LookAt.VerticalDown = new CurveMapper(src.RangeMapVerticalDown.InputMaxValue.Value, src.RangeMapVerticalDown.OutputScale.Value);
                }
            }

            // firstPerson
            if (vrmExtension.FirstPerson != null && vrmExtension.FirstPerson.MeshAnnotations != null)
            {
                var fp = vrmExtension.FirstPerson;
                foreach (var x in fp.MeshAnnotations)
                {
                    if (x.Node.TryGetValidIndex(Nodes.Count, out var index))
                    {
                        var node = Nodes[x.Node.Value];
                        var relative = node.RelativePathFrom(Root.transform);
                        vrm.FirstPerson.Renderers.Add(new RendererFirstPersonFlags
                        {
                            FirstPersonFlag = x.Type,
                            Renderer = relative,
                        });
                    }
                }
            }
            else
            {
                // default 値を割り当てる
                foreach (var smr in Root.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var relative = smr.transform.RelativePathFrom(Root.transform);
                    vrm.FirstPerson.Renderers.Add(new RendererFirstPersonFlags
                    {
                        FirstPersonFlag = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto,
                        Renderer = relative,
                    });
                }
            }
            // 設定の無い renderer に auto を割り当てる
            foreach (var smr in Root.GetComponentsInChildren<Renderer>())
            {
                var relative = smr.transform.RelativePathFrom(Root.transform);
                if (!vrm.FirstPerson.Renderers.Any(x => x.Renderer == relative))
                {
                    vrm.FirstPerson.Renderers.Add(new RendererFirstPersonFlags
                    {
                        FirstPersonFlag = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto,
                        Renderer = relative,
                    });
                }
            }

            return vrm;
        }

        async Task LoadSpringBoneAsync(IAwaitCaller awaitCaller, Vrm10Instance controller, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone)
        {
            await awaitCaller.NextFrame();

            // colliders
            var colliders = new List<VRM10SpringBoneCollider>();
            if (gltfVrmSpringBone.Colliders != null)
            {
                foreach (var c in gltfVrmSpringBone.Colliders)
                {
                    var collider = Nodes[c.Node.Value].gameObject.AddComponent<VRM10SpringBoneCollider>();
                    colliders.Add(collider);
                    if (c.Shape.Capsule is UniGLTF.Extensions.VRMC_springBone.ColliderShapeCapsule capsule)
                    {
                        collider.ColliderType = VRM10SpringBoneColliderTypes.Capsule;
                        collider.Offset = Vector3InvertX(capsule.Offset);
                        collider.Tail = Vector3InvertX(capsule.Tail);
                        collider.Radius = capsule.Radius.Value;
                    }
                    else if (c.Shape.Sphere is UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere sphere)
                    {
                        collider.ColliderType = VRM10SpringBoneColliderTypes.Sphere;
                        collider.Offset = Vector3InvertX(sphere.Offset);
                        collider.Radius = sphere.Radius.Value;
                    }
                    else
                    {
                        throw new Vrm10Exception("unknown shape");
                    }

                    // https://github.com/vrm-c/vrm-specification/tree/master/specification/VRMC_springBone_extended_collider-1.0
                    // VRMC_springBone_extended_collider
                    if (UniGLTF.Extensions.VRMC_springBone_extended_collider.GltfDeserializer.TryGet(c.Extensions as glTFExtension,
                        out UniGLTF.Extensions.VRMC_springBone_extended_collider.VRMC_springBone_extended_collider exCollider))
                    {
                        if (exCollider.Shape.Sphere is UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeSphere exSphere)
                        {
                            collider.ColliderType = exSphere.Inside.GetValueOrDefault() ? VRM10SpringBoneColliderTypes.SphereInside : VRM10SpringBoneColliderTypes.Sphere;
                            collider.Offset = Vector3InvertX(exSphere.Offset);
                            collider.Radius = exSphere.Radius.Value;
                            if (exSphere.Inside.HasValue && exSphere.Inside.Value)
                            {
                                collider.ColliderType = VRM10SpringBoneColliderTypes.SphereInside;
                            }
                        }
                        else if (exCollider.Shape.Capsule is UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeCapsule exCapsule)
                        {
                            collider.ColliderType = exCapsule.Inside.GetValueOrDefault() ? VRM10SpringBoneColliderTypes.CapsuleInside : VRM10SpringBoneColliderTypes.Capsule;
                            collider.Offset = Vector3InvertX(exCapsule.Offset);
                            collider.Tail = Vector3InvertX(exCapsule.Tail);
                            collider.Radius = exCapsule.Radius.Value;
                        }
                        else if (exCollider.Shape.Plane is UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapePlane exPlane)
                        {
                            collider.ColliderType = VRM10SpringBoneColliderTypes.Plane;
                            collider.Offset = Vector3InvertX(exPlane.Offset);
                            collider.Normal = Vector3InvertX(exPlane.Normal);
                            collider.Radius = 0.1f; // gizmo visualize. 10cm
                        }
                        else
                        {
                            throw new Vrm10Exception("VRMC_springBone_extended_collider: unknown shape");
                        }
                    }
                }
            }

            // colliderGroup
            if (gltfVrmSpringBone.ColliderGroups != null)
            {
                var secondary = Root.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = new GameObject("secondary").transform;
                    secondary.SetParent(Root.transform, false);
                }

                foreach (var g in gltfVrmSpringBone.ColliderGroups)
                {
                    var colliderGroup = secondary.gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                    colliderGroup.Name = g.Name;
                    controller.SpringBone.ColliderGroups.Add(colliderGroup);

                    if (g != null && g.Colliders != null)
                    {
                        foreach (var c in g.Colliders)
                        {
                            if (c < 0 || c >= colliders.Count)
                            {
                                // 不正なindexの場合は無視する
                                continue;
                            }

                            var collider = colliders[c];
                            colliderGroup.Colliders.Add(collider);
                        }
                    }
                }
            }

            // springs
            if (gltfVrmSpringBone.Springs != null)
            {
                // spring
                foreach (var gltfSpring in gltfVrmSpringBone.Springs)
                {
                    if (gltfSpring.Joints == null || gltfSpring.Joints.Count == 0)
                    {
                        continue;
                    }
                    var spring = new Vrm10InstanceSpringBone.Spring(gltfSpring.Name);
                    controller.SpringBone.Springs.Add(spring);

                    if (gltfSpring.Center.HasValue)
                    {
                        spring.Center = Nodes[gltfSpring.Center.Value];
                    }

                    if (gltfSpring.ColliderGroups != null)
                    {
                        spring.ColliderGroups = gltfSpring.ColliderGroups
                            // VRM1_Constraint_Twist_Sample_Plane.vrm
                            .Where(x => x >= 0 && x < controller.SpringBone.ColliderGroups.Count)
                            .Select(x => controller.SpringBone.ColliderGroups[x]).ToList();
                    }
                    // joint
                    foreach (var gltfJoint in gltfSpring.Joints)
                    {
                        if (gltfJoint.Node.HasValue)
                        {
                            var index = gltfJoint.Node.Value;
                            if (index < 0 || index >= Nodes.Count)
                            {
                                throw new IndexOutOfRangeException($"{index} > {Nodes.Count}");
                            }
                            // https://github.com/vrm-c/UniVRM/issues/1441
                            //
                            // https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_springBone-1.0-beta/schema/VRMC_springBone.joint.schema.json
                            // に基づきデフォルト値を補う

                            // node is required
                            var go = Nodes[gltfJoint.Node.Value].gameObject;
                            if (go.TryGetComponent<VRM10SpringBoneJoint>(out var joint))
                            {
                                // 仕様違反。マイグレーションで発生しうるのと、エクスポーターでの除外などがされていないので、
                                // エラーにせずに飛ばす
                                UniGLTFLogger.Warning($"duplicated spring joint: {Data.TargetPath}");
                                continue;
                            }

                            joint = go.AddComponent<VRM10SpringBoneJoint>();
                            joint.m_jointRadius = gltfJoint.HitRadius.GetValueOrDefault(0.0f);
                            joint.m_dragForce = gltfJoint.DragForce.GetValueOrDefault(0.5f);
                            joint.m_gravityDir = gltfJoint.GravityDir != null ? Vector3InvertX(gltfJoint.GravityDir) : Vector3.down;
                            joint.m_gravityPower = gltfJoint.GravityPower.GetValueOrDefault(0.0f);
                            joint.m_stiffnessForce = gltfJoint.Stiffness.GetValueOrDefault(1.0f);

                            if (UniGLTF.Extensions.VRMC_springBone_limit.GltfDeserializer.TryGet(gltfJoint.Extensions as glTFExtension,
                                out var extensionSpringBoneLimit))
                            {
                                if (extensionSpringBoneLimit.Limit.Cone is UniGLTF.Extensions.VRMC_springBone_limit.ConeLimit cone)
                                {
                                    joint.m_anglelimitType = UniGLTF.SpringBoneJobs.AnglelimitTypes.Cone;
                                    joint.m_limitSpaceOffset = QuaternionFromFloat4(cone.Rotation);
                                    joint.m_pitch = cone.Angle.GetValueOrDefault();
                                }
                                else if (extensionSpringBoneLimit.Limit.Hinge is UniGLTF.Extensions.VRMC_springBone_limit.HingeLimit hinge)
                                {
                                    joint.m_anglelimitType = UniGLTF.SpringBoneJobs.AnglelimitTypes.Hinge;
                                    joint.m_limitSpaceOffset = QuaternionFromFloat4(hinge.Rotation);
                                    joint.m_pitch = hinge.Angle.GetValueOrDefault();
                                }
                                else if (extensionSpringBoneLimit.Limit.Spherical is UniGLTF.Extensions.VRMC_springBone_limit.SphericalLimit spherical)
                                {
                                    joint.m_anglelimitType = UniGLTF.SpringBoneJobs.AnglelimitTypes.Spherical;
                                    joint.m_limitSpaceOffset = QuaternionFromFloat4(spherical.Rotation);
                                    joint.m_pitch = spherical.Pitch.GetValueOrDefault();
                                    joint.m_yaw = spherical.Yaw.GetValueOrDefault();
                                }
                            }

                            spring.Joints.Add(joint);
                        }
                    }
                }
            }
        }

        private static Quaternion QuaternionFromFloat4(float[] xyzw)
        {
            var q = (xyzw != null && xyzw.Length == 4)
                ? new Quaternion(xyzw[0], xyzw[1], xyzw[2], xyzw[3])
                : Quaternion.identity
                ;
            // vrm-1.0 is x inverted            
            return Axes.X.Create().InvertQuaternion(q);
        }

        static AxisMask ConstraintAxes(bool[] flags)
        {
            var mask = default(AxisMask);
            if (flags != null && flags.Length == 3)
            {
                if (flags[0]) mask |= AxisMask.X;
                if (flags[1]) mask |= AxisMask.Y;
                if (flags[2]) mask |= AxisMask.Z;
            }
            return mask;
        }

        static Vector3 Vector3InvertX(float[] f)
        {
            var v = default(Vector3);
            if (f != null && f.Length == 3)
            {
                v.x = -f[0];
                v.y = f[1];
                v.z = f[2];
            }
            return v;
        }

        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/tree/master/specification/VRMC_node_constraint-1.0_beta
        /// 
        /// * roll
        /// * aim
        /// * rotaton
        /// 
        /// </summary>
        /// <param name="awaitCaller"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        async Task LoadConstraintAsync(IAwaitCaller awaitCaller, Vrm10Instance controller)
        {
            for (int i = 0; i < Data.GLTF.nodes.Count; ++i)
            {
                var gltfNode = Data.GLTF.nodes[i];
                if (UniGLTF.Extensions.VRMC_node_constraint.GltfDeserializer.TryGet(gltfNode.extensions, out UniGLTF.Extensions.VRMC_node_constraint.VRMC_node_constraint ext))
                {
                    var constraint = ext.Constraint;
                    var node = Nodes[i];
                    if (constraint.Roll != null)
                    {
                        var roll = constraint.Roll;
                        var component = node.gameObject.AddComponent<Vrm10RollConstraint>();
                        component.Source = Nodes[roll.Source.Value]; // required
                        component.Weight = roll.Weight.GetValueOrDefault(1.0f);
                        component.RollAxis = roll.RollAxis; // required
                    }
                    else if (constraint.Aim != null)
                    {
                        var aim = constraint.Aim;
                        var component = node.gameObject.AddComponent<Vrm10AimConstraint>();
                        component.Source = Nodes[aim.Source.Value]; // required
                        component.Weight = aim.Weight.GetValueOrDefault(1.0f);
                        component.AimAxis = Vrm10ConstraintUtil.ReverseX(aim.AimAxis); // required
                    }
                    else if (constraint.Rotation != null)
                    {
                        var rotation = constraint.Rotation;
                        var component = node.gameObject.AddComponent<Vrm10RotationConstraint>();
                        component.Source = Nodes[rotation.Source.Value]; // required
                        component.Weight = rotation.Weight.GetValueOrDefault(1.0f);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            await awaitCaller.NextFrame();
        }

        public static HumanBodyBones ToUnity(VrmLib.HumanoidBones bone)
        {
            switch (bone)
            {
                // https://github.com/vrm-c/vrm-specification/issues/380
                case VrmLib.HumanoidBones.unknown: return HumanBodyBones.LastBone;
                case VrmLib.HumanoidBones.leftThumbMetacarpal: return HumanBodyBones.LeftThumbProximal;
                case VrmLib.HumanoidBones.leftThumbProximal: return HumanBodyBones.LeftThumbIntermediate;
                case VrmLib.HumanoidBones.rightThumbMetacarpal: return HumanBodyBones.RightThumbProximal;
                case VrmLib.HumanoidBones.rightThumbProximal: return HumanBodyBones.RightThumbIntermediate;
            }
            return CachedEnum.Parse<HumanBodyBones>(bone.ToString(), ignoreCase: true);
        }

        /// <summary>
        /// ヒエラルキーを再帰的に構築する
        /// <summary>
        public static void CreateNodes(VrmLib.Node node, GameObject parent, Dictionary<VrmLib.Node, GameObject> nodes)
        {
            GameObject go = new GameObject(node.Name);
            nodes.Add(node, go);

            // world
            go.transform.SetPositionAndRotation(node.Translation, node.Rotation);
            if (parent != null)
            {
                go.transform.SetParent(parent.transform, true);
            }
            // local
            go.transform.localScale = node.LocalScaling;

            if (node.Children.Count > 0)
            {
                for (int n = 0; n < node.Children.Count; n++)
                {
                    CreateNodes(node.Children[n], go, nodes);
                }
            }
        }

        /// <summary>
        /// MeshFilter + MeshRenderer もしくは SkinnedMeshRenderer を構築する
        /// </summary>
        public static async Task<Renderer> CreateRendererAsync(VrmLib.Node node, GameObject go, ModelMap map, MaterialFactory materialFactory, IAwaitCaller awaitCaller)
        {
            Renderer renderer = null;
            var hasBlendShape = node.MeshGroup.Meshes[0].MorphTargets.Any();
            if (node.MeshGroup.Skin != null || hasBlendShape)
            {
                var skinnedMeshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                renderer = skinnedMeshRenderer;
                skinnedMeshRenderer.sharedMesh = map.Meshes[node.MeshGroup];
                if (node.MeshGroup.Skin != null)
                {
                    skinnedMeshRenderer.bones = node.MeshGroup.Skin.Joints.Select(x => map.Nodes[x].transform).ToArray();
                    if (node.MeshGroup.Skin.Root != null)
                    {
                        skinnedMeshRenderer.rootBone = map.Nodes[node.MeshGroup.Skin.Root].transform;
                    }
                }
            }
            else
            {
                var meshFilter = go.AddComponent<MeshFilter>();
                renderer = go.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = map.Meshes[node.MeshGroup];
            }

            // hide by default
            renderer.enabled = false;

            if (node.MeshGroup.Meshes.Count == 0)
            {
                throw new NotImplementedException();
            }
            else if (node.MeshGroup.Meshes.Count == 1)
            {
                var materialCount = node.MeshGroup.Meshes[0].Submeshes.Count;
                var materials = new Material[materialCount];
                for (var idx = 0; idx < materialCount; ++idx)
                {
                    var materialIndex = node.MeshGroup.Meshes[0].Submeshes[idx].Material;
                    if (materialIndex.HasValidIndex())
                    {
                        materials[idx] = materialFactory.Materials[materialIndex.Value].Asset;
                    }
                    else
                    {
                        materials[idx] = await materialFactory.GetDefaultMaterialAsync(awaitCaller);
                    }
                }
                renderer.sharedMaterials = materials;
            }
            else
            {
                var materialCount = node.MeshGroup.Meshes.Count;
                var materials = new Material[materialCount];
                for (var idx = 0; idx < materialCount; ++idx)
                {
                    var materialIndex = node.MeshGroup.Meshes[idx].Submeshes[0].Material;
                    if (materialIndex.HasValidIndex())
                    {
                        materials[idx] = materialFactory.Materials[materialIndex.Value].Asset;
                    }
                    else
                    {
                        materials[idx] = await materialFactory.GetDefaultMaterialAsync(awaitCaller);
                    }
                }
                renderer.sharedMaterials = materials;
            }

            return renderer;
        }

        public override void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            // VRM 固有のリソース(ScriptableObject)
            take(SubAssetKey.Create(m_humanoid), m_humanoid);
            m_humanoid = null;

            if (m_vrmObject != null)
            {
                take(VRM10Object.SubAssetKey, m_vrmObject);
                m_vrmObject = null;
            }

            foreach (var (preset, x) in m_expressions)
            {
                take(new ExpressionKey(preset, x.name).SubAssetKey, x);
                // do nothing
            }
            m_expressions.Clear();

            // GLTF のリソース
            base.TransferOwnership(take);
        }

        public override void Dispose()
        {
            // VRM specific
            if (m_humanoid != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(m_humanoid);
                m_humanoid = null;
            }

            if (m_vrmObject != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(m_vrmObject);
                m_vrmObject = null;
            }

            foreach (var (preset, clip) in m_expressions)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(clip);
            }
            m_expressions.Clear();

            base.Dispose();
        }

        public sealed class ModelMap
        {
            public readonly Dictionary<VrmLib.Node, GameObject> Nodes = new Dictionary<VrmLib.Node, GameObject>();
            public readonly Dictionary<VrmLib.MeshGroup, UnityEngine.Mesh> Meshes = new Dictionary<VrmLib.MeshGroup, UnityEngine.Mesh>();
        }
    }
}