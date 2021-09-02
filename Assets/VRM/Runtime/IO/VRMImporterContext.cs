using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UniJSON;
using System.Threading.Tasks;
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
            IMaterialDescriptorGenerator materialGenerator = null)
            : base(data.Data, externalObjectMap, textureDeserializer, materialGenerator ?? new VRMMaterialDescriptorGenerator(data.VrmExtension))
        {
            _data = data;
            TextureDescriptorGenerator = new VrmTextureDescriptorGenerator(Data, VRM);
        }

        #region OnLoad
        protected override async Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            Root.name = "VRM";

            using (MeasureTime("VRM LoadMeta"))
            {
                await LoadMetaAsync();
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadHumanoid"))
            {
                LoadHumanoid();
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadBlendShapeMaster"))
            {
                LoadBlendShapeMaster();
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadSecondary"))
            {
                VRMSpringUtility.LoadSecondary(Root.transform, Nodes,
                VRM.secondaryAnimation);
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("VRM LoadFirstPerson"))
            {
                LoadFirstPerson();
            }
        }

        async Task LoadMetaAsync()
        {
            var meta = await ReadMetaAsync();
            var _meta = Root.AddComponent<VRMMeta>();
            _meta.Meta = meta;
            Meta = meta;
        }

        void LoadFirstPerson()
        {
            var firstPerson = Root.AddComponent<VRMFirstPerson>();

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

            // LookAt
            var lookAtHead = Root.AddComponent<VRMLookAtHead>();
            lookAtHead.OnImported(this);
        }

        void LoadBlendShapeMaster()
        {
            BlendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            BlendShapeAvatar.name = "BlendShape";

            var transformMeshTable = new Dictionary<Mesh, Transform>();
            foreach (var transform in Root.transform.Traverse())
            {
                if (transform.GetSharedMesh() != null)
                {
                    transformMeshTable.Add(transform.GetSharedMesh(), transform);
                }
            }

            var blendShapeList = VRM.blendShapeMaster.blendShapeGroups;
            if (blendShapeList != null && blendShapeList.Count > 0)
            {
                foreach (var x in blendShapeList)
                {
                    BlendShapeAvatar.Clips.Add(LoadBlendShapeBind(x, transformMeshTable));
                }
            }

            var proxy = Root.AddComponent<VRMBlendShapeProxy>();
            BlendShapeAvatar.CreateDefaultPreset();
            proxy.BlendShapeAvatar = BlendShapeAvatar;
        }

        BlendShapeClip LoadBlendShapeBind(glTF_VRM_BlendShapeGroup group, Dictionary<Mesh, Transform> transformMeshTable)
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
                asset.Preset = CacheEnum.TryParseOrDefault<BlendShapePreset>(group.presetName, true);
                asset.IsBinary = group.isBinary;
                if (asset.Preset == BlendShapePreset.Unknown)
                {
                    // fallback
                    asset.Preset = CacheEnum.TryParseOrDefault<BlendShapePreset>(group.name, true);
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
                })
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

        public async Task<VRMMetaObject> ReadMetaAsync(IAwaitCaller awaitCaller = null, bool createThumbnail = false)
        {
            awaitCaller = awaitCaller ?? new ImmediateCaller();

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
                var (key, param) = GltfTextureImporter.CreateSRGB(Data, gltfMeta.texture, Vector2.zero, Vector2.one);
                meta.Thumbnail = await TextureFactory.GetTextureAsync(param, awaitCaller) as Texture2D;
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
                UnityObjectDestoyer.DestroyRuntimeOrEditor(HumanoidAvatar);
            }
            if (Meta != null)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(Meta);
            }
            if (AvatarDescription != null)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(AvatarDescription);
            }
            if (BlendShapeAvatar != null)
            {
                foreach (var clip in BlendShapeAvatar.Clips)
                {
                    UnityObjectDestoyer.DestroyRuntimeOrEditor(clip);
                }
                UnityObjectDestoyer.DestroyRuntimeOrEditor(BlendShapeAvatar);
            }

            base.Dispose();
        }
    }
}
