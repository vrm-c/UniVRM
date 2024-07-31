using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
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
        private readonly ModelMap m_map = new ModelMap();
        private readonly bool m_useControlRig;

        private VrmLib.Model m_model;
        private IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> m_externalMap;
        private Avatar m_humanoid;
        private VRM10Object m_vrmObject;
        private List<(ExpressionPreset Preset, VRM10Expression Clip)> m_expressions = new List<(ExpressionPreset, VRM10Expression)>();

        public Vrm10Importer(
            Vrm10Data vrm,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            bool useControlRig = false
            )
            : base(vrm.Data, externalObjectMap, textureDeserializer)
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

        public override async Task<RuntimeGltfInstance> LoadAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime = null)
        {
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            // NOTE: VRM データに対して、Load 前に必要なヘビーな変換処理を行う.
            //       ヘビーなため、別スレッドで Run する.
            await awaitCaller.Run(() =>
            {
                // bin に対して右手左手変換を破壊的に実行することに注意 !(bin が変換済みになる)
                m_model = ModelReader.Read(Data);

                // assign humanoid bones
                if (m_vrm.VrmExtension.Humanoid is UniGLTF.Extensions.VRMC_vrm.Humanoid humanoid)
                {
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Hips, VrmLib.HumanoidBones.hips);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftUpperLeg, VrmLib.HumanoidBones.leftUpperLeg);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightUpperLeg, VrmLib.HumanoidBones.rightUpperLeg);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftLowerLeg, VrmLib.HumanoidBones.leftLowerLeg);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightLowerLeg, VrmLib.HumanoidBones.rightLowerLeg);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftFoot, VrmLib.HumanoidBones.leftFoot);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightFoot, VrmLib.HumanoidBones.rightFoot);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Spine, VrmLib.HumanoidBones.spine);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Chest, VrmLib.HumanoidBones.chest);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Neck, VrmLib.HumanoidBones.neck);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Head, VrmLib.HumanoidBones.head);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftShoulder, VrmLib.HumanoidBones.leftShoulder);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightShoulder, VrmLib.HumanoidBones.rightShoulder);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftUpperArm, VrmLib.HumanoidBones.leftUpperArm);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightUpperArm, VrmLib.HumanoidBones.rightUpperArm);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftLowerArm, VrmLib.HumanoidBones.leftLowerArm);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightLowerArm, VrmLib.HumanoidBones.rightLowerArm);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftHand, VrmLib.HumanoidBones.leftHand);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightHand, VrmLib.HumanoidBones.rightHand);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftToes, VrmLib.HumanoidBones.leftToes);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightToes, VrmLib.HumanoidBones.rightToes);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftEye, VrmLib.HumanoidBones.leftEye);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightEye, VrmLib.HumanoidBones.rightEye);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.Jaw, VrmLib.HumanoidBones.jaw);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftThumbMetacarpal, VrmLib.HumanoidBones.leftThumbMetacarpal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftThumbProximal, VrmLib.HumanoidBones.leftThumbProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftThumbDistal, VrmLib.HumanoidBones.leftThumbDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftIndexProximal, VrmLib.HumanoidBones.leftIndexProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftIndexIntermediate, VrmLib.HumanoidBones.leftIndexIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftIndexDistal, VrmLib.HumanoidBones.leftIndexDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftMiddleProximal, VrmLib.HumanoidBones.leftMiddleProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftMiddleIntermediate, VrmLib.HumanoidBones.leftMiddleIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftMiddleDistal, VrmLib.HumanoidBones.leftMiddleDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftRingProximal, VrmLib.HumanoidBones.leftRingProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftRingIntermediate, VrmLib.HumanoidBones.leftRingIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftRingDistal, VrmLib.HumanoidBones.leftRingDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftLittleProximal, VrmLib.HumanoidBones.leftLittleProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftLittleIntermediate, VrmLib.HumanoidBones.leftLittleIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.LeftLittleDistal, VrmLib.HumanoidBones.leftLittleDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightThumbMetacarpal, VrmLib.HumanoidBones.rightThumbMetacarpal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightThumbProximal, VrmLib.HumanoidBones.rightThumbProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightThumbDistal, VrmLib.HumanoidBones.rightThumbDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightIndexProximal, VrmLib.HumanoidBones.rightIndexProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightIndexIntermediate, VrmLib.HumanoidBones.rightIndexIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightIndexDistal, VrmLib.HumanoidBones.rightIndexDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightMiddleProximal, VrmLib.HumanoidBones.rightMiddleProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightMiddleIntermediate, VrmLib.HumanoidBones.rightMiddleIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightMiddleDistal, VrmLib.HumanoidBones.rightMiddleDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightRingProximal, VrmLib.HumanoidBones.rightRingProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightRingIntermediate, VrmLib.HumanoidBones.rightRingIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightRingDistal, VrmLib.HumanoidBones.rightRingDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightLittleProximal, VrmLib.HumanoidBones.rightLittleProximal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightLittleIntermediate, VrmLib.HumanoidBones.rightLittleIntermediate);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.RightLittleDistal, VrmLib.HumanoidBones.rightLittleDistal);
                    AssignHumanoid(m_model.Nodes, humanoid.HumanBones.UpperChest, VrmLib.HumanoidBones.upperChest);
                }
            });

            return await base.LoadAsync(awaitCaller, MeasureTime);
        }

        /// <summary>
        /// VrmLib.Model から 構築する
        /// </summary>
        /// <param name="MeasureTime"></param>
        /// <returns></returns>
        protected override async Task LoadGeometryAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            // fill assets
            for (int i = 0; i < m_model.Materials.Count; ++i)
            {
                var src = m_model.Materials[i];
                var dst = MaterialFactory.Materials[i].Asset;
            }

            await awaitCaller.NextFrame();

            // mesh
            for (int i = 0; i < m_model.MeshGroups.Count; ++i)
            {
                var src = m_model.MeshGroups[i];
                UnityEngine.Mesh mesh = default;
                if (src.Meshes.Count == 1)
                {
                    mesh = MeshImporterShared.LoadSharedMesh(src.Meshes[0], src.Skin);
                }
                else
                {
                    // 頂点バッファの連結が必用
                    // VRM-1 はこっち
                    // https://github.com/vrm-c/UniVRM/issues/800
                    mesh = MeshImporterDivided.LoadDivided(src);
                }
                mesh.name = src.Name;

                m_map.Meshes.Add(src, mesh);
                Meshes.Add(new MeshWithMaterials
                {
                    Mesh = mesh,
                    Materials = src.Meshes[0].Submeshes.Select(
                        x =>
                        {
                            if (x.Material.HasValidIndex())
                            {
                                return MaterialFactory.Materials[x.Material.Value].Asset;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    ).ToArray(),
                });


                await awaitCaller.NextFrame();
            }

            // node: recursive
            CreateNodes(m_model.Root, null, m_map.Nodes);
            for (int i = 0; i < m_model.Nodes.Count; ++i)
            {
                Nodes.Add(m_map.Nodes[m_model.Nodes[i]].transform);
            }
            await awaitCaller.NextFrame();

            if (Root == null)
            {
                Root = m_map.Nodes[m_model.Root];
            }
            else
            {
                // replace
                var modelRoot = m_map.Nodes[m_model.Root];
                foreach (Transform child in modelRoot.transform)
                {
                    child.SetParent(Root.transform, true);
                }
                m_map.Nodes[m_model.Root] = Root;
            }
            await awaitCaller.NextFrame();

            // renderer
            var map = m_map;
            foreach (var (node, go) in map.Nodes.Select(kv => (kv.Key, kv.Value)))
            {
                if (node.MeshGroup is null)
                {
                    continue;
                }

                await CreateRendererAsync(node, go, map, MaterialFactory, awaitCaller);
                await awaitCaller.NextFrame();
            }
        }

        protected override async Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            Root.name = "VRM1";

            // humanoid
            var humanoid = Root.AddComponent<UniHumanoid.Humanoid>();
            humanoid.AssignBones(m_map.Nodes.Select(x => (ToUnity(x.Key.HumanoidBone.GetValueOrDefault()), x.Value.transform)));
            m_humanoid = humanoid.CreateAvatar();
            m_humanoid.name = "humanoid";
            var animator = Root.AddComponent<Animator>();
            animator.avatar = m_humanoid;

            // VrmController
            var controller = Root.AddComponent<Vrm10Instance>();
            controller.InitializeAtRuntime(m_useControlRig);
            controller.enabled = false;

            // vrm
            controller.Vrm = await LoadVrmAsync(awaitCaller, m_vrm.VrmExtension);

            // springBone
            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Data.GLTF.extensions, out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                await LoadSpringBoneAsync(awaitCaller, controller, springBone);
            }
            // constraint
            await LoadConstraintAsync(awaitCaller, controller);

            // Hierarchyの構築が終わるまで遅延させる
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
                    clip.MorphTargetBindings = expression.MorphTargetBinds?.Select(x => x.Build10(Root, m_map, m_model))
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
                    var node = Nodes[x.Node.Value];
                    var relative = node.RelativePathFrom(Root.transform);
                    vrm.FirstPerson.Renderers.Add(new RendererFirstPersonFlags
                    {
                        FirstPersonFlag = x.Type,
                        Renderer = relative,
                    });
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
                        spring.ColliderGroups = gltfSpring.ColliderGroups.Select(x => controller.SpringBone.ColliderGroups[x]).ToList();
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
                                Debug.LogWarning($"duplicated spring joint: {Data.TargetPath}");
                                continue;
                            }

                            joint = go.AddComponent<VRM10SpringBoneJoint>();
                            joint.m_jointRadius = gltfJoint.HitRadius.GetValueOrDefault(0.0f);
                            joint.m_dragForce = gltfJoint.DragForce.GetValueOrDefault(0.5f);
                            joint.m_gravityDir = gltfJoint.GravityDir != null ? Vector3InvertX(gltfJoint.GravityDir) : Vector3.down;
                            joint.m_gravityPower = gltfJoint.GravityPower.GetValueOrDefault(0.0f);
                            joint.m_stiffnessForce = gltfJoint.Stiffness.GetValueOrDefault(1.0f);
                            spring.Joints.Add(joint);
                        }
                    }
                }
            }
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