using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UniGLTF;
using System.Collections.Generic;
using System.Collections;
using UniTask;
#if (NET_4_6 && UNITY_2017_1_OR_NEWER)
using System.Threading.Tasks;
#endif


namespace VRM
{
    public static class VRMImporter
    {
        const string HUMANOID_KEY = "humanoid";
        const string MATERIAL_KEY = "materialProperties";

        public static GameObject LoadFromPath(string path)
        {
            var context = new VRMImporterContext(UniGLTF.UnityPath.FromFullpath(path));
            context.ParseGlb(File.ReadAllBytes(path));
            LoadFromBytes(context);
            return context.Root;
        }

        public static GameObject LoadFromBytes(Byte[] bytes)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            LoadFromBytes(context);
            return context.Root;
        }

        public static void LoadFromBytes(VRMImporterContext context)
        {
            context.MaterialImporter = new VRMMaterialImporter(context, glTF_VRM_Material.Parse(context.Json));

            gltfImporter.Load(context);

            OnLoadModel(context);

            context.ShowMeshes();
        }


        #region OnLoad
        public static Unit OnLoadModel(VRMImporterContext context)
        {
            LoadMeta(context);

            try
            {
                LoadHumanoidObsolete(context);
                Debug.LogWarning("LoadHumanoidObsolete");
            }
            catch (Exception)
            {
                LoadHumanoid(context);
            }

            LoadBlendShapeMaster(context);
            VRMSpringUtility.LoadSecondary(context.Root.transform, context.Nodes,
                context.GLTF.extensions.VRM.secondaryAnimation);
            LoadFirstPerson(context);

            return Unit.Default;
        }

        static void LoadMeta(VRMImporterContext context)
        {
            var meta = context.ReadMeta();
            if (meta.Thumbnail == null)
            {
                /*
                // 作る
                var lookAt = context.Root.GetComponent<VRMLookAtHead>();
                var thumbnail = lookAt.CreateThumbnail();
                thumbnail.name = "thumbnail";
                meta.Thumbnail = thumbnail;
                context.Textures.Add(new TextureItem(thumbnail));
                */
            }
            var _meta = context.Root.AddComponent<VRMMeta>();
            _meta.Meta = meta;
            context.Meta = meta;
        }

        static void LoadFirstPerson(VRMImporterContext context)
        {
            var firstPerson = context.Root.AddComponent<VRMFirstPerson>();

            var gltfFirstPerson = context.GLTF.extensions.VRM.firstPerson;
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
            lookAtHead.OnImported(context);
        }

