using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeshUtility;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public delegate VrmLib.TextureInfo GetOrCreateTextureDelegate(UnityEngine.Material material, UnityEngine.Texture srcTexture, VrmLib.Texture.ColorSpaceTypes colorSpace, VrmLib.Texture.TextureTypes textureType);
    public class RuntimeVrmConverter
    {
        public VrmLib.Model Model;

        public Dictionary<GameObject, VrmLib.Node> Nodes = new Dictionary<GameObject, VrmLib.Node>();
        public Dictionary<Texture2D, VrmLib.TextureInfo> Textures = new Dictionary<Texture2D, VrmLib.TextureInfo>();
        public Dictionary<UnityEngine.Material, VrmLib.Material> Materials = new Dictionary<UnityEngine.Material, VrmLib.Material>();
        public Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup> Meshes = new Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup>();

        static string GetSupportedMime(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
            }

            // .tga etc
            return null;
        }

        /// <summary>
        /// return (bytes, mime string)
        /// </summary>
        static (byte[], string) GetImageEncodedBytes(UnityEngine.Texture src, RenderTextureReadWrite renderTextureReadWrite, UnityEngine.Material renderMaterial = null)
        {
#if false
            /// 元になるアセットがあればそれを得る(png, jpgのみ)
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(src);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var mime = GetSupportedMime(assetPath);
                if (!string.IsNullOrEmpty(mime))
                {
                    return (File.ReadAllBytes(assetPath), GetSupportedMime(assetPath));
                }
            }
