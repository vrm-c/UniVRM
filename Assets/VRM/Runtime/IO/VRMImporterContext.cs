using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.Threading.Tasks;
using UniGLTF.Utils;
using VRMShaders;
using Object = UnityEngine.Object;

namespace VRM
{
    public class VRMImporterContext : ImporterContext
    {
        VRMData _data;
        public VRM.glTF_VRM_extensions VRM
        {
            get
            {
                return _data.VrmExtension;
            }
        }

        public VRMImporterContext(
            VRMData data,
            IReadOnlyDictionary<SubAssetKey, Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            bool loadAnimation = false)
            : base(data.Data, externalObjectMap, textureDeserializer, materialGenerator ?? new BuiltInVrmMaterialDescriptorGenerator(data.VrmExtension))
        {
            _data = data;
            TextureDescriptorGenerator = new VrmTextureDescriptorGenerator(Data, VRM);
            LoadAnimation = loadAnimation;
        }

        #region OnLoad
        protected override async Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            Root.name = "VRM";

            using (MeasureTime("VRM LoadMeta"))
            {
                await LoadMetaAsync(awaitCaller);
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadHumanoid"))
            {
                LoadHumanoid();
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadBlendShapeMaster"))
            {
                await LoadBlendShapeMaster(awaitCaller);
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadSecondary"))
            {
                VRMSpringUtility.LoadSecondary(Root.transform, TryGetNode,
                VRM.secondaryAnimation);
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadFirstPerson"))
            {
                await LoadFirstPerson(awaitCaller);
            }
        }

        async Task LoadMetaAsync(IAwaitCaller awaitCaller)
        {
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }
            var meta = await ReadMetaAsync(awaitCaller);
            var _meta = Root.AddComponent<VRMMeta>();
            _meta.Meta = meta;
            Meta = meta;
        }

        async Task LoadFirstPerson(IAwaitCaller awaitCaller)
        {
            var firstPerson = Root.AddComponent<VRMFirstPerson>();
            await awaitCaller.NextFrameIfTimedOut();

            var gltfFirstPerson = VRM.firstPerson;
            if (gltfFirstPerson.firstPersonBone != -1)
            {
                firstPerson.FirstPersonBone = Nodes[gltfFirstPerson.firstPersonBone];
                firstPerson.FirstPersonOffset = gltfFirstPerson.firstPersonBoneOffset;
            }
            else
            {
                // fallback
                firstPerson.SetDefault();
                firstPerson.FirstPersonOffset = gltfFirstPerson.firstPersonBoneOffset;
            }
            firstPerson.TraverseRenderers(this);
            await awaitCaller.NextFrameIfTimedOut();

            // LookAt
            var lookAtHead = Root.AddComponent<VRMLookAtHead>();
            await awaitCaller.NextFrameIfTimedOut();
            lookAtHead.OnImported(this);
            await awaitCaller.NextFrameIfTimedOut();
        }

        async Task LoadBlendShapeMaster(IAwaitCaller awaitCaller)
        {
            BlendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            BlendShapeAvatar.name = "BlendShape";

            var transformMeshTable = new Dictionary<Mesh, Transform>();
            foreach (var transform in Root.transform.Traverse())
            {
                if (transform.GetSharedMesh() != null)
                {
                    await awaitCaller.NextFrameIfTimedOut();
                    transformMeshTable.Add(transform.GetSharedMesh(), transform);
                }
            }

            var blendShapeList = VRM.blendShapeMaster.blendShapeGroups;
            if (blendShapeList != null && blendShapeList.Count > 0)
            {
                foreach (var x in blendShapeList)
                {
                    await awaitCaller.NextFrameIfTimedOut();
                    BlendShapeAvatar.Clips.Add(await LoadBlendShapeBind(x, transformMeshTable, awaitCaller));
                }
            }

            var proxy = Root.AddComponent<VRMBlendShapeProxy>();
            BlendShapeAvatar.CreateDefaultPreset();
            proxy.BlendShapeAvatar = BlendShapeAvatar;
        }

        async Task<BlendShapeClip> LoadBlendShapeBind(glTF_VRM_BlendShapeGroup group, Dictionary<Mesh, Transform> transformMeshTable, IAwaitCaller awaitCaller)
        {
            var asset = ScriptableObject.CreateInstance<BlendShapeClip>();
            var groupName = group.name;
            var prefix = "BlendShape.";
            while (groupName.FastStartsWith(prefix))
            {
                groupName = groupName.Substring(prefix.Length);
            }
            asset.name = "BlendShape." + groupName;

            if (group != null)
            {
                asset.BlendShapeName = groupName;
                asset.Preset = CachedEnum.ParseOrDefault<BlendShapePreset>(group.presetName, true);
                asset.IsBinary = group.isBinary;
                if (asset.Preset == BlendShapePreset.Unknown)
                {
                    // fallback
                    asset.Preset = CachedEnum.ParseOrDefault<BlendShapePreset>(group.name, true);
                }
                asset.Values = group.binds.Select(x =>
                {
                    var mesh = Meshes[x.mesh].Mesh;
                    var node = transformMeshTable[mesh];
                    var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(node, Root.transform);
                    return new BlendShapeBinding
                    {
                        RelativePath = relativePath,
                        Index = x.index,
                        Weight = x.weight,
                    };
                })
                .ToArray();
                await awaitCaller.NextFrameIfTimedOut();
                var materialValueBindings = group.materialValues.Select(x =>
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

                    var material = MaterialFactory.Materials
                        .Select(y => y.Asset)
                        .FirstOrDefault(y => y.name == x.materialName);
                    var propertyName = x.propertyName;
                    if (x.propertyName.FastEndsWith("_ST_S")
                        || x.propertyName.FastEndsWith("_ST_T"))
                    {
                        propertyName = x.propertyName.Substring(0, x.propertyName.Length - 2);
                    }

                    var binding = default(MaterialValueBinding?);

                    if (material != null)
                    {
                        try
                        {
                            binding = new MaterialValueBinding
                            {
                                MaterialName = x.materialName,
                                ValueName = x.propertyName,
                                TargetValue = value,
                                BaseValue = material.GetColor(propertyName),
                            };
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }

                    return binding;
                });
                await awaitCaller.NextFrameIfTimedOut();
                asset.MaterialValues = materialValueBindings
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
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

        private void LoadHumanoid()
        {
            AvatarDescription = VRM.humanoid.ToDescription(Nodes);
            AvatarDescription.name = "AvatarDescription";
            HumanoidAvatar = AvatarDescription.CreateAvatar(Root.transform);
            if (!HumanoidAvatar.isValid || !HumanoidAvatar.isHuman)
            {
                throw new Exception("fail to create avatar");
            }

            HumanoidAvatar.name = "VrmAvatar";

            var humanoid = Root.AddComponent<VRMHumanoidDescription>();
            humanoid.Avatar = HumanoidAvatar;
            humanoid.Description = AvatarDescription;

            var animator = Root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = Root.AddComponent<Animator>();
            }
            animator.avatar = HumanoidAvatar;

            // default としてとりあえず設定する
            // https://docs.unity3d.com/ScriptReference/Renderer-probeAnchor.html
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            foreach (var smr in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.probeAnchor = head;
            }
        }
        #endregion

        public UniHumanoid.AvatarDescription AvatarDescription;
        public Avatar HumanoidAvatar;
        public BlendShapeAvatar BlendShapeAvatar;
        public VRMMetaObject Meta;

        public async Task<VRMMetaObject> ReadMetaAsync(IAwaitCaller awaitCaller, bool createThumbnail = false)
        {
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            var meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            meta.name = "Meta";
            meta.ExporterVersion = VRM.exporterVersion;

            var gltfMeta = VRM.meta;
            meta.Version = gltfMeta.version; // model version
            meta.Author = gltfMeta.author;
            meta.ContactInformation = gltfMeta.contactInformation;
            meta.Reference = gltfMeta.reference;
            meta.Title = gltfMeta.title;
            if (gltfMeta.texture >= 0)
            {
                if (GltfTextureImporter.TryCreateSrgb(Data, gltfMeta.texture, Vector2.zero, Vector2.one, out var key, out var desc))
                {
                    meta.Thumbnail = await TextureFactory.GetTextureAsync(desc, awaitCaller) as Texture2D;
                }
            }
            meta.AllowedUser = gltfMeta.allowedUser;
            meta.ViolentUssage = gltfMeta.violentUssage;
            meta.SexualUssage = gltfMeta.sexualUssage;
            meta.CommercialUssage = gltfMeta.commercialUssage;
            meta.OtherPermissionUrl = gltfMeta.otherPermissionUrl;

            meta.LicenseType = gltfMeta.licenseType;
            meta.OtherLicenseUrl = gltfMeta.otherLicenseUrl;

            return meta;
        }

        public override void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            // VRM-0 は SubAssetKey を使っていないので default で済ます

            // VRM 固有のリソース(ScriptableObject)
            take(default, HumanoidAvatar);
            HumanoidAvatar = null;

            take(default, Meta);
            Meta = null;

            take(default, AvatarDescription);
            AvatarDescription = null;

            foreach (var x in BlendShapeAvatar.Clips)
            {
                take(default, x);
                {
                    // do nothing
                }
            }

            take(default, BlendShapeAvatar);
            BlendShapeAvatar = null;

            // GLTF のリソース
            base.TransferOwnership(take);
        }

        public override void Dispose()
        {
            // VRM specific
            if (HumanoidAvatar != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(HumanoidAvatar);
            }
            if (Meta != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(Meta);
            }
            if (AvatarDescription != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(AvatarDescription);
            }
            if (BlendShapeAvatar != null)
            {
                foreach (var clip in BlendShapeAvatar.Clips)
                {
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(clip);
                }
                UnityObjectDestroyer.DestroyRuntimeOrEditor(BlendShapeAvatar);
            }

            base.Dispose();
        }
    }
}
