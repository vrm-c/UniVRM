using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VrmLib;


namespace UniVRM10
{
    /// <summary>
    /// VrmLib.Model から UnityPrefab を構築する
    /// </summary>
    public class RuntimeUnityBuilder : UniGLTF.ImporterContext
    {
        readonly Model m_model;

        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm m_vrm;

        public RuntimeUnityBuilder(UniGLTF.GltfParser parser, IEnumerable<(string, UnityEngine.Object)> externalObjectMap = null) : base(parser, externalObjectMap)
        {
            m_model = VrmLoader.CreateVrmModel(parser);

            // for `VRMC_materials_mtoon`
            this.GltfMaterialImporter.GltfMaterialParamProcessors.Insert(0, Vrm10MToonMaterialImporter.TryCreateParam);

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
                if (src.Meshes.Count == 1)
                {
                    // submesh 方式
                    var mesh = new UnityEngine.Mesh();
                    mesh.name = src.Name;
                    mesh.LoadMesh(src.Meshes[0], src.Skin);
                    m_map.Meshes.Add(src, mesh);
                    Meshes.Add(new MeshWithMaterials
                    {
                        Mesh = mesh,
                        Materials = src.Meshes[0].Submeshes.Select(x => MaterialFactory.Materials[x.Material].Asset).ToArray(),
                    });
                }
                else
                {
                    // 頂点バッファの連結が必用
                    throw new NotImplementedException();
                }

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

                if (node.MeshGroup.Meshes.Count > 1)
                {
                    throw new NotImplementedException("invalid isolated vertexbuffer");
                }

                var renderer = CreateRenderer(node, go, map, MaterialFactory.Materials);
                await awaitCaller.NextFrame();
            }
        }

        UnityEngine.Avatar m_humanoid;
        VRM10MetaObject m_meta;
        VRM10ExpressionAvatar m_exressionAvatar;

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
            if (vrm.Meta != null)
            {
                var src = vrm.Meta;
                m_meta = ScriptableObject.CreateInstance<VRM10MetaObject>();
                m_meta.name = VRM10MetaObject.ExtractKey;
                controller.Meta = m_meta;
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
                if (Vrm10MToonMaterialImporter.TryGetMetaThumbnailTextureImportParam(Parser, vrm, out VRMShaders.TextureImportParam param))
                {
                    var texture = await TextureFactory.GetTextureAsync(param);
                    if (texture != null)
                    {
                        m_meta.Thumbnail = texture;
                    }
                }
            }

            // expression
            if (vrm.Expressions != null)
            {
                controller.Expression.ExpressionAvatar = ScriptableObject.CreateInstance<VRM10ExpressionAvatar>();

                m_exressionAvatar = controller.Expression.ExpressionAvatar;
                m_exressionAvatar.name = VRM10ExpressionAvatar.ExtractKey;

                foreach (var expression in vrm.Expressions)
                {
                    var clip = ScriptableObject.CreateInstance<UniVRM10.VRM10Expression>();
                    clip.Preset = expression.Preset;
                    clip.ExpressionName = expression.Name;
                    clip.name = Key(expression).ExtractKey;
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

                    m_exressionAvatar.Clips.Add(clip);
                }
            }