        static void LoadBlendShapeMaster(VRMImporterContext context)
        {
            context.BlendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            context.BlendShapeAvatar.name = "BlendShape";

            var blendShapeList = context.GLTF.extensions.VRM.blendShapeMaster.blendShapeGroups;
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
                asset.Preset = EnumUtil.TryParseOrDefault<BlendShapePreset>(group.presetName);
                if (asset.Preset == BlendShapePreset.Unknown)
                {
                    // fallback
                    asset.Preset = EnumUtil.TryParseOrDefault<BlendShapePreset>(group.name);
                }
                asset.Values = group.binds.Select(x =>
                {
                    var mesh = context.Meshes[x.mesh].Mesh;
                    var node = context.Root.transform.Traverse().First(y => y.GetSharedMesh() == mesh);
                    var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(node, context.Root.transform);
                    return new BlendShapeBinding
                    {
                        RelativePath = relativePath,
                        Index = x.index,
                        Weight = x.weight,
                    };
                })
                .ToArray();
                asset.MaterialValues = group.materialValues.Select(x =>
                {
                    var value = new Vector4();
                    for (int i = 0; i < x.targetValue.Length; ++i)
                    {
                        switch (i)
                        {
                            case 0: value.x = x.targetValue[0]; break;
                            case 1: value.y = x.targetValue[1]; break;
                            case 2: value.z = x.targetValue[2]; break;
                            case 3: value.w = x.targetValue[3]; break;
                        }
                    }
                    return new MaterialValueBinding
                    {
                        MaterialName = x.materialName,
                        ValueName = x.propertyName,
                        TargetValue = value,
                        BaseValue = context.Materials.First(y => y.name == x.materialName).GetColor(x.propertyName),
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
            var parsed = UniJSON.JsonParser.Parse(context.Json)["extensions"]["VRM"];
            var skeleton = context.Root.transform.Traverse().Select(x => ToSkeletonBone(x)).ToArray();

            var description = new HumanDescription
            {
                human = parsed[HUMANOID_KEY]["bones"]
                .ObjectItems
                .Select(x => new { x.Key, Index = x.Value.Value.GetInt32() })
                .Where(x => x.Index != -1)
                .Select(x =>
                {
                    var humanBone = EnumUtil.TryParseOrDefault<HumanBodyBones>(x.Key);
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
            context.HumanoidAvatar.name = "VrmAvatar";

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
            context.AvatarDescription = context.GLTF.extensions.VRM.humanoid.ToDescription(context.Nodes);
            context.AvatarDescription.name = "AvatarDescription";
            context.HumanoidAvatar = context.AvatarDescription.CreateAvatar(context.Root.transform);
            context.HumanoidAvatar.name = "VrmAvatar";

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
        static IEnumerator LoadTextures(VRMImporterContext context, IStorage storage)
        {
            for (int i = 0; i < context.GLTF.textures.Count; ++i)
            {
                var x = new TextureItem(context.GLTF, i);
                x.Process(context.GLTF, storage);
                context.Textures.Add(x);
                yield return null;
            }
        }

        static IEnumerator LoadMaterials(VRMImporterContext context)
        {
            if (context.GLTF.materials == null || !context.GLTF.materials.Any())
            {
                context.Materials.Add(context.MaterialImporter.CreateMaterial(context, 0));
            }
            else
            {
                for (int i = 0; i < context.GLTF.materials.Count; ++i)
                {
                    context.Materials.Add(context.MaterialImporter.CreateMaterial(context, i));
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
            }

            yield return null;
        }

        static IEnumerator BuildHierarchy(VRMImporterContext context)
        {
            var nodes = new List<gltfImporter.TransformWithSkin>();
            for (int i = 0; i < context.Nodes.Count; ++i)
            {
                nodes.Add(gltfImporter.BuildHierarchy(context, i));
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

            yield return null;
        }

#if (NET_4_6 && UNITY_2017_1_OR_NEWER)

        public static Task<GameObject> LoadVrmAsync(string path, bool show=true)
        {
            var context = new VRMImporterContext(UnityPath.FromFullpath(path));
            context.ParseGlb(File.ReadAllBytes(path));
            return LoadVrmAsyncInternal(context, show).ToTask();
        }


        public static Task<GameObject> LoadVrmAsync(Byte[] bytes, bool show=true)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            return LoadVrmAsync(context, show);
        }


        public static Task<GameObject> LoadVrmAsync(VRMImporterContext ctx, bool show=true)
        {
            return LoadVrmAsyncInternal(ctx, show).ToTask();
        }
#endif

        public static void LoadVrmAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            var context = new VRMImporterContext(UnityPath.FromFullpath(path));
            context.ParseGlb(File.ReadAllBytes(path));
            LoadVrmAsync(context, onLoaded, onError, show);
        }

        public static void LoadVrmAsync(Byte[] bytes, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            LoadVrmAsync(context, onLoaded, onError, show);
        }

        public static void LoadVrmAsync(VRMImporterContext ctx, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            if (onError == null)
            {
                onError = Debug.LogError;
            }
            LoadVrmAsyncInternal(ctx, show)
                .Subscribe(Scheduler.MainThread, onLoaded, onError);
        }

        private static Schedulable<GameObject> LoadVrmAsyncInternal(VRMImporterContext ctx, bool show)
        {
            var schedulable = Schedulable.Create();

            return schedulable
                .AddTask(Scheduler.ThreadPool, () =>
                {
                    return glTF_VRM_Material.Parse(ctx.Json);
                })
                .ContinueWith(Scheduler.MainThread, x =>
                {
                    // material function
                    ctx.MaterialImporter = new VRMMaterialImporter(ctx, x);
                })
                .OnExecute(Scheduler.ThreadPool, parent =>
                {
                    // textures
                    for (int i = 0; i < ctx.GLTF.textures.Count; ++i)
                    {
                        var index = i;
                        parent.AddTask(Scheduler.MainThread,
                                () =>
                                {
                                    var texture = new TextureItem(ctx.GLTF, index);
                                    texture.Process(ctx.GLTF, ctx.Storage);
                                    return texture;
                                })
                            .ContinueWith(Scheduler.ThreadPool, x => ctx.Textures.Add(x));
                    }
                })
                .ContinueWithCoroutine(Scheduler.MainThread, () => LoadMaterials(ctx))
                .OnExecute(Scheduler.ThreadPool, parent =>
                {
                    // meshes
                    for (int i = 0; i < ctx.GLTF.meshes.Count; ++i)
                    {
                        var index = i;
                        parent.AddTask(Scheduler.ThreadPool,
                                () => gltfImporter.ReadMesh(ctx, index))
                        .ContinueWith(Scheduler.MainThread, x => gltfImporter.BuildMesh(ctx, x))
                        .ContinueWith(Scheduler.ThreadPool, x => ctx.Meshes.Add(x))
                        ;
                    }
                })
                .ContinueWithCoroutine(Scheduler.MainThread, () => LoadNodes(ctx))
                .ContinueWithCoroutine(Scheduler.MainThread, () => BuildHierarchy(ctx))
                .ContinueWith(Scheduler.CurrentThread, _ => VRMImporter.OnLoadModel(ctx))
                .ContinueWith(Scheduler.CurrentThread,
                    _ =>
                    {
                        ctx.Root.name = "VRM";

                        if (show)
                        {
                            ctx.ShowMeshes();
                        }

                        return ctx.Root;
                    });
        }
        #endregion
    }
}
