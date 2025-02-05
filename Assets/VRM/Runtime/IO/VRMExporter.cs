using System;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
using UniJSON;
using UnityEngine;

namespace VRM
{
    public class VRMExporter : gltfExporter
    {
        public const Axes Vrm0xSpecificationInverseAxis = Axes.Z;

        public static ExportingGltfData Export(GltfExportSettings configuration, GameObject go, ITextureSerializer textureSerializer)
        {
            var data = new ExportingGltfData();
            using (var exporter = new VRMExporter(data, configuration, textureSerializer: textureSerializer))
            {
                exporter.Prepare(go);
                exporter.Export();
            }
            return data;
        }

        public readonly VRM.glTF_VRM_extensions VRM = new glTF_VRM_extensions();

        public VRMExporter(
            ExportingGltfData data,
            GltfExportSettings exportSettings,
            IAnimationExporter animationExporter = null,
            IMaterialExporter materialExporter = null,
            ITextureSerializer textureSerializer = null
        ) : base(
            data,
            exportSettings,
            animationExporter: animationExporter,
            materialExporter: materialExporter ?? VrmMaterialExporterUtility.GetValidVrmMaterialExporter(),
            textureSerializer: textureSerializer
        )
        {
            if (exportSettings == null)
            {
                throw new Exception($"VRM specification requires InverseAxis settings as {Vrm0xSpecificationInverseAxis}");
            }
            if (exportSettings.InverseAxis != Vrm0xSpecificationInverseAxis)
            {
                // migration 用に reverseX を許す
                UniGLTFLogger.Warning($"VRM specification requires InverseAxis settings as {Vrm0xSpecificationInverseAxis}");
            }

            _gltf.extensionsUsed.Add(glTF_VRM_extensions.ExtensionName);
        }

        public override void ExportExtensions(ITextureSerializer textureSerializer)
        {
            var getBone = UniHumanoid.Humanoid.Get_GetBoneTransform(Copy);

            UniHumanoid.AvatarDescription description = null;
            var isCreated = false;
            if (Copy.TryGetComponent<VRMHumanoidDescription>(out var humanoid))
            {
                if (humanoid != null)
                {
                    description = humanoid.GetDescription(out isCreated);
                }
            }
            var nodes = Copy.transform.Traverse().Skip(1).ToList();
            {
                if (description != null)
                {
                    // use description
                    VRM.humanoid.Apply(description, nodes);
                }

                if (isCreated)
                {
                    GameObject.DestroyImmediate(description);
                }
            }

            // set humanoid bone mapping
            // var avatar = animator.avatar;
            foreach (HumanBodyBones key in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (key == HumanBodyBones.LastBone)
                {
                    break;
                }

                var transform = getBone(key);
                if (transform != null)
                {
                    var nodeIndex = nodes.IndexOf(transform);
                    if (nodeIndex < 0)
                    {
                        UniGLTFLogger.Error($"ヒューマンボーンが export 対象に含まれていない？", transform);
                    }
                    else
                    {
                        VRM.humanoid.SetNodeIndex(key, nodeIndex);
                    }
                }
            }

            // morph
            if (Copy.TryGetComponent<VRMBlendShapeProxy>(out var master))
            {
                var avatar = master.BlendShapeAvatar;
                if (avatar != null)
                {
                    foreach (var x in avatar.Clips)
                    {
                        if (x == null)
                        {
                            continue;
                        }
                        VRM.blendShapeMaster.Add(x, this);
                    }
                }
            }

            // secondary
            VRMSpringUtility.ExportSecondary(Copy.transform, Nodes,
                x => VRM.secondaryAnimation.colliderGroups.Add(x),
                x => VRM.secondaryAnimation.boneGroups.Add(x)
                );

#pragma warning disable 0618
            // meta(obsolete)
            {
                if (Copy.TryGetComponent<VRMMetaInformation>(out var meta))
                {
                    VRM.meta.author = meta.Author;
                    VRM.meta.contactInformation = meta.ContactInformation;
                    VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        VRM.meta.texture = TextureExporter.RegisterExportingAsSRgb(meta.Thumbnail, needsAlpha: true);
                    }

                    VRM.meta.licenseType = meta.LicenseType;
                    VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    VRM.meta.reference = meta.Reference;
                }
            }
#pragma warning restore 0618

