using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UniGLTF;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UniTask;


namespace VRM
{
    public static class VRMImporter
    {
        const string HUMANOID_KEY = "humanoid";
        const string MATERIAL_KEY = "materialProperties";

        public static VRMImporterContext LoadFromPath(string path)
        {
            return LoadFromBytes(File.ReadAllBytes(path), path);
        }

        public static VRMImporterContext LoadFromBytes(byte[] bytes, string path=null)
        {
            var context = new VRMImporterContext();
            if (!string.IsNullOrEmpty(path))
            {
                context.Path = path;
            }

            var chunks = glbImporter.ParseGlbChanks(bytes);

            if (chunks.Count != 2)
            {
                throw new Exception("unknown chunk count: " + chunks.Count);
            }

            if (chunks[0].ChunkType != GlbChunkType.JSON)
            {
                throw new Exception("chunk 0 is not JSON");
            }

            if (chunks[1].ChunkType != GlbChunkType.BIN)
            {
                throw new Exception("chunk 1 is not BIN");
            }

            var jsonBytes = chunks[0].Bytes;
            var json = Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count);

            context.Json = json;
            context.CreateMaterial = VRMImporter.GetMaterialFunc(glTF_VRM_Material.Parse(json));

            gltfImporter.Import<glTF_VRM>(context, chunks[1].Bytes);
            if (string.IsNullOrEmpty(context.Path))
            {
                if (string.IsNullOrEmpty(context.VRM.extensions.VRM.meta.title))
                {
                    context.Root.name = "VRM_LOADED";
                }
                else
                {
                    context.Root.name = context.VRM.extensions.VRM.meta.title;
                }
            }
            else
            {
                context.Root.name = Path.GetFileNameWithoutExtension(context.Path);
            }

            OnLoadModel(context);

            context.ShowMeshes();

            return context;
        }

