using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UniVRM10
{
    public static class ComponentBuilder
    {
        #region Util
        static (Transform, Mesh) GetTransformAndMesh(Transform t)
        {
            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return (t, skinnedMeshRenderer.sharedMesh);
            }

            var filter = t.GetComponent<MeshFilter>();
            if (filter != null)
            {
                return (t, filter.sharedMesh);
            }

            return default;
        }
        #endregion

        #region Build10

        static UniVRM10.MorphTargetBinding Build10(this VrmLib.MorphTargetBind bind, GameObject root, ModelMap loader)
        {
            var node = loader.Nodes[bind.Node].transform;
            var mesh = loader.Meshes[bind.Node.MeshGroup];
            // var transformMeshTable = loader.Root.transform.Traverse()
            //     .Select(GetTransformAndMesh)
            //     .Where(x => x.Item2 != null)
            //     .ToDictionary(x => x.Item2, x => x.Item1);
            // var node = transformMeshTable[mesh];
            // var transform = loader.Nodes[node].transform;
            var relativePath = node.RelativePathFrom(root.transform);

            var names = new List<string>();
            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                names.Add(mesh.GetBlendShapeName(i));
            }

            // VRM-1.0 では値域は [0-1.0f]
            return new UniVRM10.MorphTargetBinding(relativePath, names.IndexOf(bind.Name), bind.Value * 100.0f);
        }

        static UniVRM10.MaterialColorBinding? Build10(this VrmLib.MaterialColorBind bind, ModelMap loader)
        {
            var kv = bind.Property;
            var value = kv.Value.ToUnityVector4();
            var material = loader.Materials[bind.Material];

            var binding = default(UniVRM10.MaterialColorBinding?);
            if (material != null)
            {
                try
                {
                    binding = new UniVRM10.MaterialColorBinding
                    {
                        MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
                        BindType = bind.BindType,
                        TargetValue = value,
                        // BaseValue = material.GetColor(kv.Key),
                    };
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return binding;
        }

        static UniVRM10.MaterialUVBinding? Build10(this VrmLib.TextureTransformBind bind, ModelMap loader)
        {
            var material = loader.Materials[bind.Material];

            var binding = default(UniVRM10.MaterialUVBinding?);
            if (material != null)
            {
                try
                {
                    binding = new UniVRM10.MaterialUVBinding
                    {
                        MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
                        Scaling = new Vector2(bind.Scale.X, bind.Scale.Y),
                        Offset = new Vector2(bind.Offset.X, bind.Offset.Y),
                    };
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return binding;
        }

        public static void Build10(VrmLib.Model model, ModelAsset asset)
        {
            // meta
            var controller = asset.Root.AddComponent<UniVRM10.VRM10Controller>();
            {
                var meta = model.Vrm.Meta;
                controller.Meta = ScriptableObject.CreateInstance<UniVRM10.VRM10MetaObject>();
                controller.Meta.Name = meta.Name;
                controller.Meta.Version = meta.Version;
                controller.Meta.CopyrightInformation = meta.CopyrightInformation;
                controller.Meta.Authors = meta.Authors.ToArray();
                controller.Meta.ContactInformation = meta.ContactInformation;
                controller.Meta.Reference = meta.Reference;
                var thumbnailImages = asset.Map.Textures.Where(x => ((VrmLib.ImageTexture)x.Key).Image == meta.Thumbnail);
                if (meta.Thumbnail != null && thumbnailImages.Count() > 0)
                {
                    controller.Meta.Thumbnail = thumbnailImages.First().Value;
                }
                else if (meta.Thumbnail != null && meta.Thumbnail.Bytes.Count > 0)
                {
                    var thumbnail = new Texture2D(2, 2, TextureFormat.ARGB32, false, false);
                    thumbnail.name = "Thumbnail";
                    thumbnail.LoadImage(meta.Thumbnail.Bytes.ToArray());
                    controller.Meta.Thumbnail = thumbnail;
                    asset.Textures.Add(thumbnail);
                }
                // avatar permission
                controller.Meta.AllowedUser = meta.AvatarPermission.AvatarUsage;
                controller.Meta.ViolentUsage = meta.AvatarPermission.IsAllowedViolentUsage;
                controller.Meta.SexualUsage = meta.AvatarPermission.IsAllowedSexualUsage;
                controller.Meta.CommercialUsage = meta.AvatarPermission.CommercialUsage;
                controller.Meta.GameUsage = meta.AvatarPermission.IsAllowedGameUsage;
                controller.Meta.PoliticalOrReligiousUsage = meta.AvatarPermission.IsAllowedPoliticalOrReligiousUsage;
                controller.Meta.OtherPermissionUrl = meta.AvatarPermission.OtherPermissionUrl;

                // redistribution license
                controller.Meta.CreditNotation = meta.RedistributionLicense.CreditNotation;
                controller.Meta.ModificationLicense = meta.RedistributionLicense.ModificationLicense;
                controller.Meta.Redistribution = meta.RedistributionLicense.IsAllowRedistribution;
                controller.Meta.OtherLicenseUrl = meta.RedistributionLicense.OtherLicenseUrl;

                asset.ScriptableObjects.Add(controller.Meta);
            }

            // expression
            {
                controller.Expression.ExpressionAvatar = ScriptableObject.CreateInstance<UniVRM10.VRM10ExpressionAvatar>();
                asset.ScriptableObjects.Add(controller.Expression.ExpressionAvatar);
                if (model.Vrm.ExpressionManager != null)
                {
                    foreach (var expression in model.Vrm.ExpressionManager.ExpressionList)
                    {
                        var clip = ScriptableObject.CreateInstance<UniVRM10.VRM10Expression>();
                        clip.Preset = expression.Preset;
                        clip.ExpressionName = expression.Name;
                        clip.IsBinary = expression.IsBinary;
                        clip.OverrideBlink = expression.OverrideBlink;
                        clip.OverrideLookAt = expression.OverrideLookAt;
                        clip.OverrideMouth = expression.OverrideMouth;

                        clip.MorphTargetBindings = expression.MorphTargetBinds.Select(x => x.Build10(asset.Root, asset.Map))
                            .ToArray();
                        clip.MaterialColorBindings = expression.MaterialColorBinds.Select(x => x.Build10(asset.Map))
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToArray();
                        clip.MaterialUVBindings = expression.TextureTransformBinds.Select(x => x.Build10(asset.Map))
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToArray();
                        controller.Expression.ExpressionAvatar.Clips.Add(clip);
                        asset.ScriptableObjects.Add(clip);
                    }
                }
            }

            // firstPerson
            {
                // VRMFirstPerson
                if (model.Vrm.FirstPerson != null)
                {
                    controller.FirstPerson.Renderers = model.Vrm.FirstPerson.Annotations.Select(x =>
                        new UniVRM10.RendererFirstPersonFlags()
                        {
                            Renderer = asset.Map.Renderers[x.Node],
                            FirstPersonFlag = x.FirstPersonFlag
                        }
                        ).ToList();
                }

                // VRMLookAtApplyer
                if (model.Vrm.LookAt != null)
                {
                    controller.LookAt.OffsetFromHead = model.Vrm.LookAt.OffsetFromHeadBone.ToUnityVector3();
                    if (model.Vrm.LookAt.LookAtType == VrmLib.LookAtType.Expression)
                    {
                        var lookAtApplyer = controller;
                        lookAtApplyer.LookAt.LookAtType = VRM10ControllerLookAt.LookAtTypes.Expression;
                        lookAtApplyer.LookAt.HorizontalOuter = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.HorizontalOuter.InputMaxValue,
                            model.Vrm.LookAt.HorizontalOuter.OutputScaling);
                        lookAtApplyer.LookAt.VerticalUp = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.VerticalUp.InputMaxValue,
                            model.Vrm.LookAt.VerticalUp.OutputScaling);
                        lookAtApplyer.LookAt.VerticalDown = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.VerticalDown.InputMaxValue,
                            model.Vrm.LookAt.VerticalDown.OutputScaling);
                    }
                    else if (model.Vrm.LookAt.LookAtType == VrmLib.LookAtType.Bone)
                    {
                        var lookAtBoneApplyer = controller;
                        lookAtBoneApplyer.LookAt.HorizontalInner = new UniVRM10.CurveMapper(
                             model.Vrm.LookAt.HorizontalInner.InputMaxValue,
                             model.Vrm.LookAt.HorizontalInner.OutputScaling);
                        lookAtBoneApplyer.LookAt.HorizontalOuter = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.HorizontalOuter.InputMaxValue,
                            model.Vrm.LookAt.HorizontalOuter.OutputScaling);
                        lookAtBoneApplyer.LookAt.VerticalUp = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.VerticalUp.InputMaxValue,
                            model.Vrm.LookAt.VerticalUp.OutputScaling);
                        lookAtBoneApplyer.LookAt.VerticalDown = new UniVRM10.CurveMapper(
                            model.Vrm.LookAt.VerticalDown.InputMaxValue,
                            model.Vrm.LookAt.VerticalDown.OutputScaling);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            // springBone
            if (model.Vrm.SpringBone != null)
            {
                foreach (var vrmSpring in model.Vrm.SpringBone.Springs)
                {
                    // create a spring
                    var springBone = new UniVRM10.VRM10SpringBone();
                    springBone.m_comment = vrmSpring.Comment;
                    if (vrmSpring.Origin != null && asset.Map.Nodes.TryGetValue(vrmSpring.Origin, out GameObject origin))
                    {
                        springBone.m_center = origin.transform;
                    }
                    controller.SpringBone.Springs.Add(springBone);

                    // create colliders for the spring
                    foreach (var vrmSpringBoneCollider in vrmSpring.Colliders)
                    {
                        var go = asset.Map.Nodes[vrmSpringBoneCollider.Node];
                        var springBoneColliderGroup = go.GetComponent<UniVRM10.VRM10SpringBoneColliderGroup>();
                        if (springBoneColliderGroup != null)
                        {
                            // already setup
                        }
                        else
                        {
                            // new collider
                            springBoneColliderGroup = go.AddComponent<UniVRM10.VRM10SpringBoneColliderGroup>();

                            // add collider shapes
                            springBoneColliderGroup.Colliders.Clear();
                            foreach (var x in vrmSpringBoneCollider.Colliders)
                            {
                                switch (x.ColliderType)
                                {
                                    case VrmLib.VrmSpringBoneColliderTypes.Sphere:
                                        springBoneColliderGroup.Colliders.Add(new UniVRM10.VRM10SpringBoneCollider()
                                        {
                                            ColliderType = VRM10SpringBoneColliderTypes.Sphere,
                                            Offset = x.Offset.ToUnityVector3(),
                                            Radius = x.Radius
                                        });
                                        break;

                                    case VrmLib.VrmSpringBoneColliderTypes.Capsule:
                                        springBoneColliderGroup.Colliders.Add(new UniVRM10.VRM10SpringBoneCollider()
                                        {
                                            ColliderType = VRM10SpringBoneColliderTypes.Capsule,
                                            Offset = x.Offset.ToUnityVector3(),
                                            Radius = x.Radius,
                                            Tail = x.CapsuleTail.ToUnityVector3(),
                                        });
                                        break;

                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }
                        springBone.ColliderGroups.Add(springBoneColliderGroup);
                    }

                    // create joint for the spring
                    foreach (var vrmJoint in vrmSpring.Joints)
                    {
                        var go = asset.Map.Nodes[vrmJoint.Node];
                        var joint = new VRM10SpringJoint(go.transform);

                        joint.m_stiffnessForce = vrmJoint.Stiffness;
                        joint.m_gravityPower = vrmJoint.GravityPower;
                        joint.m_gravityDir = vrmJoint.GravityDir.ToUnityVector3();
                        joint.m_dragForce = vrmJoint.DragForce;
                        joint.m_jointRadius = vrmJoint.HitRadius;
                        joint.m_exclude = vrmJoint.Exclude;

                        springBone.Joints.Add(joint);
                    }
                }
            }

            // Assets
            controller.ModelAsset = asset;
        }
        #endregion
    }
}
