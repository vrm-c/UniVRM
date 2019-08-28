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

        public VRMExporter(glTF gltf) : base(gltf)
        {
            gltf.extensionsUsed.Add(glTF_VRM_extensions.ExtensionName);
            gltf.extensions.VRM = new glTF_VRM_extensions();
        }

        public new static glTF Export(GameObject go, bool exportOnlyBlendShapePosition = false)
        {
            var gltf = new glTF();

            using (var exporter = new VRMExporter(gltf)
            {
#if VRM_EXPORTER_USE_SPARSE
                // experimental
                UseSparseAccessorForBlendShape = true
#endif
                ExportOnlyBlendShapePosition = exportOnlyBlendShapePosition
            })
            {
                _Export(gltf, exporter, go);
            }

            return gltf;
        }

        public static void _Export(glTF gltf, VRMExporter exporter, GameObject go)
        {
            exporter.Prepare(go);
            exporter.Export();

            // avatar
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                var humanoid = go.GetComponent<VRMHumanoidDescription>();
                UniHumanoid.AvatarDescription description = null;
                var nodes = go.transform.Traverse().Skip(1).ToList();
                {
                    var isCreated = false;
                    if (humanoid != null)
                    {
                        description = humanoid.GetDescription(out isCreated);
                    }

                    if (description != null)
                    {
                        // use description
                        gltf.extensions.VRM.humanoid.Apply(description, nodes);
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
                            gltf.extensions.VRM.humanoid.SetNodeIndex(key, nodes.IndexOf(transform));
                        }
                    }
                }
            }

            // morph
            var master = go.GetComponent<VRMBlendShapeProxy>();
            if (master != null)
            {
                var avatar = master.BlendShapeAvatar;
                if (avatar != null)
                {
                    var meshes = exporter.Meshes;
                    foreach (var x in avatar.Clips)
                    {
                        gltf.extensions.VRM.blendShapeMaster.Add(x, exporter.Copy.transform, meshes);
                    }
                }
            }

            // secondary
            VRMSpringUtility.ExportSecondary(exporter.Copy.transform, exporter.Nodes,
                x => gltf.extensions.VRM.secondaryAnimation.colliderGroups.Add(x),
                x => gltf.extensions.VRM.secondaryAnimation.boneGroups.Add(x)
                );

#pragma warning disable 0618
            // meta(obsolete)
            {
                var meta = exporter.Copy.GetComponent<VRMMetaInformation>();
                if (meta != null)
                {
                    gltf.extensions.VRM.meta.author = meta.Author;
                    gltf.extensions.VRM.meta.contactInformation = meta.ContactInformation;
                    gltf.extensions.VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        gltf.extensions.VRM.meta.texture = TextureIO.ExportTexture(gltf, gltf.buffers.Count - 1, meta.Thumbnail, glTFTextureTypes.Unknown);
                    }

                    gltf.extensions.VRM.meta.licenseType = meta.LicenseType;
                    gltf.extensions.VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    gltf.extensions.VRM.meta.reference = meta.Reference;
                }
            }
#pragma warning restore 0618

            // meta
            {
                var _meta = exporter.Copy.GetComponent<VRMMeta>();
                if (_meta != null && _meta.Meta != null)
                {
                    var meta = _meta.Meta;

                    // info
                    gltf.extensions.VRM.meta.version = meta.Version;
                    gltf.extensions.VRM.meta.author = meta.Author;
                    gltf.extensions.VRM.meta.contactInformation = meta.ContactInformation;
                    gltf.extensions.VRM.meta.reference = meta.Reference;
                    gltf.extensions.VRM.meta.title = meta.Title;
                    if (meta.Thumbnail != null)
                    {
                        gltf.extensions.VRM.meta.texture = TextureIO.ExportTexture(gltf, gltf.buffers.Count - 1, meta.Thumbnail, glTFTextureTypes.Unknown);
                    }

                    // ussage permission
                    gltf.extensions.VRM.meta.allowedUser = meta.AllowedUser;
                    gltf.extensions.VRM.meta.violentUssage = meta.ViolentUssage;
                    gltf.extensions.VRM.meta.sexualUssage = meta.SexualUssage;
                    gltf.extensions.VRM.meta.commercialUssage = meta.CommercialUssage;
                    gltf.extensions.VRM.meta.otherPermissionUrl = meta.OtherPermissionUrl;

                    // distribution license
                    gltf.extensions.VRM.meta.licenseType = meta.LicenseType;
                    if (meta.LicenseType == LicenseType.Other)
                    {
                        gltf.extensions.VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    }
                }
            }

            // firstPerson
            var firstPerson = exporter.Copy.GetComponent<VRMFirstPerson>();
            if (firstPerson != null)
            {
                if (firstPerson.FirstPersonBone != null)
                {
                    gltf.extensions.VRM.firstPerson.firstPersonBone = exporter.Nodes.IndexOf(firstPerson.FirstPersonBone);
                    gltf.extensions.VRM.firstPerson.firstPersonBoneOffset = firstPerson.FirstPersonOffset;
                    gltf.extensions.VRM.firstPerson.meshAnnotations = firstPerson.Renderers.Select(x => new glTF_VRM_MeshAnnotation
                    {
                        mesh = exporter.Meshes.IndexOf(x.SharedMesh),
                        firstPersonFlag = x.FirstPersonFlag.ToString(),
                    }).ToList();
                }

                // lookAt
                {
                    var lookAtHead = exporter.Copy.GetComponent<VRMLookAtHead>();
                    if (lookAtHead != null)
                    {
                        var boneApplyer = exporter.Copy.GetComponent<VRMLookAtBoneApplyer>();
                        var blendShapeApplyer = exporter.Copy.GetComponent<VRMLookAtBlendShapeApplyer>();
                        if (boneApplyer != null)
                        {
                            gltf.extensions.VRM.firstPerson.lookAtType = LookAtType.Bone;
                            gltf.extensions.VRM.firstPerson.lookAtHorizontalInner.Apply(boneApplyer.HorizontalInner);
                            gltf.extensions.VRM.firstPerson.lookAtHorizontalOuter.Apply(boneApplyer.HorizontalOuter);
                            gltf.extensions.VRM.firstPerson.lookAtVerticalDown.Apply(boneApplyer.VerticalDown);
                            gltf.extensions.VRM.firstPerson.lookAtVerticalUp.Apply(boneApplyer.VerticalUp);
                        }
                        else if (blendShapeApplyer != null)
                        {
                            gltf.extensions.VRM.firstPerson.lookAtType = LookAtType.BlendShape;
                            gltf.extensions.VRM.firstPerson.lookAtHorizontalOuter.Apply(blendShapeApplyer.Horizontal);
                            gltf.extensions.VRM.firstPerson.lookAtVerticalDown.Apply(blendShapeApplyer.VerticalDown);
                            gltf.extensions.VRM.firstPerson.lookAtVerticalUp.Apply(blendShapeApplyer.VerticalUp);
                        }
                    }
                }
            }

            // materials
            foreach (var m in exporter.Materials)
            {
                gltf.extensions.VRM.materialProperties.Add(VRMMaterialExporter.CreateFromMaterial(m, exporter.TextureManager.Textures));
            }
        }
    }
}