        public static CreateMaterialFunc GetMaterialFunc(List<glTF_VRM_Material> materials)
        {
            var fallback = gltfImporter.CreateMaterialFuncFromShader(Shader.Find("Standard"));
            if (materials == null && materials.Count == 0)
            {
                return fallback;
            }

            return (ctx, i) =>
            {
                var item = materials[i];
                var shaderName = item.shader;
                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogErrorFormat("shader {0} not found. set Assets/VRM/Shaders/VRMShaders to Edit - project setting - Graphics - preloaded shaders", shaderName);
                    return fallback(ctx, i);
                }
                else
                {
                    var material = new Material(shader);
                    material.name = item.name;
                    material.renderQueue = item.renderQueue;

                    foreach (var kv in item.floatProperties)
                    {
                        material.SetFloat(kv.Key, kv.Value);
                    }
                    foreach (var kv in item.vectorProperties)
                    {
                        var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                        material.SetVector(kv.Key, v);
                    }
                    foreach (var kv in item.textureProperties)
                    {
                        material.SetTexture(kv.Key, ctx.Textures[kv.Value].Texture);
                    }
                    foreach (var kv in item.keywordMap)
                    {
                        if (kv.Value)
                        {
                            material.EnableKeyword(kv.Key);
                        }
                        else
                        {
                            material.DisableKeyword(kv.Key);
                        }
                    }
                    foreach (var kv in item.tagMap)
                    {
                        material.SetOverrideTag(kv.Key, kv.Value);
                    }
                    return material;
                }
            };
        }

        #region OnLoad
        public static Unit OnLoadModel(VRMImporterContext context)
        {
            try
            {
                LoadHumanoidObsolete(context);
                Debug.LogWarning("LoadHumanoidObsolete");
            }
            catch (Exception ex)
            {
                LoadHumanoid(context);
            }

            LoadBlendShapeMaster(context);
            LoadSecondaryMotions(context);
            LoadFirstPerson(context);
            LoadMeta(context);

            return Unit.Default;
        }

        static void LoadMeta(VRMImporterContext context)
        {
            var meta = context.Root.AddComponent<VRMMetaInformation>();
            var gltfMeta = context.VRM.extensions.VRM.meta;
            meta.Author = gltfMeta.author;
            meta.ContactInformation = gltfMeta.contactInformation;
            meta.Title = gltfMeta.title;
            if (gltfMeta.texture != -1)
            {
                meta.Thumbnail = context.Textures[gltfMeta.texture].Texture;
            }
            else
            {
                // 作る
                var lookAt = context.Root.GetComponent<VRMLookAtHead>();
                var thumbnail = lookAt.CreateThumbnail();
                thumbnail.name = "thumbnail";
                meta.Thumbnail = thumbnail;
                //context.Textures.Add(new TextureItem(thumbnail));
            }

            meta.LicenseType = gltfMeta.licenseType;
            meta.OtherLicenseUrl = gltfMeta.otherLicenseUrl;
            meta.Reference = gltfMeta.reference;
        }

        static void LoadFirstPerson(VRMImporterContext context)
        {
            var firstPerson = context.Root.AddComponent<VRMFirstPerson>();

            var gltfFirstPerson = context.VRM.extensions.VRM.firstPerson;
            if (gltfFirstPerson.firstPersonBone != -1)
            {
                firstPerson.FirstPersonBone = context.Nodes[gltfFirstPerson.firstPersonBone];
                firstPerson.FirstPersonOffset = gltfFirstPerson.firstPersonBoneOffset;
            }
            else
            {
                // fallback
                firstPerson.SetDefault();
            }
            firstPerson.TraverseRenderers(context);

            // LookAt
            var lookAtHead = context.Root.AddComponent<VRMLookAtHead>();

            var lookAt = context.Root.AddComponent<VRMLookAtBoneApplyer>();
            lookAt.HorizontalInner.Apply(gltfFirstPerson.lookAtHorizontalInner);
            lookAt.HorizontalOuter.Apply(gltfFirstPerson.lookAtHorizontalOuter);
            lookAt.VerticalDown.Apply(gltfFirstPerson.lookAtVerticalDown);
            lookAt.VerticalUp.Apply(gltfFirstPerson.lookAtVerticalUp);
            //lookAt.GetBones();
        }

        static void LoadSecondaryMotions(VRMImporterContext context)
        {
            var secondary = context.Root.transform.Find("secondary");
            if (secondary == null)
            {
                secondary = new GameObject("secondary").transform;
                secondary.SetParent(context.Root.transform, false);
            }

            var secondaryAnimation = context.VRM.extensions.VRM.secondaryAnimation;
            var colliders = new List<VRMSpringBoneColliderGroup>();
            foreach (var colliderGroup in secondaryAnimation.colliderGroups)
            {
                var vrmGroup = context.Nodes[colliderGroup.node].gameObject.AddComponent<VRMSpringBoneColliderGroup>();
                vrmGroup.Colliders = colliderGroup.colliders.Select(x =>
                {
                    return new VRMSpringBoneColliderGroup.SphereCollider
                    {
                        Offset = x.offset,
                        Radius = x.radius
                    };
                }).ToArray();
                colliders.Add(vrmGroup);
            }

            if (secondaryAnimation.boneGroups.Count > 0)
            {
                foreach (var boneGroup in secondaryAnimation.boneGroups)
                {
                    var vrmBoneGroup = secondary.gameObject.AddComponent<VRMSpringBone>();
                    if (boneGroup.center != -1)
                    {
                        vrmBoneGroup.m_center = context.Nodes[boneGroup.center];
                    }
                    vrmBoneGroup.m_comment = boneGroup.comment;
                    vrmBoneGroup.m_dragForce = boneGroup.dragForce;
                    vrmBoneGroup.m_gravityDir = boneGroup.gravityDir;
                    vrmBoneGroup.m_gravityPower = boneGroup.gravityPower;
                    vrmBoneGroup.m_hitRadius = boneGroup.hitRadius;
                    vrmBoneGroup.m_stiffnessForce = boneGroup.stiffiness;
                    if (boneGroup.colliderGroups != null && boneGroup.colliderGroups.Any())
                    {
                        vrmBoneGroup.ColliderGroups = boneGroup.colliderGroups.Select(x => colliders[x]).ToArray();
                    }
                    vrmBoneGroup.RootBones = boneGroup.bones.Select(x => context.Nodes[x]).ToList();
                }
            }
            else
            {
                secondary.gameObject.AddComponent<VRMSpringBone>();
            }
        }

        static void LoadBlendShapeMaster(VRMImporterContext context)
        {
            context.BlendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            context.BlendShapeAvatar.name = "BlendShape";

            var blendShapeList = context.VRM.extensions.VRM.blendShapeMaster.blendShapeGroups;
            if (blendShapeList != null && blendShapeList.Count > 0)
            {
                foreach (var x in blendShapeList)
                {
                    context.BlendShapeAvatar.Clips.Add(LoadBlendShapeBind(context, x));
                }
            }

            var proxy = context.Root.AddComponent<VRMBlendShapeProxy>();
            context.BlendShapeAvatar.CreateDefaultPreset();
            proxy.BlendShapeAvatar = context.BlendShapeAvatar;
        }

        private static BlendShapeClip LoadBlendShapeBind(VRMImporterContext context,
            glTF_VRM_BlendShapeGroup group)
        {
            var asset = ScriptableObject.CreateInstance<BlendShapeClip>();
            var groupName = group.name;
            var prefix = "BlendShape.";
            while (groupName.StartsWith(prefix))
            {
                groupName = groupName.Substring(prefix.Length);
            }
            asset.name = "BlendShape." + groupName;

            if (group != null)
            {
                asset.BlendShapeName = groupName;
                asset.Preset = group.presetName.ToBlendShapePreset();
                if (asset.Preset == BlendShapePreset.Unknown)
                {
                    // fallback
                    asset.Preset = group.name.ToBlendShapePreset();
                }
                asset.Values = group.binds.Select(x =>
                {
                    var mesh = context.Meshes[x.mesh].Mesh;
                    var node = context.Root.transform.Traverse().First(y => y.GetSharedMesh() == mesh);
                    var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(node, context.Root.transform);
                    return new BlendShapeBinding
                    {
                        //Name = name,
                        RelativePath = relativePath,
                        Index = x.index,
                        Weight = x.weight,
                    };
                })
                .ToArray();
            }

            return asset;
        }

        static String ToHumanBoneName(HumanBodyBones b)
        {
            foreach (var x in HumanTrait.BoneName)
            {
                if (x.Replace(" ", "") == b.ToString())
                {
                    return x;
                }
            }

            throw new KeyNotFoundException();
        }

        static SkeletonBone ToSkeletonBone(Transform t)
        {
            var sb = new SkeletonBone();
            sb.name = t.name;
            sb.position = t.localPosition;
            sb.rotation = t.localRotation;
            sb.scale = t.localScale;
            return sb;
        }

        [Obsolete]
        private static void LoadHumanoidObsolete(VRMImporterContext context)
        {
            var parsed = context.Json.ParseAsJson()["extensions"]["VRM"];
            var skeleton = context.Root.transform.Traverse().Select(x => ToSkeletonBone(x)).ToArray();

            var description = new HumanDescription
            {
                human = parsed[HUMANOID_KEY]["bones"]
                .ObjectItems
                .Select(x => new { x.Key, Index = x.Value.GetInt32() })
                .Where(x => x.Index != -1)
                .Select(x =>
                {
                    var humanBone = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), x.Key.ToUpperCamelCase());
                    var hb = new HumanBone
                    {
                        boneName = context.Nodes[x.Index].name,
                        humanName = ToHumanBoneName(humanBone)
                    };
                    hb.limit.useDefaultValues = true;
                    return hb;
                }).ToArray(),
                skeleton = skeleton,
                lowerArmTwist = 0.5f,
                upperArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0.0f,
            };

            context.HumanoidAvatar = AvatarBuilder.BuildHumanAvatar(context.Root, description);
            context.HumanoidAvatar.name = Path.GetFileNameWithoutExtension(context.Path);

            context.AvatarDescription = UniHumanoid.AvatarDescription.CreateFrom(description);
            context.AvatarDescription.name = "AvatarDescription";
            var humanoid = context.Root.AddComponent<VRMHumanoidDescription>();
            humanoid.Avatar = context.HumanoidAvatar;
            humanoid.Description = context.AvatarDescription;

            var animator = context.Root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = context.Root.AddComponent<Animator>();
            }
            animator.avatar = context.HumanoidAvatar;
        }

        private static void LoadHumanoid(VRMImporterContext context)
        {
            context.AvatarDescription = context.VRM.extensions.VRM.humanoid.ToDescription(context.Nodes);
            context.AvatarDescription.name = "AvatarDescription";
            context.HumanoidAvatar = context.AvatarDescription.CreateAvatar(context.Root.transform);
            context.HumanoidAvatar.name = Path.GetFileNameWithoutExtension(context.Path);

            var humanoid = context.Root.AddComponent<VRMHumanoidDescription>();
            humanoid.Avatar = context.HumanoidAvatar;
            humanoid.Description = context.AvatarDescription;

            var animator = context.Root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = context.Root.AddComponent<Animator>();
            }
            animator.avatar = context.HumanoidAvatar;
        }
        #endregion

        #region LoadVrmAsync
        static IEnumerator LoadTextures(VRMImporterContext context)
        {
            foreach (var gltfTexture in context.GLTF.textures)
            {
                var x = UniGLTF.gltfImporter.ImportTexture(context.GLTF, gltfTexture.source);
                context.Textures.Add(x);
                yield return null;
            }
        }

        static IEnumerator LoadMaterials(VRMImporterContext context)
        {
            if (context.GLTF.materials == null || !context.GLTF.materials.Any())
            {
                context.Materials.Add(context.CreateMaterial(context, 0));
            }
            else
            {
                for (int i = 0; i < context.GLTF.materials.Count; ++i)
                {
                    context.Materials.Add(context.CreateMaterial(context, i));
                    yield return null;
                }
            }
        }

        static IEnumerator LoadMeshes(VRMImporterContext context)
        {
            for (int i = 0; i < context.GLTF.meshes.Count; ++i)
            {
                var meshWithMaterials = gltfImporter.ImportMesh(context, i);
                var mesh = meshWithMaterials.Mesh;
                if (string.IsNullOrEmpty(mesh.name))
                {
                    mesh.name = string.Format("UniGLTF import#{0}", i);
                }
                context.Meshes.Add(meshWithMaterials);

                yield return null;
            }
        }

        static IEnumerator LoadNodes(VRMImporterContext context)
        {
            foreach (var x in context.GLTF.nodes)
            {
                context.Nodes.Add(gltfImporter.ImportNode(x).transform);
                yield return null;
            }
        }

        static IEnumerator BuildHierarchy(VRMImporterContext context)
        {
            var nodes = new List<gltfImporter.TransformWithSkin>();
            for (int i = 0; i < context.Nodes.Count; ++i)
            {
                nodes.Add(gltfImporter.BuildHierarchy(context, i));
                yield return null;
            }

            gltfImporter.FixCoordinate(context, nodes);

            // skinning
            for (int i = 0; i < nodes.Count; ++i)
            {
                gltfImporter.SetupSkinning(context, nodes, i);
            }

            // connect root
            context.Root = new GameObject("_root_");
            foreach (var x in context.GLTF.rootnodes)
            {
                var t = nodes[x].Transform;
                t.SetParent(context.Root.transform, false);
            }
        }

        public static void LoadVrmAsync(Byte[] bytes, Action<GameObject> onLoaded)
        {
            var ctx = new VRMImporterContext();
            LoadVrmAsync(ctx, bytes, onLoaded);
        }

        public static void LoadVrmAsync(string path, Action<GameObject> onLoaded)
        {
            var ctx = new VRMImporterContext
            {
                Path = path,
            };

            var schedule = Schedulable.Create();

            schedule.AddTask(MainThreadDispatcher.Instance.ThreadScheduler, () =>
            {
                return File.ReadAllBytes(path);
            })
                .Subscribe(MainThreadDispatcher.Instance.UnityScheduler,
                bytes =>
                {
                    LoadVrmAsync(ctx, bytes, onLoaded);
                },
                ex =>
                {
                    ctx.Destroy(true);
                    Debug.LogErrorFormat("Error in scheduler: {0}", ex);
                })
                ;
        }

        public static void LoadVrmAsync(VRMImporterContext ctx, Byte[] bytes, Action<GameObject> onLoaded)
        {
            var schedule = Schedulable.Create();

            schedule
            .AddTask(MainThreadDispatcher.Instance.ThreadScheduler, () =>
            {
                return glbImporter.ParseGlbChanks(bytes);
            })
            .ContinueWith(MainThreadDispatcher.Instance.ThreadScheduler, chunks =>
            {
                if (chunks.Count != 2)
                {
                    throw new Exception("unknown chunk count: " + chunks.Count);
                }

                if (chunks[0].ChunkType != GlbChunkType.JSON)
                {
                    throw new Exception("chunk 0 is not JSON");
                }

                if (chunks[1].ChunkType != GlbChunkType.BIN)
                {
                    throw new Exception("chunk 1 is not BIN");
                }

                var jsonBytes = chunks[0].Bytes;
                ctx.Json = Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count);

                //var parsed = json.ParseAsJson();
                ctx.GLTF = JsonUtility.FromJson<glTF_VRM>(ctx.Json);
                if (ctx.GLTF.asset.version != "2.0")
                {
                    throw new VRMException("unknown gltf version {0}", ctx.GLTF.asset.version);
                }
                ctx.GLTF.baseDir = Path.GetDirectoryName(ctx.Path);
                foreach (var buffer in ctx.GLTF.buffers)
                {
                    buffer.OpenStorage(ctx.GLTF.baseDir, chunks[1].Bytes);
                }

                return Unit.Default;
            })
                .Subscribe(MainThreadDispatcher.Instance.UnityScheduler,
                _ =>
                {
                    BuildAsync(ctx, onLoaded);
                },
                ex =>
                {
                    ctx.Destroy(true);
                    Debug.LogErrorFormat("Error in scheduler: {0}", ex);
                })
                ;
        }

        static void BuildAsync(VRMImporterContext ctx, Action<GameObject> onLoaded)
        {
            var schedulable = Schedulable.Create();

            schedulable
                .AddTask(MainThreadDispatcher.Instance.ThreadScheduler, () =>
                {
                    return glTF_VRM_Material.Parse(ctx.Json);
                })
                .ContinueWith(MainThreadDispatcher.Instance.UnityScheduler, x =>
                {
                    // material function
                    ctx.CreateMaterial = VRMImporter.GetMaterialFunc(x);
                })
                .OnExecute(MainThreadDispatcher.Instance.UnityScheduler, parent =>
                {
                    // textures
                    for (int i = 0; i < ctx.GLTF.textures.Count; ++i)
                    {
                        var index = i;
                        parent.AddTask(MainThreadDispatcher.Instance.UnityScheduler,
                                () => gltfImporter.ImportTexture(ctx.GLTF, index))
                            .ContinueWith(MainThreadDispatcher.Instance.ThreadScheduler, x => ctx.Textures.Add(x));
                    }
                })
                .ContinueWithCoroutine(MainThreadDispatcher.Instance.UnityScheduler, () => LoadMaterials(ctx))
                .OnExecute(MainThreadDispatcher.Instance.UnityScheduler, parent =>
                {
                    // meshes
                    for (int i = 0; i < ctx.GLTF.meshes.Count; ++i)
                    {
                        var index = i;
                        parent.AddTask(MainThreadDispatcher.Instance.ThreadScheduler,
                                () => gltfImporter.ReadMesh(ctx, index))
                        .ContinueWith(MainThreadDispatcher.Instance.UnityScheduler, x => gltfImporter.BuildMesh(ctx, x))
                        .ContinueWith(MainThreadDispatcher.Instance.ThreadScheduler, x => ctx.Meshes.Add(x))
                        ;
                    }
                })
                .ContinueWithCoroutine(MainThreadDispatcher.Instance.UnityScheduler, () => LoadNodes(ctx))
                .ContinueWithCoroutine(MainThreadDispatcher.Instance.UnityScheduler, () => BuildHierarchy(ctx))
                .ContinueWith(MainThreadDispatcher.Instance.UnityScheduler, _ => VRMImporter.OnLoadModel(ctx))
                .Subscribe(MainThreadDispatcher.Instance.UnityScheduler,
                _ =>
            {
                Debug.LogFormat("task end: {0}/{1}/{2}/{3}",
                    ctx.Textures.Count,
                    ctx.Materials.Count,
                    ctx.Meshes.Count,
                    ctx.Nodes.Count
                    );
                ctx.Root.name = Path.GetFileNameWithoutExtension(ctx.Path);

                // 非表示のメッシュを表示する
                ctx.ShowMeshes();

                onLoaded(ctx.Root);
            }, ex =>
            {
                Debug.LogError(ex);
            })
            ;
        }
        #endregion
    }
}
