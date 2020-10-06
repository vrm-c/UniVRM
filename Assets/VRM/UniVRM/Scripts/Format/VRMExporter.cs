using System;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public class VRMExporter : gltfExporter
    {
        protected override IMaterialExporter CreateMaterialExporter()
        {
            return new VRMMaterialExporter();
        }

        public static glTF Export(MeshExportSettings configuration, GameObject go)
        {
            var gltf = new glTF();
            using (var exporter = new VRMExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export(configuration);
            }
            return gltf;
        }
        
        public VRMExporter(glTF gltf) : base(gltf)
        {
            gltf.extensionsUsed.Add(glTF_VRM_extensions.ExtensionName);
            gltf.extensions.VRM = new glTF_VRM_extensions();
        }

        public override void Export(MeshExportSettings configuration)
        {
            base.Export(configuration);

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
                        glTF.extensions.VRM.humanoid.Apply(description, nodes);
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
                            glTF.extensions.VRM.humanoid.SetNodeIndex(key, nodes.IndexOf(transform));
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
                        glTF.extensions.VRM.blendShapeMaster.Add(x, this);
                    }
                }
            }

            // secondary
            VRMSpringUtility.ExportSecondary(Copy.transform, Nodes,
                x => glTF.extensions.VRM.secondaryAnimation.colliderGroups.Add(x),
                x => glTF.extensions.VRM.secondaryAnimation.boneGroups.Add(x)
                );

#pragma warning disable 0618
            // meta(obsolete)
            {
                var meta = Copy.GetComponent<VRMMetaInformation>();
                if (meta != null)
                {
                    glTF.extensions.VRM.meta.author = meta.Author;
                    glTF.extensions.VRM.meta.contactInformation = meta.ContactInformation;
                    glTF.extensions.VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        glTF.extensions.VRM.meta.texture = TextureIO.ExportTexture(glTF, glTF.buffers.Count - 1, meta.Thumbnail, glTFTextureTypes.Unknown);
                    }

                    glTF.extensions.VRM.meta.licenseType = meta.LicenseType;
                    glTF.extensions.VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    glTF.extensions.VRM.meta.reference = meta.Reference;
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
                    glTF.extensions.VRM.meta.version = meta.Version;
                    glTF.extensions.VRM.meta.author = meta.Author;
                    glTF.extensions.VRM.meta.contactInformation = meta.ContactInformation;
                    glTF.extensions.VRM.meta.reference = meta.Reference;
                    glTF.extensions.VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        glTF.extensions.VRM.meta.texture = TextureIO.ExportTexture(glTF, glTF.buffers.Count - 1, meta.Thumbnail, glTFTextureTypes.Unknown);
                    }

                    // ussage permission
                    glTF.extensions.VRM.meta.allowedUser = meta.AllowedUser;
                    glTF.extensions.VRM.meta.violentUssage = meta.ViolentUssage;
                    glTF.extensions.VRM.meta.sexualUssage = meta.SexualUssage;
                    glTF.extensions.VRM.meta.commercialUssage = meta.CommercialUssage;
                    glTF.extensions.VRM.meta.otherPermissionUrl = meta.OtherPermissionUrl;

                    // distribution license
                    glTF.extensions.VRM.meta.licenseType = meta.LicenseType;
                    if (meta.LicenseType == LicenseType.Other)
                    {
                        glTF.extensions.VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    }
                }
            }

            // firstPerson
            var firstPerson = Copy.GetComponent<VRMFirstPerson>();
            if (firstPerson != null)
            {
                if (firstPerson.FirstPersonBone != null)
                {
                    glTF.extensions.VRM.firstPerson.firstPersonBone = Nodes.IndexOf(firstPerson.FirstPersonBone);
                    glTF.extensions.VRM.firstPerson.firstPersonBoneOffset = firstPerson.FirstPersonOffset;
                    glTF.extensions.VRM.firstPerson.meshAnnotations = firstPerson.Renderers.Select(x => new glTF_VRM_MeshAnnotation
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
                            glTF.extensions.VRM.firstPerson.lookAtType = LookAtType.Bone;
                            glTF.extensions.VRM.firstPerson.lookAtHorizontalInner.Apply(boneApplyer.HorizontalInner);
                            glTF.extensions.VRM.firstPerson.lookAtHorizontalOuter.Apply(boneApplyer.HorizontalOuter);
                            glTF.extensions.VRM.firstPerson.lookAtVerticalDown.Apply(boneApplyer.VerticalDown);
                            glTF.extensions.VRM.firstPerson.lookAtVerticalUp.Apply(boneApplyer.VerticalUp);
                        }
                        else if (blendShapeApplyer != null)
                        {
                            glTF.extensions.VRM.firstPerson.lookAtType = LookAtType.BlendShape;
                            glTF.extensions.VRM.firstPerson.lookAtHorizontalOuter.Apply(blendShapeApplyer.Horizontal);
                            glTF.extensions.VRM.firstPerson.lookAtVerticalDown.Apply(blendShapeApplyer.VerticalDown);
                            glTF.extensions.VRM.firstPerson.lookAtVerticalUp.Apply(blendShapeApplyer.VerticalUp);
                        }
                    }
                }
            }

            // materials
            foreach (var m in Materials)
            {
                glTF.extensions.VRM.materialProperties.Add(VRMMaterialExporter.CreateFromMaterial(m, TextureManager.Textures));
            }
        }
    }
}