#endif

            var copy = UnityTextureUtil.CopyTexture(src, renderTextureReadWrite, renderMaterial);
            return (copy.EncodeToPNG(), "image/png");
        }

        public VrmLib.TextureInfo GetOrCreateTexture(UnityEngine.Material material, UnityEngine.Texture srcTexture, VrmLib.Texture.ColorSpaceTypes colorSpace, VrmLib.Texture.TextureTypes textureType)
        {
            var texture = srcTexture as Texture2D;
            if (texture is null)
            {
                return null;
            }

            if (!Textures.TryGetValue(texture, out VrmLib.TextureInfo info))
            {
                UnityEngine.Material converter = null;
                if (textureType == VrmLib.Texture.TextureTypes.NormalMap)
                {
                    converter = TextureConvertMaterial.GetNormalMapConvertUnityToGltf();
                }
                else if (textureType == VrmLib.Texture.TextureTypes.MetallicRoughness)
                {
                    float smoothness = 0.0f;
                    if (material.HasProperty("_GlossMapScale"))
                    {
                        smoothness = material.GetFloat("_GlossMapScale");
                    }

                    converter = TextureConvertMaterial.GetMetallicRoughnessUnityToGltf(smoothness);
                }
                else if (textureType == VrmLib.Texture.TextureTypes.Occlusion)
                {
                    converter = TextureConvertMaterial.GetOcclusionUnityToGltf();
                }

                var (bytes, mime) = GetImageEncodedBytes(
                    texture,
                    (colorSpace == VrmLib.Texture.ColorSpaceTypes.Linear) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB,
                    converter
                    );

                if (converter != null)
                {
                    UnityEngine.Object.DestroyImmediate(converter);
                }

                var sampler = new VrmLib.TextureSampler
                {
                    MagFilter = texture.filterMode.ToVrmLibMagFilter(),
                    MinFilter = texture.filterMode.ToVrmLibMinFilter(),
                    WrapS = texture.wrapMode.ToVrmLib(),
                    WrapT = texture.wrapMode.ToVrmLib(),
                };
                var image = new VrmLib.Image(texture.name, mime, VrmLib.ImageUsage.None, new ArraySegment<byte>(bytes));
                info = new VrmLib.TextureInfo(new VrmLib.ImageTexture(texture.name, sampler, image, colorSpace, textureType));
                Textures.Add(texture, info);

                if (Model != null)
                {
                    Model.Images.Add(image);
                    Model.Textures.Add(info.Texture);
                }
            }

            return info;
        }

        #region Export 1.0
        /// <summary>
        /// VRM-0.X の MaterialBindValue を VRM-1.0 仕様に変換する
        ///
        /// * Property名 => enum MaterialBindType
        /// * 特に _MainTex_ST の場合、MaterialBindType.UvScale + MaterialBindType.UvScale ２つになりうる
        ///
        /// </summary>
        VrmLib.Expression ToVrmLib(VRM10Expression clip, GameObject root)
        {
            var expression = new VrmLib.Expression(clip.Preset, clip.ExpressionName, clip.IsBinary);
            expression.IgnoreBlink = clip.IgnoreBlink;
            expression.IgnoreLookAt = clip.IgnoreLookAt;
            expression.IgnoreMouth = clip.IgnoreMouth;

            foreach (var binding in clip.MorphTargetBindings)
            {
                var transform = GetTransformFromRelativePath(root.transform, binding.RelativePath);
                if (transform == null)
                    continue;
                var renderer = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (renderer == null)
                    continue;
                var mesh = renderer.sharedMesh;
                if (mesh == null)
                    continue;

                var names = new List<string>();
                for (int i = 0; i < mesh.blendShapeCount; ++i)
                {
                    names.Add(mesh.GetBlendShapeName(i));
                }

                var node = Nodes[transform.gameObject];
                var blendShapeValue = new VrmLib.MorphTargetBind(
                    node,
                    names[binding.Index],
                    binding.Weight
                    );
                expression.MorphTargetBinds.Add(blendShapeValue);
            }

            foreach (var binding in clip.MaterialColorBindings)
            {
                var materialPair = Materials.FirstOrDefault(x => x.Key.name == binding.MaterialName);
                if (materialPair.Value != null)
                {
                    var bind = new VrmLib.MaterialColorBind(
                        materialPair.Value,
                        binding.BindType,
                        binding.TargetValue.ToNumericsVector4()
                        );
                    expression.MaterialColorBinds.Add(bind);
                }
            }

            foreach (var binding in clip.MaterialUVBindings)
            {
                var materialPair = Materials.FirstOrDefault(x => x.Key.name == binding.MaterialName);
                if (materialPair.Value != null)
                {
                    var bind = new VrmLib.TextureTransformBind(
                        materialPair.Value,
                        binding.Scaling.ToNumericsVector2(),
                        binding.Offset.ToNumericsVector2()
                    );
                    expression.TextureTransformBinds.Add(bind);
                }
            }

            return expression;
        }

        /// <summary>
        /// metaObject が null のときは、root から取得する
        /// </summary>
        public VrmLib.Model ToModelFrom10(GameObject root, VRM10MetaObject metaObject = null)
        {
            Model = new VrmLib.Model(VrmLib.Coordinates.Unity);

            if (metaObject is null)
            {
                var vrmController = root.GetComponent<VRM10Controller>();
                if (vrmController is null || vrmController.Meta is null)
                {
                    throw new NullReferenceException("metaObject is null");
                }
                metaObject = vrmController.Meta;
            }

            ToGlbModel(root);

            // meta
            var meta = new VrmLib.Meta();
            meta.Name = metaObject.Name;
            meta.Version = metaObject.Version;
            meta.CopyrightInformation = metaObject.CopyrightInformation;
            meta.Authors.AddRange(metaObject.Authors);
            meta.ContactInformation = metaObject.ContactInformation;
            meta.Reference = metaObject.Reference;
            meta.Thumbnail = metaObject.Thumbnail.ToPngImage(VrmLib.ImageUsage.None);

            meta.AvatarPermission = new VrmLib.AvatarPermission
            {
                AvatarUsage = metaObject.AllowedUser,
                IsAllowedViolentUsage = metaObject.ViolentUsage,
                IsAllowedSexualUsage = metaObject.SexualUsage,
                CommercialUsage = metaObject.CommercialUsage,
                IsAllowedGameUsage = metaObject.GameUsage,
                IsAllowedPoliticalOrReligiousUsage = metaObject.PoliticalOrReligiousUsage,
                OtherPermissionUrl = metaObject.OtherPermissionUrl,
            };
            meta.RedistributionLicense = new VrmLib.RedistributionLicense
            {
                CreditNotation = metaObject.CreditNotation,
                IsAllowRedistribution = metaObject.Redistribution,
                ModificationLicense = metaObject.ModificationLicense,
                OtherLicenseUrl = metaObject.OtherLicenseUrl,
            };
            Model.Vrm = new VrmLib.Vrm(meta, UniVRM10.VRMVersion.VERSION, UniVRM10.VRMSpecVersion.Version);

            // humanoid
            {
                var humanoid = root.GetComponent<MeshUtility.Humanoid>();
                if (humanoid is null)
                {
                    humanoid = root.AddComponent<MeshUtility.Humanoid>();
                    humanoid.AssignBonesFromAnimator();
                }

                foreach (HumanBodyBones humanBoneType in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    var transform = humanoid.GetBoneTransform(humanBoneType);
                    if (transform != null && Nodes.TryGetValue(transform.gameObject, out VrmLib.Node node))
                    {
                        node.HumanoidBone = (VrmLib.HumanoidBones)Enum.Parse(typeof(VrmLib.HumanoidBones), humanBoneType.ToString(), true);
                    }
                }
            }


            // blendShape
            var controller = root.GetComponent<UniVRM10.VRM10Controller>();
            if (controller != null)
            {
                {
                    Model.Vrm.ExpressionManager = new VrmLib.ExpressionManager();
                    if (controller != null)
                    {
                        foreach (var clip in controller.Expression.ExpressionAvatar.Clips)
                        {
                            var expression = ToVrmLib(clip, root);
                            if (expression != null)
                            {
                                Model.Vrm.ExpressionManager.ExpressionList.Add(expression);
                            }
                        }
                    }
                }

                // firstPerson
                {
                    var firstPerson = new VrmLib.FirstPerson();
                    if (controller != null)
                    {
                        foreach (var annotation in controller.FirstPerson.Renderers)
                        {
                            firstPerson.Annotations.Add(
                                new VrmLib.FirstPersonMeshAnnotation(Nodes[annotation.Renderer.gameObject],
                                annotation.FirstPersonFlag)
                                );
                        }
                        Model.Vrm.FirstPerson = firstPerson;
                    }
                }

                // lookAt
                {
                    var lookAt = new VrmLib.LookAt();
                    if (controller != null)
                    {
                        if (controller.LookAt.LookAtType == VRM10ControllerLookAt.LookAtTypes.Expression)
                        {
                            lookAt.HorizontalInner = new VrmLib.LookAtRangeMap();
                            lookAt.HorizontalOuter = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.HorizontalOuter.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.HorizontalOuter.CurveYRangeDegree
                            };
                            lookAt.VerticalUp = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.VerticalUp.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.VerticalUp.CurveYRangeDegree,
                            };
                            lookAt.VerticalDown = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.VerticalDown.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.VerticalDown.CurveYRangeDegree,
                            };
                        }
                        else if (controller.LookAt.LookAtType == VRM10ControllerLookAt.LookAtTypes.Bone)
                        {
                            lookAt.HorizontalInner = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.HorizontalInner.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.HorizontalInner.CurveYRangeDegree
                            };
                            lookAt.HorizontalOuter = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.HorizontalOuter.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.HorizontalOuter.CurveYRangeDegree
                            };
                            lookAt.VerticalUp = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.VerticalUp.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.VerticalUp.CurveYRangeDegree,
                            };
                            lookAt.VerticalDown = new VrmLib.LookAtRangeMap()
                            {
                                InputMaxValue = controller.LookAt.VerticalDown.CurveXRangeDegree,
                                OutputScaling = controller.LookAt.VerticalDown.CurveYRangeDegree,
                            };
                        }
                        lookAt.OffsetFromHeadBone = controller.LookAt.OffsetFromHead.ToNumericsVector3();
                    }
                    Model.Vrm.LookAt = lookAt;
                }

                // springBone
                {
                    var springBoneManager = controller.SpringBone;
                    foreach (var springBone in springBoneManager.Springs)
                    {
                        var vrmSpringBone = new VrmLib.SpringBone()
                        {
                            Comment = springBone.m_comment,
                            Origin = (springBone.m_center != null) ? Nodes[springBone.m_center.gameObject] : null,
                        };

                        foreach (var joint in springBone.Joints)
                        {
                            vrmSpringBone.Joints.Add(new VrmLib.SpringJoint(Nodes[joint.Transform.gameObject])
                            {
                                Stiffness = joint.m_stiffnessForce,
                                GravityPower = joint.m_gravityPower,
                                GravityDir = joint.m_gravityDir.ToNumericsVector3(),
                                DragForce = joint.m_dragForce,
                                HitRadius = joint.m_jointRadius,
                                Exclude = joint.m_exclude,
                            });
                        }

                        foreach (var colliderGroup in springBone.ColliderGroups)
                        {
                            var colliderGroups = colliderGroup.Colliders.Select(x =>
                            {
                                switch (x.ColliderType)
                                {
                                    case VRM10SpringBoneColliderTypes.Sphere:
                                        return VrmLib.VrmSpringBoneCollider.CreateSphere(x.Offset.ToNumericsVector3(), x.Radius);

                                    case VRM10SpringBoneColliderTypes.Capsule:
                                        return VrmLib.VrmSpringBoneCollider.CreateCapsule(x.Offset.ToNumericsVector3(), x.Radius, x.Tail.ToNumericsVector3());

                                    default:
                                        throw new NotImplementedException();
                                }
                            });
                            var vrmColliderGroup = new VrmLib.SpringBoneColliderGroup(Nodes[colliderGroup.gameObject], colliderGroups);
                            vrmSpringBone.Colliders.Add(vrmColliderGroup);
                        }

                        Model.Vrm.SpringBone.Springs.Add(vrmSpringBone);
                    }
                }
            }

            return Model;
        }

        public VrmLib.Model ToGlbModel(GameObject root)
        {
            if (Model == null)
            {
                Model = new VrmLib.Model(VrmLib.Coordinates.Unity);
            }

            // node
            {
                Model.Root.Name = root.name;
                CreateNodes(root.transform, Model.Root, Nodes);
                Model.Nodes = Nodes
                .Where(x => x.Value != Model.Root)
                            .Select(x => x.Value).ToList();
            }

            // material and textures
            var rendererComponents = root.GetComponentsInChildren<Renderer>();
            {
                foreach (var renderer in rendererComponents)
                {
                    var materials = renderer.sharedMaterials; // avoid copy
                    foreach (var material in materials)
                    {
                        if (Materials.ContainsKey(material))
                        {
                            continue;
                        }

                        var vrmMaterial = Export10(material, GetOrCreateTexture);
                        Model.Materials.Add(vrmMaterial);
                        Materials.Add(material, vrmMaterial);
                    }
                }
            }

            // mesh
            {
                foreach (var renderer in rendererComponents)
                {
                    if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        if (skinnedMeshRenderer.sharedMesh != null)
                        {
                            var mesh = CreateMesh(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer, Materials);
                            var skin = CreateSkin(skinnedMeshRenderer, Nodes, root);
                            if (skin != null)
                            {
                                // blendshape only で skinning が無いやつがある
                                mesh.Skin = skin;
                                Model.Skins.Add(mesh.Skin);
                            }
                            Model.MeshGroups.Add(mesh);
                            Nodes[renderer.gameObject].MeshGroup = mesh;
                            Meshes.Add(skinnedMeshRenderer.sharedMesh, mesh);
                        }
                    }
                    else if (renderer is MeshRenderer meshRenderer)
                    {
                        var filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                        if (filter != null && filter.sharedMesh != null)
                        {
                            var mesh = CreateMesh(filter.sharedMesh, meshRenderer, Materials);
                            Model.MeshGroups.Add(mesh);
                            Nodes[renderer.gameObject].MeshGroup = mesh;
                            Meshes.Add(filter.sharedMesh, mesh);
                        }
                    }
                }
            }

            return Model;
        }
        #endregion

        public VrmLib.Material Export10(UnityEngine.Material src, GetOrCreateTextureDelegate map)
        {
            switch (src.shader.name)
            {
                case "VRM/MToon":
                    {
                        var def = MToon.Utils.GetMToonParametersFromMaterial(src);
                        return new VrmLib.MToonMaterial(src.name)
                        {
                            Definition = def.ToVrmLib(src, map),
                        };
                    }

                case "Unlit/Color":
                    return new VrmLib.UnlitMaterial(src.name)
                    {
                        BaseColorFactor = src.color.FromUnitySrgbToLinear(),
                    };

                case "Unlit/Texture":
                    return new VrmLib.UnlitMaterial(src.name)
                    {
                        BaseColorTexture = map(src, src.mainTexture as Texture2D, VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default),
                    };

                case "Unlit/Transparent":
                    return new VrmLib.UnlitMaterial(src.name)
                    {
                        BaseColorTexture = map(src, src.mainTexture as Texture2D, VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default),
                        AlphaMode = VrmLib.AlphaModeType.BLEND,
                    };

                case "Unlit/Transparent Cutout":
                    return new VrmLib.UnlitMaterial(src.name)
                    {
                        BaseColorTexture = map(src, src.mainTexture as Texture2D, VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default),
                        AlphaMode = VrmLib.AlphaModeType.MASK,
                        AlphaCutoff = src.GetFloat("_Cutoff"),
                    };

                case "UniGLTF/UniUnlit":
                case "VRM/UniUnlit":
                    {
                        var material = new VrmLib.UnlitMaterial(src.name)
                        {
                            BaseColorFactor = src.color.FromUnitySrgbToLinear(),
                            BaseColorTexture = map(src, src.mainTexture as Texture2D, VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default),
                            AlphaMode = GetAlphaMode(src),
                            DoubleSided = UniGLTF.UniUnlit.Utils.GetCullMode(src) == UniGLTF.UniUnlit.UniUnlitCullMode.Off,
                        };
                        if (material.AlphaMode == VrmLib.AlphaModeType.MASK)
                        {
                            material.AlphaCutoff = src.GetFloat("_Cutoff");
                        }
                        // TODO: VertexColorMode
                        return material;
                    }

                default:
                    return ExportStandard(src, map);
            }
        }

        static VrmLib.AlphaModeType GetAlphaMode(UnityEngine.Material m)
        {
            switch (UniGLTF.UniUnlit.Utils.GetRenderMode(m))
            {
                case UniGLTF.UniUnlit.UniUnlitRenderMode.Opaque: return VrmLib.AlphaModeType.OPAQUE;
                case UniGLTF.UniUnlit.UniUnlitRenderMode.Cutout: return VrmLib.AlphaModeType.MASK;
                case UniGLTF.UniUnlit.UniUnlitRenderMode.Transparent: return VrmLib.AlphaModeType.BLEND;
            }
            throw new NotImplementedException();
        }

        static VrmLib.PBRMaterial ExportStandard(UnityEngine.Material src, GetOrCreateTextureDelegate map)
        {
            var material = new VrmLib.PBRMaterial(src.name)
            {
            };

            switch (src.GetTag("RenderType", true))
            {
                case "Transparent":
                    material.AlphaMode = VrmLib.AlphaModeType.BLEND;
                    break;

                case "TransparentCutout":
                    material.AlphaMode = VrmLib.AlphaModeType.MASK;
                    material.AlphaCutoff = src.GetFloat("_Cutoff");
                    break;

                default:
                    material.AlphaMode = VrmLib.AlphaModeType.OPAQUE;
                    break;
            }

            if (src.HasProperty("_Color"))
            {
                material.BaseColorFactor = src.color.linear.FromUnitySrgbToLinear();
            }

            if (src.HasProperty("_MainTex"))
            {
                material.BaseColorTexture = map(src, src.GetTexture("_MainTex"), VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default);
            }

            if (src.HasProperty("_MetallicGlossMap"))
            {
                // float smoothness = 0.0f;
                // if (m.HasProperty("_GlossMapScale"))
                // {
                //     smoothness = m.GetFloat("_GlossMapScale");
                // }

                material.MetallicRoughnessTexture = map(
                    src,
                    src.GetTexture("_MetallicGlossMap"),
                    VrmLib.Texture.ColorSpaceTypes.Linear,
                    VrmLib.Texture.TextureTypes.MetallicRoughness)?.Texture;
                if (material.MetallicRoughnessTexture != null)
                {
                    material.MetallicFactor = 1.0f;
                    // Set 1.0f as hard-coded. See: https://github.com/vrm-c/UniVRM/issues/212.
                    material.RoughnessFactor = 1.0f;
                }
            }

            if (material.MetallicRoughnessTexture == null)
            {
                if (src.HasProperty("_Metallic"))
                {
                    material.MetallicFactor = src.GetFloat("_Metallic");
                }

                if (src.HasProperty("_Glossiness"))
                {
                    material.RoughnessFactor = 1.0f - src.GetFloat("_Glossiness");
                }
            }

            if (src.HasProperty("_BumpMap"))
            {
                material.NormalTexture = map(src, src.GetTexture("_BumpMap"), VrmLib.Texture.ColorSpaceTypes.Linear, VrmLib.Texture.TextureTypes.NormalMap)?.Texture;

                if (src.HasProperty("_BumpScale"))
                {
                    material.NormalTextureScale = src.GetFloat("_BumpScale");
                }
            }

            if (src.HasProperty("_OcclusionMap"))
            {
                material.OcclusionTexture = map(src, src.GetTexture("_OcclusionMap"), VrmLib.Texture.ColorSpaceTypes.Linear, VrmLib.Texture.TextureTypes.Occlusion)?.Texture;

                if (src.HasProperty("_OcclusionStrength"))
                {
                    material.OcclusionTextureStrength = src.GetFloat("_OcclusionStrength");
                }
            }

            if (src.IsKeywordEnabled("_EMISSION"))
            {
                if (src.HasProperty("_EmissionColor"))
                {
                    var color = src.GetColor("_EmissionColor");
                    if (color.maxColorComponent > 1)
                    {
                        color /= color.maxColorComponent;
                    }
                    material.EmissiveFactor = new System.Numerics.Vector3(color.r, color.g, color.b);
                }

                if (src.HasProperty("_EmissionMap"))
                {
                    material.EmissiveTexture = map(src, src.GetTexture("_EmissionMap"), VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Emissive)?.Texture;
                }
            }

            return material;
        }

        private static void CreateNodes(
            Transform parentTransform,
            VrmLib.Node parentNode,
            Dictionary<GameObject, VrmLib.Node> nodes)
        {
            // parentNode.SetMatrix(parentTransform.localToWorldMatrix.ToNumericsMatrix4x4(), false);
            parentNode.LocalTranslation = parentTransform.localPosition.ToNumericsVector3();
            parentNode.LocalRotation = parentTransform.localRotation.ToNumericsQuaternion();
            parentNode.LocalScaling = parentTransform.localScale.ToNumericsVector3();
            nodes.Add(parentTransform.gameObject, parentNode);

            foreach (Transform child in parentTransform)
            {
                var childNode = new VrmLib.Node(child.gameObject.name);
                CreateNodes(child, childNode, nodes);
                parentNode.Add(childNode);
            }
        }

        private static Transform GetTransformFromRelativePath(Transform root, string relativePath)
        {
            var paths = new Queue<string>(relativePath.Split('/'));
            return GetTransformFromRelativePath(root, paths);
        }

        private static Transform GetTransformFromRelativePath(Transform root, Queue<string> relativePath)
        {
            var name = relativePath.Dequeue();
            foreach (Transform node in root)
            {
                if (node.gameObject.name == name)
                {
                    if (relativePath.Count == 0)
                    {
                        return node;
                    }
                    else
                    {
                        return GetTransformFromRelativePath(node, relativePath);
                    }
                }
            }

            return null;
        }

        private static VrmLib.MeshGroup CreateMesh(UnityEngine.Mesh mesh, Renderer renderer, Dictionary<UnityEngine.Material, VrmLib.Material> materials)
        {
            var meshGroup = new VrmLib.MeshGroup(mesh.name);
            var vrmMesh = new VrmLib.Mesh();
            vrmMesh.VertexBuffer = new VrmLib.VertexBuffer();
            vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(mesh.vertices));

            if (mesh.boneWeights.Length == mesh.vertexCount)
            {
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.WeightKey,
                    ToBufferAccessor(mesh.boneWeights.Select(x =>
                    new Vector4(x.weight0, x.weight1, x.weight2, x.weight3)).ToArray()
                    ));
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.JointKey,
                    ToBufferAccessor(mesh.boneWeights.Select(x =>
                    new VrmLib.SkinJoints((ushort)x.boneIndex0, (ushort)x.boneIndex1, (ushort)x.boneIndex2, (ushort)x.boneIndex3)).ToArray()
                    ));
            }
            if (mesh.uv.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.TexCoordKey, ToBufferAccessor(mesh.uv));
            if (mesh.normals.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.NormalKey, ToBufferAccessor(mesh.normals));
            if (mesh.colors.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.ColorKey, ToBufferAccessor(mesh.colors));
            vrmMesh.IndexBuffer = ToBufferAccessor(mesh.triangles);

            int offset = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