            // lookat
            if (vrm.LookAt != null)
            {
                var src = vrm.LookAt;
                controller.LookAt.LookAtType = src.LookAtType;
                controller.LookAt.OffsetFromHead = new Vector3(src.OffsetFromHeadBone[0], src.OffsetFromHeadBone[1], src.OffsetFromHeadBone[2]);
                controller.LookAt.HorizontalInner = new CurveMapper(src.LookAtHorizontalInner.InputMaxValue.Value, src.LookAtHorizontalInner.OutputScale.Value);
                controller.LookAt.HorizontalOuter = new CurveMapper(src.LookAtHorizontalOuter.InputMaxValue.Value, src.LookAtHorizontalOuter.OutputScale.Value);
                controller.LookAt.VerticalUp = new CurveMapper(src.LookAtVerticalUp.InputMaxValue.Value, src.LookAtHorizontalOuter.OutputScale.Value);
                controller.LookAt.VerticalDown = new CurveMapper(src.LookAtVerticalDown.InputMaxValue.Value, src.LookAtHorizontalOuter.OutputScale.Value);
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
                        FirstPersonFlag = x.FirstPersonType,
                        Renderer = node.GetComponent<Renderer>()
                    });
                }
            }
        }

        async Task LoadSpringBoneAsync(IAwaitCaller awaitCaller, VRM10Controller controller, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone)
        {
            await awaitCaller.NextFrame();

            var springBoneManager = controller.SpringBone;

            // springs
            if (gltfVrmSpringBone.Springs != null)
            {
                foreach (var gltfSpring in gltfVrmSpringBone.Springs)
                {
                    var springBone = new VRM10SpringBone();
                    springBone.Comment = gltfSpring.Name;

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
                            var joint = new VRM10SpringJoint(Nodes[gltfJoint.Node.Value]);
                            joint.m_jointRadius = gltfJoint.HitRadius.Value;
                            joint.m_dragForce = gltfJoint.DragForce.Value;
                            joint.m_gravityDir = Vector3(gltfJoint.GravityDir);
                            joint.m_gravityPower = gltfJoint.GravityPower.Value;
                            joint.m_stiffnessForce = gltfJoint.Stiffness.Value;
                            // joint.m_exclude = gltfJoint.Exclude.GetValueOrDefault();
                            springBone.Joints.Add(joint);
                        }
                    }

                    // collider
                    springBone.ColliderGroups.AddRange(gltfSpring.Colliders.Select(colliderNode =>
                    {
                        if (UniGLTF.Extensions.VRMC_node_collider.GltfDeserializer.TryGet(Parser.GLTF.nodes[colliderNode].extensions,
                            out UniGLTF.Extensions.VRMC_node_collider.VRMC_node_collider extension))
                        {
                            var node = Nodes[colliderNode];
                            var colliderGroup = node.gameObject.GetOrAddComponent<VRM10SpringBoneColliderGroup>();
                            colliderGroup.Colliders.AddRange(extension.Shapes.Select(x =>
                            {
                                if (x.Sphere != null)
                                {
                                    return new VRM10SpringBoneCollider
                                    {
                                        ColliderType = VRM10SpringBoneColliderTypes.Sphere,
                                        Offset = Vector3(x.Sphere.Offset),
                                        Radius = x.Sphere.Radius.Value,
                                    };
                                }
                                else if (x.Capsule != null)
                                {
                                    return new VRM10SpringBoneCollider
                                    {
                                        ColliderType = VRM10SpringBoneColliderTypes.Capsule,
                                        Offset = Vector3(x.Capsule.Offset),
                                        Radius = x.Capsule.Radius.Value,
                                        Tail = Vector3(x.Capsule.Tail),
                                    };
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }));
                            return colliderGroup;
                        }
                        else
                        {
                            return null;
                        }
                    }).Where(x => x != null));

                    springBoneManager.Springs.Add(springBone);
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

        static Vector3 Vector3(float[] f)
        {
            var v = default(Vector3);
            if (f != null && f.Length == 3)
            {
                v.x = f[0];
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
                if (UniGLTF.Extensions.VRMC_constraints.GltfDeserializer.TryGet(gltfNode.extensions, out UniGLTF.Extensions.VRMC_constraints.VRMC_constraints constraint))
                {
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
                        aimConstraint.AimVector = Vector3(a.AimVector);
                        aimConstraint.UpVector = Vector3(a.UpVector);
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
            var mesh = node.MeshGroup.Meshes[0];

            Renderer renderer = null;
            var hasBlendShape = mesh.MorphTargets.Any();
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
            var materials = mesh.Submeshes.Select(x => materialLoadInfos[x.Material].Asset).ToArray();
            renderer.sharedMaterials = materials;

            return renderer;
        }

        public override void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            // VRM 固有のリソース(ScriptableObject)
            if (take(m_humanoid))
            {
                m_humanoid = null;
            }

            if (take(m_meta))
            {
                m_meta = null;
            }

            if (m_exressionAvatar != null && m_exressionAvatar.Clips != null)
            {
                foreach (var x in m_exressionAvatar.Clips)
                {
                    if (take(x))
                    {
                        // do nothing
                    }
                }

                if (take(m_exressionAvatar))
                {
                    m_exressionAvatar = null;
                }
            }

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
            if (m_exressionAvatar != null)
            {
                foreach (var clip in m_exressionAvatar.Clips)
                {
                    destroy(clip);
                }
                destroy(m_exressionAvatar);
            }

            base.Dispose();
        }
    }
}
