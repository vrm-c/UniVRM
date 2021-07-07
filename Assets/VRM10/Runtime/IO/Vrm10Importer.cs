using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;


namespace UniVRM10
{
    /// <summary>
    /// VrmLib.Model から UnityPrefab を構築する
    /// </summary>
    public class Vrm10Importer : UniGLTF.ImporterContext
    {
        readonly VrmLib.Model m_model;

        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm m_vrm;

        IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> m_externalMap;

        public Vrm10Importer(
            UniGLTF.GltfData data, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null)
            : base(data, externalObjectMap, textureDeserializer)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (vrm == null)
            {
                throw new ArgumentNullException("vrm");
            }
            m_vrm = vrm;

            TextureDescriptorGenerator = new Vrm10TextureDescriptorGenerator(data);
            MaterialDescriptorGenerator = new Vrm10MaterialDescriptorGenerator();

            m_externalMap = externalObjectMap;
            if (m_externalMap == null)
            {
                m_externalMap = new Dictionary<SubAssetKey, UnityEngine.Object>();
            }

            // bin に対して右手左手変換を破壊的に実行することに注意 !(bin が変換済みになる)
            m_model = ModelReader.Read(data);

            // assign humanoid bones
            if (m_vrm.Humanoid != null)
            {
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Hips, VrmLib.HumanoidBones.hips);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftUpperLeg, VrmLib.HumanoidBones.leftUpperLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightUpperLeg, VrmLib.HumanoidBones.rightUpperLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLowerLeg, VrmLib.HumanoidBones.leftLowerLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLowerLeg, VrmLib.HumanoidBones.rightLowerLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftFoot, VrmLib.HumanoidBones.leftFoot);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightFoot, VrmLib.HumanoidBones.rightFoot);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Spine, VrmLib.HumanoidBones.spine);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Chest, VrmLib.HumanoidBones.chest);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Neck, VrmLib.HumanoidBones.neck);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Head, VrmLib.HumanoidBones.head);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftShoulder, VrmLib.HumanoidBones.leftShoulder);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightShoulder, VrmLib.HumanoidBones.rightShoulder);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftUpperArm, VrmLib.HumanoidBones.leftUpperArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightUpperArm, VrmLib.HumanoidBones.rightUpperArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLowerArm, VrmLib.HumanoidBones.leftLowerArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLowerArm, VrmLib.HumanoidBones.rightLowerArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftHand, VrmLib.HumanoidBones.leftHand);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightHand, VrmLib.HumanoidBones.rightHand);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftToes, VrmLib.HumanoidBones.leftToes);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightToes, VrmLib.HumanoidBones.rightToes);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftEye, VrmLib.HumanoidBones.leftEye);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightEye, VrmLib.HumanoidBones.rightEye);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Jaw, VrmLib.HumanoidBones.jaw);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbProximal, VrmLib.HumanoidBones.leftThumbProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbIntermediate, VrmLib.HumanoidBones.leftThumbIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbDistal, VrmLib.HumanoidBones.leftThumbDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexProximal, VrmLib.HumanoidBones.leftIndexProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexIntermediate, VrmLib.HumanoidBones.leftIndexIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexDistal, VrmLib.HumanoidBones.leftIndexDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleProximal, VrmLib.HumanoidBones.leftMiddleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleIntermediate, VrmLib.HumanoidBones.leftMiddleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleDistal, VrmLib.HumanoidBones.leftMiddleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingProximal, VrmLib.HumanoidBones.leftRingProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingIntermediate, VrmLib.HumanoidBones.leftRingIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingDistal, VrmLib.HumanoidBones.leftRingDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleProximal, VrmLib.HumanoidBones.leftLittleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleIntermediate, VrmLib.HumanoidBones.leftLittleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleDistal, VrmLib.HumanoidBones.leftLittleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbProximal, VrmLib.HumanoidBones.rightThumbProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbIntermediate, VrmLib.HumanoidBones.rightThumbIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbDistal, VrmLib.HumanoidBones.rightThumbDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexProximal, VrmLib.HumanoidBones.rightIndexProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexIntermediate, VrmLib.HumanoidBones.rightIndexIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexDistal, VrmLib.HumanoidBones.rightIndexDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleProximal, VrmLib.HumanoidBones.rightMiddleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleIntermediate, VrmLib.HumanoidBones.rightMiddleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleDistal, VrmLib.HumanoidBones.rightMiddleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingProximal, VrmLib.HumanoidBones.rightRingProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingIntermediate, VrmLib.HumanoidBones.rightRingIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingDistal, VrmLib.HumanoidBones.rightRingDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleProximal, VrmLib.HumanoidBones.rightLittleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleIntermediate, VrmLib.HumanoidBones.rightLittleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleDistal, VrmLib.HumanoidBones.rightLittleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.UpperChest, VrmLib.HumanoidBones.upperChest);
            }
        }

        public class ModelMap
        {
            public readonly Dictionary<VrmLib.Node, GameObject> Nodes = new Dictionary<VrmLib.Node, GameObject>();
            public readonly Dictionary<VrmLib.MeshGroup, UnityEngine.Mesh> Meshes = new Dictionary<VrmLib.MeshGroup, UnityEngine.Mesh>();
        }

        /// <summary>
        /// VrmLib.Model の オブジェクトと UnityEngine.Object のマッピングを記録する
        /// </summary>
        /// <returns></returns>
        readonly ModelMap m_map = new ModelMap();

        static void AssignHumanoid(List<VrmLib.Node> nodes, UniGLTF.Extensions.VRMC_vrm.HumanBone humanBone, VrmLib.HumanoidBones key)
        {
            if (humanBone != null && humanBone.Node.HasValue)
            {
                nodes[humanBone.Node.Value].HumanoidBone = key;
            }
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
                    mesh = MeshImporter.LoadSharedMesh(src.Meshes[0], src.Skin);
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
                    Materials = src.Meshes[0].Submeshes.Select(x => MaterialFactory.Materials[x.Material].Asset).ToArray(),
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
            foreach (var (node, go) in map.Nodes)
            {
                if (node.MeshGroup is null)
                {
                    continue;
                }

                CreateRenderer(node, go, map, MaterialFactory.Materials);
                await awaitCaller.NextFrame();
            }
        }

        UnityEngine.Avatar m_humanoid;
        VRM10Object m_vrmObject;
        List<VRM10Expression> m_expressions = new List<VRM10Expression>();

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
            var controller = Root.AddComponent<VRM10Controller>();

            // vrm
            controller.Vrm = await LoadVrmAsync(awaitCaller, m_vrm);

            // springBone
            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Data.GLTF.extensions, out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                await LoadSpringBoneAsync(awaitCaller, controller, springBone);
            }
            // constraint
            await LoadConstraintAsync(awaitCaller, controller);
        }

        VRM10Expression GetOrLoadExpression(in SubAssetKey key, UniGLTF.Extensions.VRMC_vrm.Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            VRM10Expression clip = default;
            if (m_externalMap.TryGetValue(key, out UnityEngine.Object expressionObj))
            {
                clip = expressionObj as VRM10Expression;
            }
            else
            {
                clip = ScriptableObject.CreateInstance<UniVRM10.VRM10Expression>();
                clip.Preset = ExpressionPreset.custom;
                clip.ExpressionName = key.Name;
                clip.name = key.Name;
                clip.IsBinary = expression.IsBinary.GetValueOrDefault();
                clip.OverrideBlink = expression.OverrideBlink;
                clip.OverrideLookAt = expression.OverrideLookAt;
                clip.OverrideMouth = expression.OverrideMouth;

                clip.MorphTargetBindings = expression?.MorphTargetBinds?.Select(x => x.Build10(Root, m_map, m_model))
                    .ToArray();
                clip.MaterialColorBindings = expression?.MaterialColorBinds?.Select(x => x.Build10(MaterialFactory.Materials))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray();
                clip.MaterialUVBindings = expression?.TextureTransformBinds?.Select(x => x.Build10(MaterialFactory.Materials))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray();
                m_expressions.Add(clip);
            }
            return clip;
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
                meta.ContactInformation = src.ContactInformation;
                // avatar
                meta.AllowedUser = src.AvatarPermission;
                meta.ViolentUsage = src.AllowExcessivelyViolentUsage.Value;
                meta.SexualUsage = src.AllowExcessivelySexualUsage.Value;
                meta.CommercialUsage = src.CommercialUsage;
                meta.PoliticalOrReligiousUsage = src.AllowPoliticalOrReligiousUsage.Value;
                // redistribution
                meta.CreditNotation = src.CreditNotation;
                meta.Redistribution = src.AllowRedistribution.Value;
                meta.ModificationLicense = src.Modification;
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
                if (Vrm10TextureDescriptorGenerator.TryGetMetaThumbnailTextureImportParam(Data, vrmExtension, out (SubAssetKey, VRMShaders.TextureDescriptor Param) kv))
                {
                    var texture = await TextureFactory.GetTextureAsync(kv.Param, awaitCaller);
                    if (texture is Texture2D tex2D)
                    {
                        meta.Thumbnail = tex2D;
                    }
                }
            }

            // expression
            if (vrmExtension.Expressions != null)
            {
                vrm.Expression.Happy = GetOrLoadExpression(ExpressionKey.Happy.SubAssetKey, vrmExtension.Expressions.Preset.Happy);
                vrm.Expression.Angry = GetOrLoadExpression(ExpressionKey.Angry.SubAssetKey, vrmExtension.Expressions.Preset.Angry);
                vrm.Expression.Sad = GetOrLoadExpression(ExpressionKey.Sad.SubAssetKey, vrmExtension.Expressions.Preset.Sad);
                vrm.Expression.Relaxed = GetOrLoadExpression(ExpressionKey.Relaxed.SubAssetKey, vrmExtension.Expressions.Preset.Relaxed);
                vrm.Expression.Surprised = GetOrLoadExpression(ExpressionKey.Surprised.SubAssetKey, vrmExtension.Expressions.Preset.Surprised);
                vrm.Expression.Aa = GetOrLoadExpression(ExpressionKey.Aa.SubAssetKey, vrmExtension.Expressions.Preset.Aa);
                vrm.Expression.Ih = GetOrLoadExpression(ExpressionKey.Ih.SubAssetKey, vrmExtension.Expressions.Preset.Ih);
                vrm.Expression.Ou = GetOrLoadExpression(ExpressionKey.Ou.SubAssetKey, vrmExtension.Expressions.Preset.Ou);
                vrm.Expression.Ee = GetOrLoadExpression(ExpressionKey.Ee.SubAssetKey, vrmExtension.Expressions.Preset.Ee);
                vrm.Expression.Oh = GetOrLoadExpression(ExpressionKey.Oh.SubAssetKey, vrmExtension.Expressions.Preset.Oh);
                vrm.Expression.Blink = GetOrLoadExpression(ExpressionKey.Blink.SubAssetKey, vrmExtension.Expressions.Preset.Blink);
                vrm.Expression.BlinkLeft = GetOrLoadExpression(ExpressionKey.BlinkLeft.SubAssetKey, vrmExtension.Expressions.Preset.BlinkLeft);
                vrm.Expression.BlinkRight = GetOrLoadExpression(ExpressionKey.BlinkRight.SubAssetKey, vrmExtension.Expressions.Preset.BlinkRight);
                vrm.Expression.LookUp = GetOrLoadExpression(ExpressionKey.LookUp.SubAssetKey, vrmExtension.Expressions.Preset.LookUp);
                vrm.Expression.LookDown = GetOrLoadExpression(ExpressionKey.LookDown.SubAssetKey, vrmExtension.Expressions.Preset.LookDown);
                vrm.Expression.LookLeft = GetOrLoadExpression(ExpressionKey.LookLeft.SubAssetKey, vrmExtension.Expressions.Preset.LookLeft);
                vrm.Expression.LookRight = GetOrLoadExpression(ExpressionKey.LookRight.SubAssetKey, vrmExtension.Expressions.Preset.LookRight);
                foreach (var (name, expression) in vrmExtension.Expressions.Custom)
                {
                    var key = ExpressionKey.CreateCustom(name);
                    var clip = GetOrLoadExpression(key.SubAssetKey, expression);
                    if (clip != null)
                    {
                        vrm.Expression.AddClip(clip);
                    }
                }
            }

            // lookat
            if (vrmExtension.LookAt != null)
            {
                var src = vrmExtension.LookAt;
                vrm.LookAt.LookAtType = src.Type;
                vrm.LookAt.OffsetFromHead = new Vector3(src.OffsetFromHeadBone[0], src.OffsetFromHeadBone[1], src.OffsetFromHeadBone[2]);
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

            return vrm;
        }

        async Task LoadSpringBoneAsync(IAwaitCaller awaitCaller, VRM10Controller controller, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone)
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
                        throw new Vrm10Exception();
                    }
                }

                var secondary = Root.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = new GameObject("secondary").transform;
                    secondary.SetParent(Root.transform, false);
                }

                // colliderGroup
                if (gltfVrmSpringBone.ColliderGroups != null)
                {
                    foreach (var g in gltfVrmSpringBone.ColliderGroups)
                    {
                        var colliderGroup = secondary.gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                        controller.SpringBone.ColliderGroups.Add(colliderGroup);

                        foreach (var c in g.Colliders)
                        {
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
                    var spring = new VRM10ControllerSpringBone.Spring(gltfSpring.Name);
                    controller.SpringBone.Springs.Add(spring);

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
                            var joint = Nodes[gltfJoint.Node.Value].gameObject.AddComponent<VRM10SpringBoneJoint>();
                            joint.m_jointRadius = gltfJoint.HitRadius.Value;
                            joint.m_dragForce = gltfJoint.DragForce.Value;
                            joint.m_gravityDir = Vector3InvertX(gltfJoint.GravityDir);
                            joint.m_gravityPower = gltfJoint.GravityPower.Value;
                            joint.m_stiffnessForce = gltfJoint.Stiffness.Value;
                            // joint.m_exclude = gltfJoint.Exclude.GetValueOrDefault();
                            spring.Joints.Add(joint);
                        }
                    }
                }
            }
        }

        static AxisMask FreezeAxis(bool[] flags)
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

        async Task LoadConstraintAsync(IAwaitCaller awaitCaller, VRM10Controller controller)
        {
            for (int i = 0; i < Data.GLTF.nodes.Count; ++i)
            {
                var gltfNode = Data.GLTF.nodes[i];
                if (UniGLTF.Extensions.VRMC_node_constraint.GltfDeserializer.TryGet(gltfNode.extensions, out UniGLTF.Extensions.VRMC_node_constraint.VRMC_node_constraint ext))
                {
                    var constraint = ext.Constraint;
                    var node = Nodes[i];
                    if (constraint.Position != null)
                    {
                        var p = constraint.Position;
                        var positionConstraint = node.gameObject.AddComponent<VRM10PositionConstraint>();
                        positionConstraint.SourceCoordinate = p.SourceSpace;
                        positionConstraint.Source = Nodes[p.Source.Value];
                        positionConstraint.DestinationCoordinate = p.DestinationSpace;
                        positionConstraint.FreezeAxes = FreezeAxis(p.FreezeAxes);
                        positionConstraint.Weight = p.Weight.Value;
                        positionConstraint.ModelRoot = Root.transform;
                    }
                    else if (constraint.Rotation != null)
                    {
                        var r = constraint.Rotation;
                        var rotationConstraint = node.gameObject.AddComponent<VRM10RotationConstraint>();
                        rotationConstraint.SourceCoordinate = r.SourceSpace;
                        rotationConstraint.Source = Nodes[r.Source.Value];
                        rotationConstraint.DestinationCoordinate = r.DestinationSpace;
                        rotationConstraint.FreezeAxes = FreezeAxis(r.FreezeAxes);
                        rotationConstraint.Weight = r.Weight.Value;
                        rotationConstraint.ModelRoot = Root.transform;
                    }
                    else if (constraint.Aim != null)
                    {
                        var a = constraint.Aim;
                        var aimConstraint = node.gameObject.AddComponent<VRM10AimConstraint>();
                        aimConstraint.Source = Nodes[a.Source.Value];
                        // aimConstraint.AimVector = Vector3InvertX(a.AimVector);
                        // aimConstraint.UpVector = Vector3InvertX(a.UpVector);
                    }
                }
            }

            await awaitCaller.NextFrame();
        }

        public static HumanBodyBones ToUnity(VrmLib.HumanoidBones bone)
        {
            if (bone == VrmLib.HumanoidBones.unknown)
            {
                return HumanBodyBones.LastBone;
            }
            return VrmLib.EnumUtil.Cast<HumanBodyBones>(bone);
        }

        /// <summary>
        /// ヒエラルキーを再帰的に構築する
        /// <summary>
        public static void CreateNodes(VrmLib.Node node, GameObject parent, Dictionary<VrmLib.Node, GameObject> nodes)
        {
            GameObject go = new GameObject(node.Name);
            go.transform.SetPositionAndRotation(node.Translation.ToUnityVector3(), node.Rotation.ToUnityQuaternion());
            nodes.Add(node, go);
            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
            }

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
        public static Renderer CreateRenderer(VrmLib.Node node, GameObject go, ModelMap map,
            IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo> materialLoadInfos)
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

            if (node.MeshGroup.Meshes.Count == 0)
            {
                throw new NotImplementedException();
            }
            else if (node.MeshGroup.Meshes.Count == 1)
            {
                var materials = node.MeshGroup.Meshes[0].Submeshes.Select(x => materialLoadInfos[x.Material].Asset).ToArray();
                renderer.sharedMaterials = materials;
            }
            else
            {
                var materials = node.MeshGroup.Meshes.Select(x => materialLoadInfos[x.Submeshes[0].Material].Asset).ToArray();
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

            foreach (var x in m_expressions)
            {
                take(ExpressionKey.CreateFromClip(x).SubAssetKey, x);
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
                UnityObjectDestoyer.DestroyRuntimeOrEditor(m_humanoid);
                m_humanoid = null;
            }

            if (m_vrmObject != null)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(m_vrmObject);
                m_vrmObject = null;
            }

            foreach (var clip in m_expressions)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(clip);
            }
            m_expressions.Clear();

            base.Dispose();
        }
    }
}
