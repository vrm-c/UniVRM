using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public class Vrm10Exporter : IDisposable
    {
        public const string VRM_SPEC_VERSION = "1.0";
        public const string SPRINGBONE_SPEC_VERSION = "1.0";
        public const string SPRINGBONE_EXTENDED_COLLIDER_SPEC_VERSION = "1.0";
        public const string NODE_CONSTRAINT_SPEC_VERSION = "1.0";
        public const string MTOON_SPEC_VERSION = "1.0";

        public const string LICENSE_URL_JA = "https://vrm.dev/licenses/1.0/";
        public const string LICENSE_URL_EN = "https://vrm.dev/licenses/1.0/en/";

        public readonly ExportingGltfData Storage = new ExportingGltfData();

        public readonly string VrmExtensionName = "VRMC_vrm";

        IMaterialExporter m_materialExporter;
        ITextureSerializer m_textureSerializer;
        TextureExporter m_textureExporter;

        GltfExportSettings m_settings;

        public Vrm10Exporter(
            GltfExportSettings settings,
            IMaterialExporter materialExporter = null,
            ITextureSerializer textureSerializer = null
        )
        {
            m_settings = settings ?? throw new ArgumentException(nameof(settings));
            m_materialExporter = materialExporter ?? Vrm10MaterialExporterUtility.GetValidVrm10MaterialExporter();
            m_textureSerializer = textureSerializer ?? new RuntimeTextureSerializer();
            m_textureExporter = new TextureExporter(m_textureSerializer);

            Storage.Gltf.extensionsUsed.Add(glTF_KHR_texture_transform.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(glTF_KHR_materials_unlit.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_node_constraint.VRMC_node_constraint.ExtensionName);
        }

        public void Dispose()
        {
            m_textureExporter.Dispose();
        }

        public static glTFAssets ExportAsset(Model model)
        {
            var asset = new glTFAssets();
            if (!string.IsNullOrEmpty(model.AssetVersion)) asset.version = model.AssetVersion;
            if (!string.IsNullOrEmpty(model.AssetMinVersion)) asset.minVersion = model.AssetMinVersion;
            if (!string.IsNullOrEmpty(model.AssetGenerator)) asset.generator = model.AssetGenerator;
            if (!string.IsNullOrEmpty(model.AssetCopyright)) asset.copyright = model.AssetCopyright;
            return asset;
        }

        public static IEnumerable<glTFMesh> ExportMeshes(List<MeshGroup> groups, List<object> materials, ExportingGltfData data, ExportArgs option)
        {
            foreach (var group in groups)
            {
                yield return group.ExportMeshGroup(materials, data, option);
            }
        }

        public static IEnumerable<(glTFNode, glTFSkin)> ExportNodes(INativeArrayManager arrayManager, List<Node> nodes, List<MeshGroup> groups, ExportingGltfData data, ExportArgs option)
        {
            foreach (var node in nodes)
            {
                var gltfNode = new glTFNode
                {
                    name = node.Name,
                };
                glTFSkin gltfSkin = default;

                gltfNode.translation = node.LocalTranslation.ToFloat3();
                gltfNode.rotation = node.LocalRotation.ToFloat4();
                gltfNode.scale = node.LocalScaling.ToFloat3();

                if (node.MeshGroup != null)
                {
                    gltfNode.mesh = groups.IndexOfThrow(node.MeshGroup);
                    var skin = node.MeshGroup.Skin;
                    if (skin != null)
                    {
                        gltfSkin = new glTFSkin()
                        {
                            joints = skin.Joints.Select(joint =>
                            {
                                var index = nodes.IndexOf(joint);
                                if (index < 0)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return index;
                                }
                            }).ToArray()
                        };
                        if (skin.InverseMatrices == null)
                        {
                            skin.CalcInverseMatrices(arrayManager);
                        }
                        if (skin.InverseMatrices != null)
                        {
                            gltfSkin.inverseBindMatrices = skin.InverseMatrices.AddAccessorTo(data, 0, option.sparse);
                        }
                        if (skin.Root != null)
                        {
                            var rootIndex = nodes.IndexOf(skin.Root);
                            if (rootIndex != -1)
                            {
                                gltfSkin.skeleton = rootIndex;
                            }
                        }
                    }
                }

                gltfNode.children = node.Children.Select(child => nodes.IndexOfThrow(child)).ToArray();

                yield return (gltfNode, gltfSkin);
            }
        }

        /// <summary>
        /// revere X
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        static float[] ReverseX(Vector3 v)
        {
            return new float[] { -v.x, v.y, v.z };
        }

        ///
        /// 必要な容量を計算
        /// (sparseは考慮してないので大きめ)
        static int CalcReserveBytes(Model model)
        {
            int reserveBytes = 0;
            // mesh
            foreach (var g in model.MeshGroups)
            {
                foreach (var mesh in g.Meshes)
                {
                    // 頂点バッファ
                    reserveBytes += mesh.IndexBuffer.ByteLength;
                    foreach (var kv in mesh.VertexBuffer)
                    {
                        reserveBytes += kv.Value.ByteLength;
                    }
                    // morph
                    foreach (var morph in mesh.MorphTargets)
                    {
                        foreach (var kv in morph.VertexBuffer)
                        {
                            reserveBytes += kv.Value.ByteLength;
                        }
                    }
                }
            }
            return reserveBytes;
        }

        static IEnumerable<glTFMaterial> ExportMaterials(Model model, IMaterialExporter materialExporter, ITextureExporter textureExporter, GltfExportSettings settings)
        {
            foreach (Material material in model.Materials)
            {
                yield return materialExporter.ExportMaterial(material, textureExporter, settings);
            }
        }

        public void Export(GameObject root, Model model, ModelExporter converter, ExportArgs option, VRM10ObjectMeta vrmMeta = null)
        {
            Storage.Gltf.asset = ExportAsset(model);

            Storage.Reserve(CalcReserveBytes(model));

            foreach (var material in ExportMaterials(model, m_materialExporter, m_textureExporter, m_settings))
            {
                Storage.Gltf.materials.Add(material);
            }

            foreach (var mesh in ExportMeshes(model.MeshGroups, model.Materials, Storage, option))
            {
                Storage.Gltf.meshes.Add(mesh);
            }

            using (var arrayManager = new NativeArrayManager())
            {
                foreach (var (node, skin) in ExportNodes(arrayManager, model.Nodes, model.MeshGroups, Storage, option))
                {
                    Storage.Gltf.nodes.Add(node);
                    if (skin != null)
                    {
                        var skinIndex = Storage.Gltf.skins.Count;
                        Storage.Gltf.skins.Add(skin);
                        node.skin = skinIndex;
                    }
                }
            }
            Storage.Gltf.scenes.Add(new gltfScene()
            {
                nodes = model.Root.Children.Select(child => model.Nodes.IndexOfThrow(child)).ToArray()
            });

            var (vrm, vrmSpringBone, thumbnailTextureIndex) = ExportVrm(root, model, converter, vrmMeta, Storage.Gltf.nodes, m_textureExporter);

            // Extension で Texture が増える場合があるので最後に呼ぶ
            var exportedTextures = m_textureExporter.Export();
            for (var exportedTextureIdx = 0; exportedTextureIdx < exportedTextures.Count; ++exportedTextureIdx)
            {
                var (unityTexture, texColorSpace) = exportedTextures[exportedTextureIdx];
                GltfTextureExporter.PushGltfTexture(Storage, unityTexture, texColorSpace, m_textureSerializer);
            }

            if (thumbnailTextureIndex.HasValue)
            {
                vrm.Meta.ThumbnailImage = Storage.Gltf.textures[thumbnailTextureIndex.Value].source;
            }

            UniGLTF.Extensions.VRMC_vrm.GltfSerializer.SerializeTo(ref Storage.Gltf.extensions, vrm);

            if (vrmSpringBone != null)
            {
                UniGLTF.Extensions.VRMC_springBone.GltfSerializer.SerializeTo(ref Storage.Gltf.extensions, vrmSpringBone);
            }

            // Fix Duplicated name
            gltfExporter.FixName(Storage.Gltf);
        }


        /// <summary>
        /// VRMコンポーネントのエクスポート
        /// </summary>
        /// <param name="vrm"></param>
        /// <param name="springBone"></param>
        /// <param name="constraint"></param>
        /// <param name="root"></param>
        /// <param name="model"></param>
        /// <param name="converter"></param>
        /// <param name="vrmObject"></param>
        /// <returns></returns>
        (UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm,
        UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone,
        int? thumbnailIndex) ExportVrm(GameObject root, Model model, ModelExporter converter,
        VRM10ObjectMeta vrmMeta, List<glTFNode> nodes, ITextureExporter textureExporter)
        {
            if (root == null)
            {
                throw new System.ArgumentNullException("root");
            }

            if (root.TryGetComponent<Vrm10Instance>(out var vrmController))
            {
                if (vrmMeta == null)
                {
                    if (vrmController.Vrm?.Meta == null)
                    {
                        throw new NullReferenceException("metaObject is null");
                    }
                    vrmMeta = vrmController.Vrm.Meta;
                }
            }

            var vrm = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm
            {
                SpecVersion = VRM_SPEC_VERSION,
                Humanoid = new UniGLTF.Extensions.VRMC_vrm.Humanoid
                {
                    HumanBones = new UniGLTF.Extensions.VRMC_vrm.HumanBones
                    {
                    },
                },
                Meta = new UniGLTF.Extensions.VRMC_vrm.Meta
                {
                    LicenseUrl = LICENSE_URL_JA,
                    AllowExcessivelySexualUsage = false,
                    AllowExcessivelyViolentUsage = false,
                    AllowPoliticalOrReligiousUsage = false,
                    AllowRedistribution = false,
                },
            };

            //
            // required
            //
            ExportHumanoid(vrm, model);
            var thumbnailTextureIndex = ExportMeta(vrm, vrmMeta, textureExporter);

            //
            // optional
            //
            UniGLTF.Extensions.VRMC_springBone.VRMC_springBone vrmSpringBone = default;
            if (vrmController != null)
            {
                ExportExpression(vrm, vrmController, model, converter);
                ExportLookAt(vrm, vrmController);
                ExportFirstPerson(vrm, vrmController, model, converter);

                vrmSpringBone = ExportSpringBone(vrmController, model, converter);
                ExportConstraints(vrmController, model, converter, nodes);
            }

            return (vrm, vrmSpringBone, thumbnailTextureIndex);
        }

        static UniGLTF.Extensions.VRMC_springBone.ColliderShape ExportShape(VRM10SpringBoneCollider z)
        {
            var shape = new UniGLTF.Extensions.VRMC_springBone.ColliderShape();
            switch (z.ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    {
                        shape.Sphere = new UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.Capsule:
                    {
                        shape.Capsule = new UniGLTF.Extensions.VRMC_springBone.ColliderShapeCapsule
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                            Tail = ReverseX(z.Tail),
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.Plane:
                    {
                        const float DISTANCE = 1000.0f;
                        shape.Sphere = new UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere
                        {
                            Radius = 1000.0f,
                            Offset = ReverseX(z.Offset - z.TailOrNormal.normalized * DISTANCE),
                        };
                        break;
                    }

                default:
                    {
                        // 既存実装で未知の collider が来た時に throw しているので
                        // 回避するために適当な Shpere を作る。
                        shape.Sphere = new UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere
                        {
                            Radius = 0,
                            Offset = new float[] { 0, -10000, 0 },
                        };
                        break;
                    }
            }
            return shape;
        }

        static UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShape ExportShapeExtended(VRM10SpringBoneCollider z)
        {
            var shape = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShape();
            switch (z.ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    {
                        shape.Sphere = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeSphere
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.Capsule:
                    {
                        shape.Capsule = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeCapsule
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                            Tail = ReverseX(z.Tail),
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.SphereInside:
                    {
                        shape.Sphere = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeSphere
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                            Inside = true,
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    {
                        shape.Capsule = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapeCapsule
                        {
                            Radius = z.Radius,
                            Offset = ReverseX(z.Offset),
                            Tail = ReverseX(z.Tail),
                            Inside = true,
                        };
                        break;
                    }

                case VRM10SpringBoneColliderTypes.Plane:
                    {
                        shape.Plane = new UniGLTF.Extensions.VRMC_springBone_extended_collider.ExtendedColliderShapePlane
                        {
                            Offset = ReverseX(z.Offset),
                            Normal = ReverseX(z.Normal),
                        };
                        break;
                    }
            }
            return shape;
        }

        static UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint ExportJoint(VRM10SpringBoneJoint y, Func<Transform, int?> getIndexFromTransform)
        {
            var nodeIndex = getIndexFromTransform(y.transform);
            if (!nodeIndex.HasValue)
            {
                return default;
            }
            var joint = new UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint
            {
                Node = nodeIndex,
                HitRadius = y.m_jointRadius,
                DragForce = y.m_dragForce,
                Stiffness = y.m_stiffnessForce,
                GravityDir = ReverseX(y.m_gravityDir),
                GravityPower = y.m_gravityPower,
            };
            return joint;
        }

        static UniGLTF.Extensions.VRMC_springBone.VRMC_springBone ExportSpringBone(Vrm10Instance controller, Model model, ModelExporter converter)
        {
            var colliders = controller.GetComponentsInChildren<VRM10SpringBoneCollider>();

            // if colliders, collider groups and springs don't exist, don't export the extension
            if (
                colliders.Length == 0 &&
                controller.SpringBone.ColliderGroups.Count == 0 &&
                controller.SpringBone.Springs.Count == 0
            )
            {
                return null;
            }

            var springBone = new UniGLTF.Extensions.VRMC_springBone.VRMC_springBone
            {
                SpecVersion = SPRINGBONE_SPEC_VERSION,
                Colliders = new List<UniGLTF.Extensions.VRMC_springBone.Collider>(),
                ColliderGroups = new List<UniGLTF.Extensions.VRMC_springBone.ColliderGroup>(),
                Springs = new List<UniGLTF.Extensions.VRMC_springBone.Spring>(),
            };

            // colliders
            Func<Transform, int?> getNodeIndexFromTransform = t =>
            {
                var node = converter.Nodes[t.gameObject];
                var nodeIndex = model.Nodes.IndexOf(node);
                if (nodeIndex == -1)
                {
                    return default;
                }
                return nodeIndex;
            };

            foreach (var c in colliders)
            {
                var nodeIndex = getNodeIndexFromTransform(c.transform);
                if (!nodeIndex.HasValidIndex(model.Nodes.Count))
                {
                    continue;
                }
                var exportCollider = new UniGLTF.Extensions.VRMC_springBone.Collider
                {
                    Node = nodeIndex.Value,
                    Shape = ExportShape(c),
                };

                if (c.ColliderType == VRM10SpringBoneColliderTypes.SphereInside
                || c.ColliderType == VRM10SpringBoneColliderTypes.CapsuleInside
                || c.ColliderType == VRM10SpringBoneColliderTypes.Plane
                )
                {
                    var extendedCollider = new UniGLTF.Extensions.VRMC_springBone_extended_collider.VRMC_springBone_extended_collider
                    {
                        SpecVersion = SPRINGBONE_EXTENDED_COLLIDER_SPEC_VERSION,
                        Shape = ExportShapeExtended(c),
                    };
                    glTFExtension extensions = default;
                    UniGLTF.Extensions.VRMC_springBone_extended_collider.GltfSerializer.SerializeTo(ref extensions, extendedCollider);
                    exportCollider.Extensions = extensions;
                }
                springBone.Colliders.Add(exportCollider);
            }

            // colliderGroups
            foreach (var x in controller.SpringBone.ColliderGroups)
            {
                springBone.ColliderGroups.Add(new UniGLTF.Extensions.VRMC_springBone.ColliderGroup
                {
                    Name = x.Name,
                    Colliders = x.Colliders
                        .Select(y => Array.IndexOf(colliders, y))
                        .Where(y => y != -1)
                        .ToArray(),
                });
            }

            // springs
            foreach (var runtimeSpring in controller.SpringBone.Springs)
            {
                var vrmSpring = new UniGLTF.Extensions.VRMC_springBone.Spring
                {
                    Name = runtimeSpring.Name,
                    Joints = runtimeSpring.Joints
                        .Select(y => ExportJoint(y, getNodeIndexFromTransform))
                        .Where(y => y != null)
                        .ToList(),
                    ColliderGroups = runtimeSpring.ColliderGroups
                    .Select(y => controller.SpringBone.ColliderGroups.IndexOf(y))
                    .Where(y => y != -1)
                    .ToArray(),
                };

                if (runtimeSpring.Center != null)
                {
                    var center = model.Nodes.IndexOf(converter.Nodes[runtimeSpring.Center.gameObject]);
                    if (center != -1)
                    {
                        vrmSpring.Center = center;
                    }
                }

                springBone.Springs.Add(vrmSpring);
            }

            return springBone;
        }

        static void ExportConstraints(Vrm10Instance vrmController, Model model, ModelExporter converter, List<glTFNode> nodes)
        {
            var constraints = vrmController.GetComponentsInChildren<IVrm10Constraint>();
            foreach (var constraint in constraints)
            {
                var vrmConstraint = new UniGLTF.Extensions.VRMC_node_constraint.VRMC_node_constraint
                {
                    SpecVersion = NODE_CONSTRAINT_SPEC_VERSION,
                    Constraint = new UniGLTF.Extensions.VRMC_node_constraint.Constraint
                    {
                    },
                };

                switch (constraint)
                {
                    case Vrm10AimConstraint aimConstraint:
                        vrmConstraint.Constraint.Aim = new UniGLTF.Extensions.VRMC_node_constraint.AimConstraint
                        {
                            Source = model.Nodes.IndexOf(converter.Nodes[aimConstraint.Source.gameObject]),
                            Weight = aimConstraint.Weight,
                            AimAxis = Vrm10ConstraintUtil.ReverseX(aimConstraint.AimAxis),
                        };
                        break;

                    case Vrm10RollConstraint rollConstraint:
                        vrmConstraint.Constraint.Roll = new UniGLTF.Extensions.VRMC_node_constraint.RollConstraint
                        {
                            Source = model.Nodes.IndexOf(converter.Nodes[rollConstraint.Source.gameObject]),
                            Weight = rollConstraint.Weight,
                            RollAxis = rollConstraint.RollAxis,
                        };
                        break;

                    case Vrm10RotationConstraint rotationConstraint:
                        vrmConstraint.Constraint.Rotation = new UniGLTF.Extensions.VRMC_node_constraint.RotationConstraint
                        {
                            Source = model.Nodes.IndexOf(converter.Nodes[rotationConstraint.Source.gameObject]),
                            Weight = rotationConstraint.Weight,
                        };
                        break;

                    default:
                        throw new NotImplementedException();
                }

                // serialize to gltfNode
                var node = converter.Nodes[constraint.ConstraintTarget];
                var nodeIndex = model.Nodes.IndexOf(node);
                var gltfNode = nodes[nodeIndex];
                UniGLTF.Extensions.VRMC_node_constraint.GltfSerializer.SerializeTo(ref gltfNode.extensions, vrmConstraint);
            }
        }

        static bool[] ToArray(AxisMask mask)
        {
            return new bool[]
            {
                mask.HasFlag(AxisMask.X),
                mask.HasFlag(AxisMask.Y),
                mask.HasFlag(AxisMask.Z),
            };
        }


        static UniGLTF.Extensions.VRMC_vrm.MeshAnnotation ExportMeshAnnotation(RendererFirstPersonFlags flags, Transform root, Func<Renderer, int> getIndex)
        {
            return new UniGLTF.Extensions.VRMC_vrm.MeshAnnotation
            {
                Node = getIndex(flags.GetRenderer(root)),
                Type = flags.FirstPersonFlag,
            };
        }

        void ExportFirstPerson(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, Vrm10Instance vrmController, Model model, ModelExporter converter)
        {
            if (!(vrmController?.Vrm?.FirstPerson is VRM10ObjectFirstPerson firstPerson))
            {
                return;
            }

            vrm.FirstPerson = new UniGLTF.Extensions.VRMC_vrm.FirstPerson
            {
                MeshAnnotations = new List<UniGLTF.Extensions.VRMC_vrm.MeshAnnotation>(),
            };
            Func<Renderer, int> getIndex = r =>
            {
                var node = converter.Nodes[r.gameObject];
                return model.Nodes.IndexOf(node);
            };
            foreach (var f in firstPerson.Renderers)
            {
                vrm.FirstPerson.MeshAnnotations.Add(ExportMeshAnnotation(f, vrmController.transform, getIndex));
            }
        }

        static UniGLTF.Extensions.VRMC_vrm.LookAtRangeMap ExportLookAtRangeMap(CurveMapper mapper)
        {
            return new UniGLTF.Extensions.VRMC_vrm.LookAtRangeMap
            {
                InputMaxValue = mapper.CurveXRangeDegree,
                OutputScale = mapper.CurveYRangeDegree,
            };
        }

        static void ExportLookAt(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, Vrm10Instance vrmController)
        {
            if (!(vrmController?.Vrm?.LookAt is VRM10ObjectLookAt lookAt))
            {
                return;
            }

            vrm.LookAt = new UniGLTF.Extensions.VRMC_vrm.LookAt
            {
                Type = lookAt.LookAtType,
                OffsetFromHeadBone = lookAt.OffsetFromHead.ReverseX().ToFloat3(),
                RangeMapHorizontalInner = ExportLookAtRangeMap(lookAt.HorizontalInner),
                RangeMapHorizontalOuter = ExportLookAtRangeMap(lookAt.HorizontalOuter),
                RangeMapVerticalDown = ExportLookAtRangeMap(lookAt.VerticalDown),
                RangeMapVerticalUp = ExportLookAtRangeMap(lookAt.VerticalUp),
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.MorphTargetBind ExportMorphTargetBinding(MorphTargetBinding binding, Func<string, int> getIndex)
        {
            return new UniGLTF.Extensions.VRMC_vrm.MorphTargetBind
            {
                Node = getIndex(binding.RelativePath),
                Index = binding.Index,
                Weight = binding.Weight,
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.MaterialColorBind ExportMaterialColorBinding(MaterialColorBinding binding, Func<string, int> getIndex)
        {
            return new UniGLTF.Extensions.VRMC_vrm.MaterialColorBind
            {
                Material = getIndex(binding.MaterialName),
                Type = binding.BindType,
                TargetValue = new float[] { binding.TargetValue.x, binding.TargetValue.y, binding.TargetValue.z, binding.TargetValue.w },
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.TextureTransformBind ExportTextureTransformBinding(MaterialUVBinding binding, Func<string, int> getIndex)
        {
            var (scale, offset) = TextureTransform.VerticalFlipScaleOffset(binding.Scaling, binding.Offset);
            return new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
            {
                Material = getIndex(binding.MaterialName),
                Offset = new float[] { offset.x, offset.y },
                Scale = new float[] { scale.x, scale.y },
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.Expression ExportExpression(VRM10Expression e, Vrm10Instance vrmController, Model model, ModelExporter converter)
        {
            if (e == null)
            {
                return null;
            }

            Func<string, int> getIndexFromRelativePath = relativePath =>
            {
                var rendererNode = vrmController.transform.GetFromPath(relativePath);
                var node = converter.Nodes[rendererNode.gameObject];
                return model.Nodes.IndexOf(node);
            };

            var vrmExpression = new UniGLTF.Extensions.VRMC_vrm.Expression
            {
                // Preset = e.Preset,
                // Name = e.ExpressionName,
                IsBinary = e.IsBinary,
                OverrideBlink = e.OverrideBlink,
                OverrideLookAt = e.OverrideLookAt,
                OverrideMouth = e.OverrideMouth,
                MorphTargetBinds = new List<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind>(),
                MaterialColorBinds = new List<UniGLTF.Extensions.VRMC_vrm.MaterialColorBind>(),
                TextureTransformBinds = new List<UniGLTF.Extensions.VRMC_vrm.TextureTransformBind>(),
            };
            Func<string, int> getIndexFromMaterialName = materialName =>
            {
                for (int i = 0; i < model.Materials.Count; ++i)
                {
                    var m = model.Materials[i] as Material;
                    if (m.name == materialName)
                    {
                        return i;
                    }
                }
                throw new KeyNotFoundException();
            };

            foreach (var b in e.MorphTargetBindings)
            {
                try
                {
                    vrmExpression.MorphTargetBinds.Add(ExportMorphTargetBinding(b, getIndexFromRelativePath));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
            foreach (var b in e.MaterialColorBindings)
            {
                try
                {
                    vrmExpression.MaterialColorBinds.Add(ExportMaterialColorBinding(b, getIndexFromMaterialName));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
            foreach (var b in e.MaterialUVBindings)
            {
                try
                {
                    vrmExpression.TextureTransformBinds.Add(ExportTextureTransformBinding(b, getIndexFromMaterialName));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
            return vrmExpression;
        }

        static void ExportExpression(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, Vrm10Instance vrmController, Model model, ModelExporter converter)
        {
            if (vrmController?.Vrm?.Expression?.Clips == null)
            {
                return;
            }

            vrm.Expressions = new UniGLTF.Extensions.VRMC_vrm.Expressions
            {
                Preset = new UniGLTF.Extensions.VRMC_vrm.Preset
                {
                    Happy = ExportExpression(vrmController.Vrm.Expression.Happy, vrmController, model, converter),
                    Angry = ExportExpression(vrmController.Vrm.Expression.Angry, vrmController, model, converter),
                    Sad = ExportExpression(vrmController.Vrm.Expression.Sad, vrmController, model, converter),
                    Relaxed = ExportExpression(vrmController.Vrm.Expression.Relaxed, vrmController, model, converter),
                    Surprised = ExportExpression(vrmController.Vrm.Expression.Surprised, vrmController, model, converter),
                    Aa = ExportExpression(vrmController.Vrm.Expression.Aa, vrmController, model, converter),
                    Ih = ExportExpression(vrmController.Vrm.Expression.Ih, vrmController, model, converter),
                    Ou = ExportExpression(vrmController.Vrm.Expression.Ou, vrmController, model, converter),
                    Ee = ExportExpression(vrmController.Vrm.Expression.Ee, vrmController, model, converter),
                    Oh = ExportExpression(vrmController.Vrm.Expression.Oh, vrmController, model, converter),
                    Blink = ExportExpression(vrmController.Vrm.Expression.Blink, vrmController, model, converter),
                    BlinkLeft = ExportExpression(vrmController.Vrm.Expression.BlinkLeft, vrmController, model, converter),
                    BlinkRight = ExportExpression(vrmController.Vrm.Expression.BlinkRight, vrmController, model, converter),
                    LookUp = ExportExpression(vrmController.Vrm.Expression.LookUp, vrmController, model, converter),
                    LookDown = ExportExpression(vrmController.Vrm.Expression.LookDown, vrmController, model, converter),
                    LookLeft = ExportExpression(vrmController.Vrm.Expression.LookLeft, vrmController, model, converter),
                    LookRight = ExportExpression(vrmController.Vrm.Expression.LookRight, vrmController, model, converter),
                    Neutral = ExportExpression(vrmController.Vrm.Expression.Neutral, vrmController, model, converter),
                },
                Custom = vrmController.Vrm.Expression.CustomClips.ToDictionary(c => c.name, c => ExportExpression(c, vrmController, model, converter)),
            };
        }

        public static int? ExportMeta(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, VRM10ObjectMeta meta, ITextureExporter textureExporter)
        {
            vrm.Meta.Name = meta.Name;
            vrm.Meta.Version = meta.Version;
            vrm.Meta.Authors = meta.Authors.ToList();
            vrm.Meta.CopyrightInformation = meta.CopyrightInformation;
            vrm.Meta.ContactInformation = meta.ContactInformation;
            vrm.Meta.References = meta.References.ToList();
            vrm.Meta.ThirdPartyLicenses = meta.ThirdPartyLicenses;
            vrm.Meta.AvatarPermission = meta.AvatarPermission;
            vrm.Meta.AllowExcessivelyViolentUsage = meta.ViolentUsage;
            vrm.Meta.AllowExcessivelySexualUsage = meta.SexualUsage;
            vrm.Meta.CommercialUsage = meta.CommercialUsage;
            vrm.Meta.AllowPoliticalOrReligiousUsage = meta.PoliticalOrReligiousUsage;
            vrm.Meta.AllowAntisocialOrHateUsage = meta.AntisocialOrHateUsage;
            vrm.Meta.CreditNotation = meta.CreditNotation;
            vrm.Meta.AllowRedistribution = meta.Redistribution;
            vrm.Meta.Modification = meta.Modification;
            vrm.Meta.OtherLicenseUrl = meta.OtherLicenseUrl;
            int? thumbnailTextureIndex = default;
            if (meta.Thumbnail != null)
            {
                thumbnailTextureIndex = textureExporter.RegisterExportingAsSRgb(meta.Thumbnail, needsAlpha: true);
            }
            return thumbnailTextureIndex;
        }

        static void ExportHumanoid(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, Model model)
        {
            // humanoid
            for (int i = 0; i < model.Nodes.Count; ++i)
            {
                var bone = model.Nodes[i];
                switch (bone.HumanoidBone)
                {
                    case HumanoidBones.hips: vrm.Humanoid.HumanBones.Hips = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.spine: vrm.Humanoid.HumanBones.Spine = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.chest: vrm.Humanoid.HumanBones.Chest = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.upperChest: vrm.Humanoid.HumanBones.UpperChest = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.neck: vrm.Humanoid.HumanBones.Neck = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.head: vrm.Humanoid.HumanBones.Head = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftEye: vrm.Humanoid.HumanBones.LeftEye = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightEye: vrm.Humanoid.HumanBones.RightEye = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.jaw: vrm.Humanoid.HumanBones.Jaw = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftUpperLeg: vrm.Humanoid.HumanBones.LeftUpperLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftLowerLeg: vrm.Humanoid.HumanBones.LeftLowerLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftFoot: vrm.Humanoid.HumanBones.LeftFoot = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftToes: vrm.Humanoid.HumanBones.LeftToes = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightUpperLeg: vrm.Humanoid.HumanBones.RightUpperLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightLowerLeg: vrm.Humanoid.HumanBones.RightLowerLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightFoot: vrm.Humanoid.HumanBones.RightFoot = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightToes: vrm.Humanoid.HumanBones.RightToes = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftShoulder: vrm.Humanoid.HumanBones.LeftShoulder = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftUpperArm: vrm.Humanoid.HumanBones.LeftUpperArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftLowerArm: vrm.Humanoid.HumanBones.LeftLowerArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftHand: vrm.Humanoid.HumanBones.LeftHand = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightShoulder: vrm.Humanoid.HumanBones.RightShoulder = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightUpperArm: vrm.Humanoid.HumanBones.RightUpperArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightLowerArm: vrm.Humanoid.HumanBones.RightLowerArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightHand: vrm.Humanoid.HumanBones.RightHand = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftThumbMetacarpal: vrm.Humanoid.HumanBones.LeftThumbMetacarpal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftThumbProximal: vrm.Humanoid.HumanBones.LeftThumbProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftThumbDistal: vrm.Humanoid.HumanBones.LeftThumbDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftIndexProximal: vrm.Humanoid.HumanBones.LeftIndexProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftIndexIntermediate: vrm.Humanoid.HumanBones.LeftIndexIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftIndexDistal: vrm.Humanoid.HumanBones.LeftIndexDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftMiddleProximal: vrm.Humanoid.HumanBones.LeftMiddleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftMiddleIntermediate: vrm.Humanoid.HumanBones.LeftMiddleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftMiddleDistal: vrm.Humanoid.HumanBones.LeftMiddleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftRingProximal: vrm.Humanoid.HumanBones.LeftRingProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftRingIntermediate: vrm.Humanoid.HumanBones.LeftRingIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftRingDistal: vrm.Humanoid.HumanBones.LeftRingDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftLittleProximal: vrm.Humanoid.HumanBones.LeftLittleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftLittleIntermediate: vrm.Humanoid.HumanBones.LeftLittleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.leftLittleDistal: vrm.Humanoid.HumanBones.LeftLittleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightThumbMetacarpal: vrm.Humanoid.HumanBones.RightThumbMetacarpal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightThumbProximal: vrm.Humanoid.HumanBones.RightThumbProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightThumbDistal: vrm.Humanoid.HumanBones.RightThumbDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightIndexProximal: vrm.Humanoid.HumanBones.RightIndexProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightIndexIntermediate: vrm.Humanoid.HumanBones.RightIndexIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightIndexDistal: vrm.Humanoid.HumanBones.RightIndexDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightMiddleProximal: vrm.Humanoid.HumanBones.RightMiddleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightMiddleIntermediate: vrm.Humanoid.HumanBones.RightMiddleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightMiddleDistal: vrm.Humanoid.HumanBones.RightMiddleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightRingProximal: vrm.Humanoid.HumanBones.RightRingProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightRingIntermediate: vrm.Humanoid.HumanBones.RightRingIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightRingDistal: vrm.Humanoid.HumanBones.RightRingDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightLittleProximal: vrm.Humanoid.HumanBones.RightLittleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightLittleIntermediate: vrm.Humanoid.HumanBones.RightLittleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.rightLittleDistal: vrm.Humanoid.HumanBones.RightLittleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                }
            }
        }

        /// <summary>
        /// 便利関数
        /// </summary>
        /// <param name="go"></param>
        /// <param name="getTextureBytes"></param>
        /// <returns></returns>
        public static byte[] Export(
            GameObject go,
            IMaterialExporter materialExporter = null,
            ITextureSerializer textureSerializer = null,
            VRM10ObjectMeta vrmMeta = null)
        {
            using (var arrayManager = new NativeArrayManager())
            {
                // ヒエラルキーからジオメトリーを収集
                var converter = new UniVRM10.ModelExporter();
                var model = converter.Export(arrayManager, go);

                // 右手系に変換
                model.ConvertCoordinate(VrmLib.Coordinates.Vrm1);

                // Model と go から VRM-1.0 にExport
                var exporter10 = new Vrm10Exporter(new GltfExportSettings(), materialExporter, textureSerializer);
                var option = new VrmLib.ExportArgs
                {
                };
                exporter10.Export(go, model, converter, option, vrmMeta);
                return exporter10.Storage.ToGlbBytes();
            }
        }
    }
}