#if UNITY_2019
                var subMesh = mesh.GetSubMesh(i);
                try
                {
                    vrmMesh.Submeshes.Add(new VrmLib.Submesh(offset, subMesh.indexCount, materials[renderer.sharedMaterials[i]]));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                offset += subMesh.indexCount;
#else
                var triangles = mesh.GetTriangles(i);
                try
                {
                    vrmMesh.Submeshes.Add(new VrmLib.Submesh(offset, triangles.Length, materials[renderer.sharedMaterials[i]]));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                offset += triangles.Length;
#endif
            }

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                var blendShapeVertices = mesh.vertices;
                var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

                var blendShapeNormals = mesh.normals;
                var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length;
                // var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length && !exportOnlyBlendShapePosition;

                var blendShapeTangents = mesh.tangents.Select(y => (Vector3)y).ToArray();
                //var useTangent = usePosition && blendShapeTangents != null && blendShapeTangents.Length == blendShapeVertices.Length;
                // var useTangent = false;

                var frameCount = mesh.GetBlendShapeFrameCount(i);
                mesh.GetBlendShapeFrameVertices(i, frameCount - 1, blendShapeVertices, blendShapeNormals, null);

                if (usePosition)
                {
                    var morphTarget = new VrmLib.MorphTarget(mesh.GetBlendShapeName(i));
                    morphTarget.VertexBuffer = new VrmLib.VertexBuffer();
                    morphTarget.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(blendShapeVertices));
                    vrmMesh.MorphTargets.Add(morphTarget);
                }
            }

            meshGroup.Meshes.Add(vrmMesh);
            return meshGroup;
        }

        private static VrmLib.Skin CreateSkin(
            SkinnedMeshRenderer skinnedMeshRenderer,
            Dictionary<GameObject, VrmLib.Node> nodes,
            GameObject root)
        {
            if (skinnedMeshRenderer.bones == null || skinnedMeshRenderer.bones.Length == 0)
            {
                return null;
            }

            var skin = new VrmLib.Skin();
            skin.InverseMatrices = ToBufferAccessor(skinnedMeshRenderer.sharedMesh.bindposes);
            if (skinnedMeshRenderer.rootBone != null)
            {
                skin.Root = nodes[skinnedMeshRenderer.rootBone.gameObject];
            }

            skin.Joints = skinnedMeshRenderer.bones.Select(x => nodes[x.gameObject]).ToList();
            return skin;
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(VrmLib.SkinJoints[] values)
        {
            return ToBufferAccessor(values, VrmLib.AccessorValueType.UNSIGNED_SHORT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Color[] colors)
        {
            return ToBufferAccessor(colors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector4[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector3[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC3);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector2[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC2);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(int[] scalars)
        {
            return ToBufferAccessor(scalars, VrmLib.AccessorValueType.UNSIGNED_INT, VrmLib.AccessorVectorType.SCALAR);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Matrix4x4[] matrixes)
        {
            return ToBufferAccessor(matrixes, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.MAT4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor<T>(T[] value, VrmLib.AccessorValueType valueType, VrmLib.AccessorVectorType vectorType) where T : struct
        {
            var span = VrmLib.SpanLike.CopyFrom(value);
            return new VrmLib.BufferAccessor(
                span.Bytes,
                valueType,
                vectorType,
                value.Length
                );
        }
    }
}
