using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VrmLib;
using VRMShaders;


namespace UniVRM10
{
    /// <summary>
    /// VrmLib.Model から UnityPrefab を構築する
    /// </summary>
    public class Vrm10Importer : UniGLTF.ImporterContext
    {
        readonly Model m_model;

        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm m_vrm;

        IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> m_externalMap;

        public Vrm10Importer(UniGLTF.GltfParser parser, IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null)
        : base(parser, externalObjectMap)
        {
            m_externalMap = externalObjectMap;
            if (m_externalMap == null)
            {
                m_externalMap = new Dictionary<SubAssetKey, UnityEngine.Object>();
            }
            m_model = ModelReader.Read(parser);

            // for `VRMC_materials_mtoon`
            this.GltfMaterialImporter.GltfMaterialParamProcessors.Insert(0, Vrm10MaterialImporter.TryCreateParam);

            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out m_vrm))
            {
                throw new Exception("VRMC_vrm is not found");
            }

            // assign humanoid bones
            if (m_vrm.Humanoid != null)
            {
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Hips, HumanoidBones.hips);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftUpperLeg, HumanoidBones.leftUpperLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightUpperLeg, HumanoidBones.rightUpperLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLowerLeg, HumanoidBones.leftLowerLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLowerLeg, HumanoidBones.rightLowerLeg);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftFoot, HumanoidBones.leftFoot);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightFoot, HumanoidBones.rightFoot);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Spine, HumanoidBones.spine);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Chest, HumanoidBones.chest);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Neck, HumanoidBones.neck);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Head, HumanoidBones.head);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftShoulder, HumanoidBones.leftShoulder);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightShoulder, HumanoidBones.rightShoulder);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftUpperArm, HumanoidBones.leftUpperArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightUpperArm, HumanoidBones.rightUpperArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLowerArm, HumanoidBones.leftLowerArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLowerArm, HumanoidBones.rightLowerArm);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftHand, HumanoidBones.leftHand);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightHand, HumanoidBones.rightHand);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftToes, HumanoidBones.leftToes);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightToes, HumanoidBones.rightToes);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftEye, HumanoidBones.leftEye);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightEye, HumanoidBones.rightEye);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.Jaw, HumanoidBones.jaw);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbProximal, HumanoidBones.leftThumbProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbIntermediate, HumanoidBones.leftThumbIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftThumbDistal, HumanoidBones.leftThumbDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexProximal, HumanoidBones.leftIndexProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexIntermediate, HumanoidBones.leftIndexIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftIndexDistal, HumanoidBones.leftIndexDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleProximal, HumanoidBones.leftMiddleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleIntermediate, HumanoidBones.leftMiddleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftMiddleDistal, HumanoidBones.leftMiddleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingProximal, HumanoidBones.leftRingProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingIntermediate, HumanoidBones.leftRingIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftRingDistal, HumanoidBones.leftRingDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleProximal, HumanoidBones.leftLittleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleIntermediate, HumanoidBones.leftLittleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.LeftLittleDistal, HumanoidBones.leftLittleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbProximal, HumanoidBones.rightThumbProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbIntermediate, HumanoidBones.rightThumbIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightThumbDistal, HumanoidBones.rightThumbDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexProximal, HumanoidBones.rightIndexProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexIntermediate, HumanoidBones.rightIndexIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightIndexDistal, HumanoidBones.rightIndexDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleProximal, HumanoidBones.rightMiddleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleIntermediate, HumanoidBones.rightMiddleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightMiddleDistal, HumanoidBones.rightMiddleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingProximal, HumanoidBones.rightRingProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingIntermediate, HumanoidBones.rightRingIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightRingDistal, HumanoidBones.rightRingDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleProximal, HumanoidBones.rightLittleProximal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleIntermediate, HumanoidBones.rightLittleIntermediate);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.RightLittleDistal, HumanoidBones.rightLittleDistal);
                AssignHumanoid(m_model.Nodes, m_vrm.Humanoid.HumanBones.UpperChest, HumanoidBones.upperChest);
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

        static void AssignHumanoid(List<Node> nodes, UniGLTF.Extensions.VRMC_vrm.HumanBone humanBone, VrmLib.HumanoidBones key)
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
        VRM10MetaObject m_meta;
        List<VRM10Expression> m_expressions = new List<VRM10Expression>();

        protected override async Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            Root.name = "VRM1";

            // humanoid
            var humanoid = Root.AddComponent<MeshUtility.Humanoid>();
            humanoid.AssignBones(m_map.Nodes.Select(x => (ToUnity(x.Key.HumanoidBone.GetValueOrDefault()), x.Value.transform)));
            m_humanoid = humanoid.CreateAvatar();
            m_humanoid.name = "humanoid";
            var animator = Root.AddComponent<Animator>();
            animator.avatar = m_humanoid;

            // VrmController
            var controller = Root.AddComponent<VRM10Controller>();

            // vrm
            await LoadVrmAsync(awaitCaller, controller, m_vrm);
            // springBone
            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                await LoadSpringBoneAsync(awaitCaller, controller, springBone);
            }
            // constraint
            await LoadConstraintAsync(awaitCaller, controller);
        }

        static ExpressionKey Key(UniGLTF.Extensions.VRMC_vrm.Expression e)
        {
            if (e.Preset == UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
            {
                return ExpressionKey.CreateCustom(e.Name);
            }
            else
            {
                return ExpressionKey.CreateFromPreset(e.Preset);
            }
        }

        async Task LoadVrmAsync(IAwaitCaller awaitCaller, VRM10Controller controller, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm)
        {
            // meta
            if (m_externalMap.TryGetValue(VRM10MetaObject.SubAssetKey, out UnityEngine.Object meta))
            {
                controller.Meta.Meta = meta as VRM10MetaObject;
            }
            else if (vrm.Meta != null)
            {
                var src = vrm.Meta;
                m_meta = ScriptableObject.CreateInstance<VRM10MetaObject>();
                m_meta.name = VRM10MetaObject.SubAssetKey.Name;
                controller.Meta.Meta = m_meta;
                m_meta.Name = src.Name;
                m_meta.Version = src.Version;
                m_meta.ContactInformation = src.ContactInformation;
                // avatar
                m_meta.AllowedUser = src.AvatarPermission;
                m_meta.ViolentUsage = src.AllowExcessivelyViolentUsage.Value;
                m_meta.SexualUsage = src.AllowExcessivelySexualUsage.Value;
                m_meta.CommercialUsage = src.CommercialUsage;
                m_meta.PoliticalOrReligiousUsage = src.AllowPoliticalOrReligiousUsage.Value;
                // redistribution
                m_meta.CreditNotation = src.CreditNotation;
                m_meta.Redistribution = src.AllowRedistribution.Value;
                m_meta.ModificationLicense = src.Modification;
                m_meta.OtherLicenseUrl = src.OtherLicenseUrl;
                //
                if (src.References != null)
                {
                    m_meta.References.AddRange(src.References);
                }
                if (src.Authors != null)
                {
                    m_meta.Authors.AddRange(src.Authors);
                }
                if (Vrm10TextureEnumerator.TryGetMetaThumbnailTextureImportParam(Parser, vrm, out (SubAssetKey, VRMShaders.TextureImportParam Param) kv))
                {
                    var texture = await TextureFactory.GetTextureAsync(kv.Param);
                    if (texture is Texture2D tex2D)
                    {
                        m_meta.Thumbnail = tex2D;
                    }
                }
            }

            // expression
            if (vrm.Expressions != null)
            {
                var expressionAvatar = Root.AddComponent<VRM10ExpressionAvatar>();

                foreach (var expression in vrm.Expressions)
                {
                    VRM10Expression clip = default;
                    if (m_externalMap.TryGetValue(Key(expression).SubAssetKey, out UnityEngine.Object expressionObj))
                    {
                        clip = expressionObj as VRM10Expression;
                    }
                    else
                    {
                        clip = ScriptableObject.CreateInstance<UniVRM10.VRM10Expression>();
                        clip.Preset = expression.Preset;
                        clip.ExpressionName = expression.Name;
                        clip.name = Key(expression).SubAssetKey.Name;
                        clip.IsBinary = expression.IsBinary.GetValueOrDefault();
                        clip.OverrideBlink = expression.OverrideBlink;
                        clip.OverrideLookAt = expression.OverrideLookAt;
                        clip.OverrideMouth = expression.OverrideMouth;

                        clip.MorphTargetBindings = expression.MorphTargetBinds.Select(x => x.Build10(Root, m_map, m_model))
                            .ToArray();
                        clip.MaterialColorBindings = expression.MaterialColorBinds.Select(x => x.Build10(MaterialFactory.Materials))
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToArray();
                        clip.MaterialUVBindings = expression.TextureTransformBinds.Select(x => x.Build10(MaterialFactory.Materials))
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToArray();
                        m_expressions.Add(clip);
                    }

                    expressionAvatar.Clips.Add(clip);
                }
            }

            // lookat
            if (vrm.LookAt != null)
            {
                var src = vrm.LookAt;
                controller.LookAt.LookAtType = src.Type;
                controller.LookAt.OffsetFromHead = new Vector3(src.OffsetFromHeadBone[0], src.OffsetFromHeadBone[1], src.OffsetFromHeadBone[2]);
                controller.LookAt.HorizontalInner = new CurveMapper(src.RangeMapHorizontalInner.InputMaxValue.Value, src.RangeMapHorizontalInner.OutputScale.Value);
                controller.LookAt.HorizontalOuter = new CurveMapper(src.RangeMapHorizontalOuter.InputMaxValue.Value, src.RangeMapHorizontalOuter.OutputScale.Value);
                controller.LookAt.VerticalUp = new CurveMapper(src.RangeMapVerticalUp.InputMaxValue.Value, src.RangeMapVerticalUp.OutputScale.Value);
                controller.LookAt.VerticalDown = new CurveMapper(src.RangeMapVerticalDown.InputMaxValue.Value, src.RangeMapVerticalDown.OutputScale.Value);
            }

            // firstPerson
            if (vrm.FirstPerson != null && vrm.FirstPerson.MeshAnnotations != null)
            {
                var fp = vrm.FirstPerson;
                foreach (var x in fp.MeshAnnotations)
                {
                    var node = Nodes[x.Node.Value];
                    controller.FirstPerson.Renderers.Add(new RendererFirstPersonFlags
                    {
                        FirstPersonFlag = x.Type,
                        Renderer = node.GetComponent<Renderer>()
                    });
                }
            }
        }

        async Task LoadSpringBoneAsync(IAwaitCaller awaitCaller, VRM10Controller controller, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone)
        {
            await awaitCaller.NextFrame();

            // springs
            if (gltfVrmSpringBone.Springs != null)
            {
                var secondary = Root.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = new GameObject("secondary").transform;
                    secondary.SetParent(Root.transform, false);
                }

                // colliderGroup
                foreach (var g in gltfVrmSpringBone.ColliderGroups)
                {
                    var colliderGroup = secondary.gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                    controller.SpringBone.ColliderGroups.Add(colliderGroup);

                    foreach (var c in g.Colliders)
                    {
                        var node = Nodes[c.Node.Value];

                        var collider = node.gameObject.AddComponent<VRM10SpringBoneCollider>();
                        colliderGroup.Colliders.Add(collider);

                        if (c.Shape.Sphere is UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere sphere)
                        {
                            collider.ColliderType = VRM10SpringBoneColliderTypes.Sphere;
                            collider.Radius = sphere.Radius.Value;
                            collider.Offset = Vector3InvertX(sphere.Offset);
                        }
                        else if (c.Shape.Capsule is UniGLTF.Extensions.VRMC_springBone.ColliderShapeCapsule capsule)
                        {
                            collider.ColliderType = VRM10SpringBoneColliderTypes.Capsule;
                            collider.Radius = capsule.Radius.Value;
                            collider.Offset = Vector3InvertX(capsule.Offset);
                            collider.Tail = Vector3InvertX(capsule.Tail);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                foreach (var gltfSpring in gltfVrmSpringBone.Springs)
                {
                    if (gltfSpring.Joints == null || gltfSpring.Joints.Count == 0)
                    {
                        continue;
                    }
                    var spring = new VRM10ControllerSpringBone.Spring(gltfSpring.Name);
                    controller.SpringBone.Springs.Add(spring);

                    spring.ColliderGroups = gltfSpring.ColliderGroups.Select(x => controller.SpringBone.ColliderGroups[x]).ToList();
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
                            var joint = Nodes[gltfJoint.Node.Value].gameObject.AddComponent<VRM10SpringJoint>();
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
            for (int i = 0; i < Parser.GLTF.nodes.Count; ++i)
            {
                var gltfNode = Parser.GLTF.nodes[i];
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

        public override void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            // VRM 固有のリソース(ScriptableObject)
            if (take(m_humanoid))
            {
                m_humanoid = null;
            }

            if (m_meta != null)
            {
                if (take(m_meta))
                {
                    m_meta = null;
                }
            }

            foreach (var x in m_expressions)
            {
                if (take(x))
                {
                    // do nothing
                }
            }
            m_expressions.Clear();

            // GLTF のリソース
            base.TransferOwnership(take);
        }

        public override void Dispose()
        {
            Action<UnityEngine.Object> destroy = UnityResourceDestroyer.DestroyResource();

            // VRM specific
            if (m_humanoid != null)
            {
                destroy(m_humanoid);
            }
            if (m_meta != null)
            {
                destroy(m_meta);
            }

            foreach (var clip in m_expressions)
            {
                destroy(clip);
            }

            base.Dispose();
        }
    }
}