            // meta
            {
                if (Copy.TryGetComponent<VRMMeta>(out var _meta) && _meta.Meta != null)
                {
                    var meta = _meta.Meta;

                    // info
                    VRM.meta.version = meta.Version;
                    VRM.meta.author = meta.Author;
                    VRM.meta.contactInformation = meta.ContactInformation;
                    VRM.meta.reference = meta.Reference;
                    VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        VRM.meta.texture = TextureExporter.RegisterExportingAsSRgb(meta.Thumbnail, needsAlpha: true);
                    }

                    // ussage permission
                    VRM.meta.allowedUser = meta.AllowedUser;
                    VRM.meta.violentUssage = meta.ViolentUssage;
                    VRM.meta.sexualUssage = meta.SexualUssage;
                    VRM.meta.commercialUssage = meta.CommercialUssage;
                    VRM.meta.otherPermissionUrl = meta.OtherPermissionUrl;

                    // distribution license
                    VRM.meta.licenseType = meta.LicenseType;
                    if (meta.LicenseType == LicenseType.Other)
                    {
                        VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    }
                }
            }

            // firstPerson
            if (Copy.TryGetComponent<VRMFirstPerson>(out var firstPerson))
            {
                if (firstPerson.FirstPersonBone != null)
                {
                    VRM.firstPerson.firstPersonBone = Nodes.IndexOf(firstPerson.FirstPersonBone);
                    VRM.firstPerson.firstPersonBoneOffset = firstPerson.FirstPersonOffset;
                    VRM.firstPerson.meshAnnotations = firstPerson.Renderers
                        .Select(x => new glTF_VRM_MeshAnnotation
                        {
                            mesh = Meshes.IndexOf(x.SharedMesh),
                            firstPersonFlag = x.FirstPersonFlag.ToString(),
                        })
                        .Where(x => x.mesh != -1)
                        .ToList();
                }

                // lookAt
                if (Copy.TryGetComponent<VRMLookAtHead>(out var lookAtHead))
                {
                    if (Copy.TryGetComponent<VRMLookAtBoneApplyer>(out var boneApplyer))
                    {
                        VRM.firstPerson.lookAtType = LookAtType.Bone;
                        VRM.firstPerson.lookAtHorizontalInner.Apply(boneApplyer.HorizontalInner);
                        VRM.firstPerson.lookAtHorizontalOuter.Apply(boneApplyer.HorizontalOuter);
                        VRM.firstPerson.lookAtVerticalDown.Apply(boneApplyer.VerticalDown);
                        VRM.firstPerson.lookAtVerticalUp.Apply(boneApplyer.VerticalUp);
                    }
                    else if (Copy.TryGetComponent<VRMLookAtBlendShapeApplyer>(out var blendShapeApplyer))
                    {
                        VRM.firstPerson.lookAtType = LookAtType.BlendShape;
                        VRM.firstPerson.lookAtHorizontalOuter.Apply(blendShapeApplyer.Horizontal);
                        VRM.firstPerson.lookAtVerticalDown.Apply(blendShapeApplyer.VerticalDown);
                        VRM.firstPerson.lookAtVerticalUp.Apply(blendShapeApplyer.VerticalUp);
                    }
                }
            }

            // materials
            foreach (var m in Materials)
            {
                VRM.materialProperties.Add(BuiltInVrmExtensionMaterialPropertyExporter.ExportMaterial(m, TextureExporter));
            }

            // Serialize VRM
            var f = new JsonFormatter();
            VRMSerializer.Serialize(f, VRM);
            var bytes = f.GetStoreBytes();
            glTFExtensionExport.GetOrCreate(ref _gltf.extensions).Add("VRM", bytes);
        }
    }
}
