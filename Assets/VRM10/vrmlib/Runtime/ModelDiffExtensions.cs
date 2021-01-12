using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VrmLib.MToon;

namespace VrmLib.Diff
{
    public static class ModelDiffExtensions
    {
        /// <summary>
        /// 違うところを集める(debug用)
        /// </summary>
        public static List<ModelDiff> Diff(this Model lhs, Model rhs)
        {
            var context = ModelDiffContext.Create();
            context.Enter(nameof(lhs.AssetGenerator)).Push(lhs.AssetGenerator, rhs.AssetGenerator, StringEquals);
            context.Enter(nameof(lhs.AssetVersion)).Push(lhs.AssetVersion, rhs.AssetVersion, StringEquals);
            context.Enter(nameof(lhs.AssetMinVersion)).Push(lhs.AssetMinVersion, rhs.AssetMinVersion, StringEquals);
            context.Enter(nameof(lhs.AssetCopyright)).Push(lhs.AssetCopyright, rhs.AssetCopyright, StringEquals);

            // Materialの参照で比較する
            ListDiff(context.Enter("Materials"), lhs.Materials, rhs.Materials, MaterialEquals);
            ListDiff(context.Enter("Meshes"), lhs.MeshGroups, rhs.MeshGroups, MeshGroupEquals);
            ListDiff(context.Enter("Nodes"), lhs.Nodes, rhs.Nodes, NodeEquals);
            ListDiff(context.Enter("Skins"), lhs.Skins, rhs.Skins, SkinEquals);
            Vrm(context.Enter("Vrm"), lhs, rhs);

            return context.List;
        }

        #region Private
        static bool ListDiff<T>(ModelDiffContext context, List<T> lhs, List<T> rhs, Func<ModelDiffContext, T, T, bool> pred, Func<T, int> order = null)
        {
            var equals = true;
            if (lhs.Count != rhs.Count)
            {
                equals = false;
                context.List.Add(new ModelDiff
                {
                    Context = context.Path,
                    Message = $"{lhs.Count} != {rhs.Count}",
                });
            }

            var l = order != null ? lhs.OrderBy(order).GetEnumerator() : lhs.GetEnumerator();
            var r = order != null ? rhs.OrderBy(order).GetEnumerator() : rhs.GetEnumerator();
            for (int i = 0; i < lhs.Count; ++i)
            {
                l.MoveNext();
                r.MoveNext();
                if (!pred(context.Enter($"{i}"), l.Current, r.Current))
                    equals = false;
            }
            return equals;
        }

        const float EPSILON = 1e-5f;

        static bool Vector3NearlyEquals(ModelDiffContext _, Vector3 l, Vector3 r)
        {
            if (Math.Abs(l.X - r.X) > EPSILON) return false;
            if (Math.Abs(l.Y - r.Y) > EPSILON) return false;
            if (Math.Abs(l.Z - r.Z) > EPSILON) return false;
            return true;
        }

        static bool QuaternionNearlyEquals(ModelDiffContext _, Quaternion l, Quaternion r)
        {
            if (Math.Abs(l.X - r.X) > EPSILON) return false;
            if (Math.Abs(l.Y - r.Y) > EPSILON) return false;
            if (Math.Abs(l.Z - r.Z) > EPSILON) return false;
            if (Math.Abs(l.W - r.W) > EPSILON) return false;
            return true;
        }

        static bool StringEquals(ModelDiffContext _, string l, string r)
        {
            if (string.IsNullOrEmpty(l))
            {
                return string.IsNullOrEmpty(r);
            }
            else
            {
                if (string.IsNullOrEmpty(r))
                {
                    return false;
                }
                else
                {
                    return l == r;
                }
            }
        }

        static bool ImageBytesEquals(ModelDiffContext context, Image lhs, Image rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (rhs is null)
            {
                return false;
            }
            return lhs.Bytes.SequenceEqual(rhs.Bytes);
        }

