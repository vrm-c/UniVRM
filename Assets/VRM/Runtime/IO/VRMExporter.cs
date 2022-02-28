using System;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace VRM
{
    public class VRMExporter : gltfExporter
    {
        public const Axes Vrm0xSpecificationInverseAxis = Axes.Z;

        public static ExportingGltfData Export(GltfExportSettings configuration, GameObject go, ITextureSerializer textureSerializer)
        {
            var data = new ExportingGltfData();
            using (var exporter = new VRMExporter(data, configuration))
            {
                exporter.Prepare(go);
                exporter.Export(textureSerializer);
            }
            return data;
        }

        public readonly VRM.glTF_VRM_extensions VRM = new glTF_VRM_extensions();

        public VRMExporter(ExportingGltfData data, GltfExportSettings exportSettings) : base(data, exportSettings)
        {
            if (exportSettings == null || exportSettings.InverseAxis != Vrm0xSpecificationInverseAxis)
            {
                throw new Exception($"VRM specification requires InverseAxis settings as {Vrm0xSpecificationInverseAxis}");
            }

            _gltf.extensionsUsed.Add(glTF_VRM_extensions.ExtensionName);
        }

        protected override IMaterialExporter CreateMaterialExporter()
        {
            return new VRMMaterialExporter();
        }

        public override void ExportExtensions(ITextureSerializer textureSerializer)
        {
            // avatar
            var animator = Copy.GetComponent<Animator>();
            if (animator != null)
            {
                var humanoid = Copy.GetComponent<VRMHumanoidDescription>();
                UniHumanoid.AvatarDescription description = null;
                var nodes = Copy.transform.Traverse().Skip(1).ToList();
                {
                    var isCreated = false;
                    if (humanoid != null)
                    {
                        description = humanoid.GetDescription(out isCreated);
                    }

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

                {
                    // set humanoid bone mapping
                    var avatar = animator.avatar;
                    foreach (HumanBodyBones key in Enum.GetValues(typeof(HumanBodyBones)))
                    {
                        if (key == HumanBodyBones.LastBone)
                        {
                            break;
                        }

                        var transform = animator.GetBoneTransform(key);
                        if (transform != null)
                        {
                            VRM.humanoid.SetNodeIndex(key, nodes.IndexOf(transform));
                        }
                    }
                }
            }

            // morph
            var master = Copy.GetComponent<VRMBlendShapeProxy>();
            if (master != null)
            {
                var avatar = master.BlendShapeAvatar;
                if (avatar != null)
                {
                    foreach (var x in avatar.Clips)
                    {
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
                var meta = Copy.GetComponent<VRMMetaInformation>();
                if (meta != null)
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
                var _meta = Copy.GetComponent<VRMMeta>();
                if (_meta != null && _meta.Meta != null)
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
            var firstPerson = Copy.GetComponent<VRMFirstPerson>();
            if (firstPerson != null)
            {
                if (firstPerson.FirstPersonBone != null)
                {
                    VRM.firstPerson.firstPersonBone = Nodes.IndexOf(firstPerson.FirstPersonBone);
                    VRM.firstPerson.firstPersonBoneOffset = firstPerson.FirstPersonOffset;
                    VRM.firstPerson.meshAnnotations = firstPerson.Renderers.Select(x => new glTF_VRM_MeshAnnotation
                    {
                        mesh = Meshes.IndexOf(x.SharedMesh),
                        firstPersonFlag = x.FirstPersonFlag.ToString(),
                    }).ToList();
                }

                // lookAt
                {
                    var lookAtHead = Copy.GetComponent<VRMLookAtHead>();
                    if (lookAtHead != null)
                    {
                        var boneApplyer = Copy.GetComponent<VRMLookAtBoneApplyer>();
                        var blendShapeApplyer = Copy.GetComponent<VRMLookAtBlendShapeApplyer>();
                        if (boneApplyer != null)
                        {
                            VRM.firstPerson.lookAtType = LookAtType.Bone;
                            VRM.firstPerson.lookAtHorizontalInner.Apply(boneApplyer.HorizontalInner);
                            VRM.firstPerson.lookAtHorizontalOuter.Apply(boneApplyer.HorizontalOuter);
                            VRM.firstPerson.lookAtVerticalDown.Apply(boneApplyer.VerticalDown);
                            VRM.firstPerson.lookAtVerticalUp.Apply(boneApplyer.VerticalUp);
                        }
                        else if (blendShapeApplyer != null)
                        {
                            VRM.firstPerson.lookAtType = LookAtType.BlendShape;
                            VRM.firstPerson.lookAtHorizontalOuter.Apply(blendShapeApplyer.Horizontal);
                            VRM.firstPerson.lookAtVerticalDown.Apply(blendShapeApplyer.VerticalDown);
                            VRM.firstPerson.lookAtVerticalUp.Apply(blendShapeApplyer.VerticalUp);
                        }
                    }
                }
            }

            // materials
            foreach (var m in Materials)
            {
                VRM.materialProperties.Add(VRMMaterialExporter.CreateFromMaterial(m, TextureExporter));
            }

            // Serialize VRM
            var f = new JsonFormatter();
            VRMSerializer.Serialize(f, VRM);
            var bytes = f.GetStoreBytes();
            glTFExtensionExport.GetOrCreate(ref _gltf.extensions).Add("VRM", bytes);
        }
    }
}
