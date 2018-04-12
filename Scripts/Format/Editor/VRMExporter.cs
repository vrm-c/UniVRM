using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public class VRMExporter : gltfExporter
    {
        public VRMExporter(glTF_VRM gltf) : base(gltf)
        { }

        public static glTF Export(GameObject go, string path = null, Action<glTF_VRM> callback=null)
        {
            var gltf = new glTF_VRM();
            gltf.asset.generator = string.Format("UniVRM-{0}.{1}", VRMVersion.MAJOR, VRMVersion.MINOR);
            using (var exporter = new VRMExporter(gltf))
            {
                _Export(gltf, exporter, go);

                if (callback != null)
                {
                    callback(gltf);
                }

                if (!string.IsNullOrEmpty(path))
                {
                    exporter.WriteTo(path);
                }
            }
            return gltf;
        }

        public static void _Export(glTF_VRM gltf, VRMExporter exporter, GameObject go)
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
            var secondary = exporter.Copy.transform.Find("secondary");
            if (secondary == null)
            {
                secondary = exporter.Copy.transform;
            }

            var colliders = new List<VRMSpringBoneColliderGroup>();
            foreach (var vrmColliderGroup in exporter.Copy.transform.Traverse().Select(x => x.GetComponent<VRMSpringBoneColliderGroup>()).Where(x => x != null))
            {
                colliders.Add(vrmColliderGroup);

                var colliderGroup = new glTF_VRM_SecondaryAnimationColliderGroup
                {
                    node = exporter.Nodes.IndexOf(vrmColliderGroup.transform)
                };

                colliderGroup.colliders = vrmColliderGroup.Colliders.Select(x =>
                {
                    return new glTF_VRM_SecondaryAnimationCollider
                    {
                        offset = x.Offset,
                        radius = x.Radius,
                    };

                }).ToList();

                gltf.extensions.VRM.secondaryAnimation.colliderGroups.Add(colliderGroup);
            }

            foreach (var spring in secondary.GetComponents<VRMSpringBone>())
            {
                gltf.extensions.VRM.secondaryAnimation.boneGroups.Add(new glTF_VRM_SecondaryAnimationGroup
                {
                    comment = spring.m_comment,
                    center = exporter.Nodes.IndexOf(spring.m_center),
                    dragForce = spring.m_dragForce,
                    gravityDir = spring.m_gravityDir,
                    gravityPower = spring.m_gravityPower,
                    stiffiness = spring.m_stiffnessForce,
                    hitRadius = spring.m_hitRadius,
                    colliderGroups = spring.ColliderGroups
                        .Select(x =>
                        {
                            var index = colliders.IndexOf(x);
                            if (index == -1)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            return index;
                        })
                        .ToArray(),
                    bones = spring.RootBones.Select(x => exporter.Nodes.IndexOf(x)).ToArray(),
                });
            }

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
                        gltf.extensions.VRM.meta.texture = gltfExporter.ExportTexture(gltf, gltf.buffers.Count - 1, meta.Thumbnail);
                    }
                    gltf.extensions.VRM.meta.licenseType = meta.LicenseType;
                    gltf.extensions.VRM.meta.otherLicenseUrl = meta.OtherLicenseUrl;
                    gltf.extensions.VRM.meta.reference = meta.Reference;
                }
            }
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
                        gltf.extensions.VRM.meta.texture = gltfExporter.ExportTexture(gltf, gltf.buffers.Count - 1, meta.Thumbnail);
                    }

                    // ussage pemission
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
                    var lookAt = exporter.Copy.GetComponent<VRMLookAt>();
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
                    else if (lookAt != null)
                    {
                        gltf.extensions.VRM.firstPerson.lookAtHorizontalInner.Apply(lookAt.HorizontalInner);
                        gltf.extensions.VRM.firstPerson.lookAtHorizontalOuter.Apply(lookAt.HorizontalOuter);
                        gltf.extensions.VRM.firstPerson.lookAtVerticalDown.Apply(lookAt.VerticalDown);
                        gltf.extensions.VRM.firstPerson.lookAtVerticalUp.Apply(lookAt.VerticalUp);
                    }
                }
            }

            // materials
            foreach (var m in exporter.Materials)
            {
                gltf.extensions.VRM.materialProperties.Add(glTF_VRM_Material.CreateFromMaterial(m, exporter.Textures));
            }
        }
    }
}