        static void Image(ModelDiffContext context, Image lhs, Image rhs)
        {
            context.Enter($"{lhs.Name}:{rhs.Name}").Push(lhs, rhs, ImageBytesEquals);
        }

        static bool TextureInfoEquals(ModelDiffContext context, TextureInfo lhs, TextureInfo rhs)
        {
            if (!context.RequireComapre(lhs, rhs, out bool equals))
            {
                return equals;
            }

            if (lhs.Offset != rhs.Offset)
            {
                return false;
            }
            if (lhs.Scaling != rhs.Scaling)
            {
                return false;
            }
            return TextureEquals(context.Enter("Texture"), lhs.Texture, rhs.Texture);
        }

        static bool TextureEquals(ModelDiffContext context, Texture lhs, Texture rhs)
        {
            if (!context.RequireComapre(lhs, rhs, out bool equals))
            {
                if (!equals)
                    return false;
                return true;
            }

            equals = true;
            if (!context.Enter("Name").Push(lhs.Name, rhs.Name, StringEquals))
                equals = false;
            if (!context.Enter("MagFilter").Push(lhs.Sampler.MagFilter, rhs.Sampler.MagFilter))
                equals = false;
            if (!context.Enter("MinFilter").Push(lhs.Sampler.MinFilter, rhs.Sampler.MinFilter))
                equals = false;
            if (!context.Enter("WrapS").Push(lhs.Sampler.WrapS, rhs.Sampler.WrapS))
                equals = false;
            if (!context.Enter("WrapT").Push(lhs.Sampler.WrapT, rhs.Sampler.WrapT))
                equals = false;
            if (lhs is ImageTexture l && rhs is ImageTexture r)
            {
                if (!ImageBytesEquals(context, l.Image, r.Image))
                    equals = false;
                return equals;
            }
            else
            {
                return false;
            }
        }

        static void Texture(ModelDiffContext context, Texture lhs, Texture rhs)
        {
            context.Enter($"{lhs.Name}:{rhs.Name}").Push(lhs, rhs, TextureEquals);
        }

        static bool BaseMaterialEquals(ModelDiffContext context, Material lhs, Material rhs)
        {
            var equals = true;
            if (!context.Enter(nameof(lhs.AlphaCutoff)).Push(lhs.AlphaCutoff, rhs.AlphaCutoff)) equals = false;
            if (!context.Enter(nameof(lhs.AlphaMode)).Push(lhs.AlphaMode, rhs.AlphaMode)) equals = false;
            if (!context.Enter(nameof(lhs.BaseColorFactor)).Push(lhs.BaseColorFactor, rhs.BaseColorFactor)) equals = false;
            if (!context.Enter(nameof(lhs.BaseColorTexture)).Push(lhs.BaseColorTexture?.Texture, rhs.BaseColorTexture?.Texture, TextureEquals)) equals = false;
            if (!context.Enter(nameof(lhs.DoubleSided)).Push(lhs.DoubleSided, rhs.DoubleSided)) equals = false;
            return equals;
        }

        static bool PBRMaterialEquals(ModelDiffContext context, PBRMaterial lhs, PBRMaterial rhs)
        {
            var equals = true;
            if (!BaseMaterialEquals(context, lhs, rhs)) equals = false;
            if (!context.Enter(nameof(lhs.EmissiveFactor)).Push(lhs.EmissiveFactor, rhs.EmissiveFactor)) equals = false;
            if (!context.Enter(nameof(lhs.EmissiveTexture)).Push(lhs.EmissiveTexture, rhs.EmissiveTexture, TextureEquals)) equals = false;
            if (!context.Enter(nameof(lhs.MetallicFactor)).Push(lhs.MetallicFactor, rhs.MetallicFactor)) equals = false;
            if (!context.Enter(nameof(lhs.MetallicRoughnessTexture)).Push(lhs.MetallicRoughnessTexture, rhs.MetallicRoughnessTexture, TextureEquals)) equals = false;
            if (!context.Enter(nameof(lhs.NormalTexture)).Push(lhs.NormalTexture, rhs.NormalTexture, TextureEquals)) equals = false;
            if (!context.Enter(nameof(lhs.OcclusionTexture)).Push(lhs.OcclusionTexture, rhs.OcclusionTexture, TextureEquals)) equals = false;
            if (!context.Enter(nameof(lhs.RoughnessFactor)).Push(lhs.RoughnessFactor, rhs.RoughnessFactor)) equals = false;
            return equals;
        }

        static bool MToonDefinitionEquals(ModelDiffContext context, object lhs, object rhs, Type t)
        {
            if (!context.RequireComapre(lhs, rhs, out bool equals))
            {
                return equals;
            }

            equals = true;
            foreach (var fi in t.GetFields())
            {
                if (fi.FieldType == typeof(TextureInfo))
                {
                    if (!context.Enter(fi.Name).Push(fi.GetValue(lhs) as TextureInfo, fi.GetValue(rhs) as TextureInfo, TextureInfoEquals))
                        equals = false;
                }
                else
                {
                    if (!context.Enter(fi.Name).Push(fi.GetValue(lhs), fi.GetValue(rhs)))
                        equals = false;
                }
            }
            return equals;
        }

        static bool MToonDefinitionEquals(ModelDiffContext context, MToonDefinition lhs, MToonDefinition rhs)
        {
            var equals = true;
            if (!MToonDefinitionEquals(context.Enter(nameof(MetaDefinition)), lhs?.Meta, rhs?.Meta, typeof(MetaDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(ColorDefinition)), lhs?.Color, rhs?.Color, typeof(ColorDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(OutlineDefinition)), lhs?.Outline, rhs?.Outline, typeof(OutlineDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(LightingInfluenceDefinition)), lhs?.Lighting.LightingInfluence, rhs?.Lighting.LightingInfluence, typeof(LightingInfluenceDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(LitAndShadeMixingDefinition)), lhs?.Lighting.LitAndShadeMixing, rhs?.Lighting.LitAndShadeMixing, typeof(LitAndShadeMixingDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(EmissionDefinition)), lhs?.Emission, rhs?.Emission, typeof(EmissionDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(MatCapDefinition)), lhs?.MatCap, rhs?.MatCap, typeof(MatCapDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(RimDefinition)), lhs?.Rim, rhs?.Rim, typeof(RimDefinition)))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(TextureUvCoordsDefinition)), lhs?.TextureOption, rhs?.TextureOption, typeof(TextureUvCoordsDefinition)))
                equals = false;
            return equals;
        }

        static bool MToonMaterialEquals(ModelDiffContext context, MToonMaterial lhs, MToonMaterial rhs)
        {
            var equals = true;
            if (!BaseMaterialEquals(context, lhs, rhs))
                equals = false;
            if (!context.Enter(nameof(lhs._DebugMode)).Push(lhs._DebugMode, rhs._DebugMode))
                equals = false;
            if (!context.Enter(nameof(lhs._DstBlend)).Push(lhs._DstBlend, rhs._DstBlend))
                equals = false;
            if (!context.Enter(nameof(lhs._SrcBlend)).Push(lhs._SrcBlend, rhs._SrcBlend))
                equals = false;
            if (!context.Enter(nameof(lhs._ZWrite)).Push(lhs._ZWrite, rhs._ZWrite))
                equals = false;
            if (!MToonDefinitionEquals(context.Enter(nameof(lhs.Definition)), lhs.Definition, rhs.Definition))
                equals = false;
            // context.Enter(nameof(lhs.KeyWords)).Push( lhs.KeyWords, rhs.KeyWords);
            return equals;
        }

        static bool UnlitMaterialEquals(ModelDiffContext context, UnlitMaterial lhs, UnlitMaterial rhs)
        {
            return BaseMaterialEquals(context, lhs, rhs);
        }

        public static bool MaterialEquals(ModelDiffContext context, Material l, Material r)
        {
            var equals = true;
            if (!context.Enter("Name").Push(l.Name, r.Name, StringEquals))
                equals = false;

            // context.Enter($"{i}:{lhs[i].Name}:{rhs[i].Name}").Push( lhs[i], rhs[i], MaterialEquals);
            if (l.GetType() != r.GetType())
            {
                context.Enter($"Type").Push(l.GetType(), r.GetType());
                equals = false;
            }
            else if (l is PBRMaterial lp && r is PBRMaterial rp)
            {
                if (!PBRMaterialEquals(context.Enter($"(PBRMaterial)"), lp, rp))
                    equals = false;
            }
            else if (l is MToonMaterial lm && r is MToonMaterial rm)
            {
                if (!MToonMaterialEquals(context.Enter($"(MToonMaterial)"), lm, rm))
                    equals = false;
            }
            else if (l is UnlitMaterial lu && r is UnlitMaterial ru)
            {
                if (!UnlitMaterialEquals(context.Enter($"(UnlitMaterial)"), lu, ru))
                    equals = false;
            }
            else
            {
                throw new Exception();
            }
            return equals;
        }

        static bool AccessorEquals(ModelDiffContext context, BufferAccessor lhs, BufferAccessor rhs)
        {
            if (!context.RequireComapre(lhs, rhs, out bool equals))
            {
                return equals;
            }

            return lhs.Bytes.SequenceEqual(rhs.Bytes);
        }

        static bool VertexBufferEquals(ModelDiffContext context, VertexBuffer lhs, VertexBuffer rhs)
        {
            var equals = true;
            foreach (var kv in lhs)
            {
                rhs.TryGetValue(kv.Key, out BufferAccessor accessor);
                if (!context.Enter(kv.Key).Push(kv.Value, accessor, AccessorEquals)) equals = false;
            }
            return equals;
        }

        static bool MeshEquals(ModelDiffContext context, Mesh lhs, Mesh rhs)
        {
            return VertexBufferEquals(context.Enter(nameof(lhs.VertexBuffer)), lhs.VertexBuffer, rhs.VertexBuffer);
        }

        static bool MeshGroupEquals(ModelDiffContext context, MeshGroup lhs, MeshGroup rhs)
        {
            return ListDiff(context.Enter("Meshes"), lhs.Meshes, rhs.Meshes, MeshEquals);
        }

        static bool NodeEquals(ModelDiffContext context, Node lhs, Node rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (rhs is null)
                {
                    return false;
                }
            }

            var equals = true;
            if (!context.Enter(nameof(lhs.Name)).Push(lhs.Name, rhs.Name, StringEquals)) equals = false;
            if (!context.Enter(nameof(lhs.LocalTranslation)).Push(lhs.LocalTranslation, rhs.LocalTranslation, Vector3NearlyEquals)) equals = false;
            if (!context.Enter(nameof(lhs.LocalRotation)).Push(lhs.LocalRotation, rhs.LocalRotation, QuaternionNearlyEquals)) equals = false;
            if (!context.Enter(nameof(lhs.LocalScaling)).Push(lhs.LocalScaling, rhs.LocalScaling, Vector3NearlyEquals)) equals = false;
            if (!context.Enter(nameof(lhs.Parent)).Push(lhs.Parent?.Name, rhs.Parent?.Name)) equals = false;
            if (!context.Enter(nameof(lhs.HumanoidBone)).Push(lhs.HumanoidBone, rhs.HumanoidBone)) equals = false;
            return equals;
        }

        static bool SkinEquals(ModelDiffContext context, Skin lhs, Skin rhs)
        {
            var equals = true;
            if (!context.Enter("Root").Push(lhs.Root, rhs.Root, NodeEquals)) equals = false;
            if (!ListDiff(context.Enter("Joints"), lhs.Joints, rhs.Joints, NodeEquals)) equals = false;
            if (!context.Enter("InverseMatrices").Push(lhs.InverseMatrices, rhs.InverseMatrices, AccessorEquals)) equals = false;
            return equals;
        }

        static void Vrm(ModelDiffContext context, Model lhs, Model rhs)
        {
            context.Enter(nameof(lhs.Vrm.SpecVersion)).Push(lhs.Vrm.SpecVersion, rhs.Vrm.SpecVersion);
            context.Enter(nameof(lhs.Vrm.ExporterVersion)).Push(lhs.Vrm.ExporterVersion, rhs.Vrm.ExporterVersion);
            VrmMeta(context.Enter(nameof(lhs.Vrm.Meta)), lhs.Vrm.Meta, rhs.Vrm.Meta);
            ListDiff(context.Enter(nameof(lhs.Vrm.ExpressionManager)), lhs.Vrm.ExpressionManager.ExpressionList, rhs.Vrm.ExpressionManager.ExpressionList, VrmExpressionEquals, x => (int)x.Preset);
            VrmFirstPerson(context.Enter(nameof(lhs.Vrm.FirstPerson)), lhs.Vrm.FirstPerson, rhs.Vrm.FirstPerson);
            VrmLookAt(context.Enter(nameof(lhs.Vrm.LookAt)), lhs.Vrm.LookAt, rhs.Vrm.LookAt);
            ListDiff(context.Enter("SpringBone.Springs"), lhs.Vrm.SpringBone.Springs, rhs.Vrm.SpringBone.Springs, VrmSpringBoneEquals);
            ListDiff(context.Enter("SpringBone.Colliders"), lhs.Vrm.SpringBone.Colliders, rhs.Vrm.SpringBone.Colliders, VrmSpringBoneColliderEquals);
        }

        static void VrmMeta(ModelDiffContext context, Meta lhs, Meta rhs)
        {
            context.Enter(nameof(lhs.Name)).Push(lhs.Name, rhs.Name);
            context.Enter(nameof(lhs.Version)).Push(lhs.Version, rhs.Version);
            context.Enter(nameof(lhs.CopyrightInformation)).Push(lhs.CopyrightInformation, rhs.CopyrightInformation);
            context.Enter(nameof(lhs.Author)).Push(lhs.Author, rhs.Author);
            context.Enter(nameof(lhs.ContactInformation)).Push(lhs.ContactInformation, rhs.ContactInformation);
            context.Enter(nameof(lhs.Reference)).Push(lhs.Reference, rhs.Reference);
            context.Enter(nameof(lhs.Thumbnail)).Push(lhs.Thumbnail, rhs.Thumbnail, ImageBytesEquals);
            // AvatarPermission
            context.Enter(nameof(lhs.AvatarPermission.AvatarUsage)).Push(lhs.AvatarPermission.AvatarUsage, rhs.AvatarPermission.AvatarUsage);
            context.Enter(nameof(lhs.AvatarPermission.IsAllowedViolentUsage)).Push(lhs.AvatarPermission.IsAllowedViolentUsage, rhs.AvatarPermission.IsAllowedViolentUsage);
            context.Enter(nameof(lhs.AvatarPermission.IsAllowedSexualUsage)).Push(lhs.AvatarPermission.IsAllowedSexualUsage, rhs.AvatarPermission.IsAllowedSexualUsage);
            context.Enter(nameof(lhs.AvatarPermission.IsAllowedCommercialUsage)).Push(lhs.AvatarPermission.IsAllowedCommercialUsage, rhs.AvatarPermission.IsAllowedCommercialUsage);
            context.Enter(nameof(lhs.AvatarPermission.CommercialUsage)).Push(lhs.AvatarPermission.CommercialUsage, rhs.AvatarPermission.CommercialUsage);
            context.Enter(nameof(lhs.AvatarPermission.IsAllowedCommercialUsage)).Push(lhs.AvatarPermission.IsAllowedCommercialUsage, rhs.AvatarPermission.IsAllowedCommercialUsage);
            context.Enter(nameof(lhs.AvatarPermission.IsAllowedCommercialUsage)).Push(lhs.AvatarPermission.IsAllowedCommercialUsage, rhs.AvatarPermission.IsAllowedCommercialUsage);
            context.Enter(nameof(lhs.AvatarPermission.OtherPermissionUrl)).Push(lhs.AvatarPermission.OtherPermissionUrl, rhs.AvatarPermission.OtherPermissionUrl);
            // RedistributionLicense
            context.Enter(nameof(lhs.RedistributionLicense.License)).Push(lhs.RedistributionLicense.License, rhs.RedistributionLicense.License);
            context.Enter(nameof(lhs.RedistributionLicense.OtherLicenseUrl)).Push(lhs.RedistributionLicense.OtherLicenseUrl, rhs.RedistributionLicense.OtherLicenseUrl);
        }

        static bool VrmExpressionEquals(ModelDiffContext context, Expression lhs, Expression rhs)
        {
            if (lhs.IsNull())
            {
                if (rhs.IsNull())
                {
                    // ok
                    return true;
                }
                else
                {
                    context.List.Add(new ModelDiff
                    {
                        Context = context.Path,
                        Message = "lhs is null",
                    });
                    return false;
                }
            }
            else
            {
                if (rhs.IsNull())
                {
                    context.List.Add(new ModelDiff
                    {
                        Context = context.Path,
                        Message = "rhs is null",
                    });
                    return false;
                }
            }

            var equals = true;
            if (!context.Enter(nameof(lhs.Preset)).Push(lhs.Preset, rhs.Preset)) equals = false;
            if (!context.Enter(nameof(lhs.Name)).Push(lhs.Name, rhs.Name, StringEquals)) equals = false;
            if (!context.Enter(nameof(lhs.IsBinary)).Push(lhs.IsBinary, rhs.IsBinary)) equals = false;
            if (!ListDiff(context.Enter(nameof(lhs.MorphTargetBinds)), lhs.MorphTargetBinds, rhs.MorphTargetBinds, VrmExpressionBindValueEquals)) equals = false;
            if (!ListDiff(context.Enter(nameof(lhs.MaterialColorBinds)), lhs.MaterialColorBinds, rhs.MaterialColorBinds, VrmMaterialBindValueEquals)) equals = false;
            return equals;
        }

        static bool VrmExpressionBindValueEquals(ModelDiffContext context, MorphTargetBind lhs, MorphTargetBind rhs)
        {
            var equals = true;
            if (!context.Enter("Node").Push(lhs.Node, rhs.Node, NodeEquals)) equals = false;
            if (!context.Enter("Name").Push(lhs.Name, rhs.Name)) equals = false;
            if (!context.Enter("Value").Push(lhs.Value, rhs.Value)) equals = false;
            return equals;
        }

        static bool VrmMaterialBindValueEquals(ModelDiffContext context, MaterialColorBind lhs, MaterialColorBind rhs)
        {
            var equals = true;
            if (!context.Enter("Material.Name").Push(lhs.Material.Name, rhs.Material.Name)) equals = false;
            if (!context.Enter("Property").Push(lhs.Property, rhs.Property)) equals = false;
            // if (!context.Enter("Value").Push(lhs.m_value, rhs.m_value)) equals = false;
            if (!context.Enter("BindType").Push(lhs.BindType, rhs.BindType)) equals = false;
            return equals;
        }

        static bool FirstPersonMeshAnnotationEquals(ModelDiffContext context, FirstPersonMeshAnnotation lhs, FirstPersonMeshAnnotation rhs)
        {
            var equals = true;
            if (!context.Enter("Node").Push(lhs.Node, rhs.Node, NodeEquals)) equals = false;
            if (!context.Enter("Flag").Push(lhs.FirstPersonFlag, rhs.FirstPersonFlag)) equals = false;
            return equals;
        }

        static void VrmFirstPerson(ModelDiffContext context, FirstPerson lhs, FirstPerson rhs)
        {
            // context.Enter("HeadNode").Push(lhs.m_fp, rhs.m_fp, NodeEquals);
            ListDiff(context.Enter("Annotations"), lhs.Annotations, rhs.Annotations, FirstPersonMeshAnnotationEquals);
        }

        static void VrmLookAt(ModelDiffContext context, LookAt lhs, LookAt rhs)
        {
            context.Enter("Offset").Push(lhs.OffsetFromHeadBone, rhs.OffsetFromHeadBone, Vector3NearlyEquals);
            context.Enter(nameof(lhs.LookAtType)).Push(lhs.LookAtType, rhs.LookAtType);
            VrmLookAtRangeMap(context.Enter(nameof(lhs.HorizontalInner)), lhs.HorizontalInner, rhs.HorizontalInner);
            VrmLookAtRangeMap(context.Enter(nameof(lhs.HorizontalOuter)), lhs.HorizontalOuter, rhs.HorizontalOuter);
            VrmLookAtRangeMap(context.Enter(nameof(lhs.VerticalUp)), lhs.VerticalUp, rhs.VerticalUp);
            VrmLookAtRangeMap(context.Enter(nameof(lhs.VerticalDown)), lhs.VerticalDown, rhs.VerticalDown);
        }

        static void VrmLookAtRangeMap(ModelDiffContext context, LookAtRangeMap lhs, LookAtRangeMap rhs)
        {
            context.Enter(nameof(lhs.InputMaxValue)).Push(lhs.InputMaxValue, rhs.InputMaxValue);
            context.Enter(nameof(lhs.OutputScaling)).Push(lhs.OutputScaling, rhs.OutputScaling);
            context.Enter("Curve").Push(lhs.Curve, rhs.Curve);
        }

        static bool VrmSpringBoneEquals(ModelDiffContext context, SpringBone lhs, SpringBone rhs)
        {
            var equals = true;
            if (!context.Enter("Comment").Push(lhs.Comment, rhs.Comment)) equals = false;
            if (!context.Enter("DragForce").Push(lhs.DragForce, rhs.DragForce)) equals = false;
            if (!context.Enter("GravityDir").Push(lhs.GravityDir, rhs.GravityDir)) equals = false;
            if (!context.Enter("GravityPower").Push(lhs.GravityPower, rhs.GravityPower)) equals = false;
            if (!context.Enter("HitRadius").Push(lhs.HitRadius, rhs.HitRadius)) equals = false;
            if (!context.Enter("Origin").Push(lhs.Origin, rhs.Origin)) equals = false;
            if (!context.Enter("Stiffness").Push(lhs.Stiffness, rhs.Stiffness)) equals = false;
            return equals;
        }

        static bool VrmSpringBoneColliderEquals(ModelDiffContext context, VrmSpringBoneCollider lhs, VrmSpringBoneCollider rhs)
        {
            var equals = true;
            if (!context.Enter("Offset").Push(lhs.Offset, rhs.Offset)) equals = false;
            if (!context.Enter("Radius").Push(lhs.Radius, rhs.Radius)) equals = false;
            return equals;
        }

        static bool VrmSpringBoneColliderEquals(ModelDiffContext context, SpringBoneColliderGroup lhs, SpringBoneColliderGroup rhs)
        {
            var equals = true;
            if (!context.Enter("Node").Push(lhs.Node, rhs.Node, NodeEquals)) equals = false;
            if (!ListDiff(context.Enter("Colliders"), lhs.Colliders, rhs.Colliders, VrmSpringBoneColliderEquals)) equals = false;
            return equals;
        }
        #endregion
    }
}
