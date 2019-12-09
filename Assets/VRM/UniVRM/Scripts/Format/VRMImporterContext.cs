using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.IO;


namespace VRM
{
    public class VRMImporterContext : ImporterContext
    {
        const string HUMANOID_KEY = "humanoid";
        const string MATERIAL_KEY = "materialProperties";

        public VRMImporterContext()
        {
        }

        public override void Parse(string path, byte[] bytes)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".vrm":
                    ParseGlb(bytes);
                    break;

                default:
                    base.Parse(path, bytes);
                    break;
            }
        }

        public override void ParseJson(string json, IStorage storage)
        {
            base.ParseJson(json, storage);
            SetMaterialImporter(new VRMMaterialImporter(this, glTF_VRM_Material.Parse(Json)));
        }

        #region OnLoad
        protected override void OnLoadModel()
        {
            Root.name = "VRM";

            using (MeasureTime("VRM LoadMeta"))
            {
                LoadMeta();
            }

            using (MeasureTime("VRM LoadHumanoid"))
            {
                LoadHumanoid();
            }

            using (MeasureTime("VRM LoadBlendShapeMaster"))
            {
                LoadBlendShapeMaster();
            }
            using (MeasureTime("VRM LoadSecondary"))
            {
                VRMSpringUtility.LoadSecondary(Root.transform, Nodes,
                GLTF.extensions.VRM.secondaryAnimation);
            }
            using (MeasureTime("VRM LoadFirstPerson"))
            {
                LoadFirstPerson();
            }
        }

        void LoadMeta()
        {
            var meta = ReadMeta();
            if (meta.Thumbnail == null)
            {
                /*
                // 作る
                var lookAt = Root.GetComponent<VRMLookAtHead>();
                var thumbnail = lookAt.CreateThumbnail();
                thumbnail.name = "thumbnail";
                meta.Thumbnail = thumbnail;
                Textures.Add(new TextureItem(thumbnail));
                */
            }
            var _meta = Root.AddComponent<VRMMeta>();
            _meta.Meta = meta;
            Meta = meta;
        }

        void LoadFirstPerson()
        {
            var firstPerson = Root.AddComponent<VRMFirstPerson>();

            var gltfFirstPerson = GLTF.extensions.VRM.firstPerson;
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

            var blendShapeList = GLTF.extensions.VRM.blendShapeMaster.blendShapeGroups;
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
            while (groupName.StartsWith(prefix))
            {
                groupName = groupName.Substring(prefix.Length);
            }
            asset.name = "BlendShape." + groupName;

            if (group != null)
            {
                asset.BlendShapeName = groupName;
                asset.Preset = EnumUtil.TryParseOrDefault<BlendShapePreset>(group.presetName);
                asset.IsBinary = group.isBinary;
                if (asset.Preset == BlendShapePreset.Unknown)
                {
                    // fallback
                    asset.Preset = EnumUtil.TryParseOrDefault<BlendShapePreset>(group.name);
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

                    var material = GetMaterials().FirstOrDefault(y => y.name == x.materialName);
                    var propertyName = x.propertyName;
                    if (x.propertyName.EndsWith("_ST_S")
                    || x.propertyName.EndsWith("_ST_T"))
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
            AvatarDescription = GLTF.extensions.VRM.humanoid.ToDescription(Nodes);
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
        }
        #endregion

        public UniHumanoid.AvatarDescription AvatarDescription;
        public Avatar HumanoidAvatar;
        public BlendShapeAvatar BlendShapeAvatar;
        public VRMMetaObject Meta;

        public VRMMetaObject ReadMeta(bool createThumbnail = false)
        {
            var meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            meta.name = "Meta";
            meta.ExporterVersion = GLTF.extensions.VRM.exporterVersion;

            var gltfMeta = GLTF.extensions.VRM.meta;
            meta.Version = gltfMeta.version; // model version
            meta.Author = gltfMeta.author;
            meta.ContactInformation = gltfMeta.contactInformation;
            meta.Reference = gltfMeta.reference;
            meta.Title = gltfMeta.title;

            var thumbnail = GetTexture(gltfMeta.texture);
            if (thumbnail != null)
            {
                // ロード済み
                meta.Thumbnail = thumbnail.Texture;
            }
            else if (createThumbnail)
            {
                // 作成する(先行ロード用)
                if (gltfMeta.texture >= 0 && gltfMeta.texture < GLTF.textures.Count)
                {
                    var t = new TextureItem(gltfMeta.texture);
                    t.Process(GLTF, Storage);
                    meta.Thumbnail = t.Texture;
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

        protected override IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        {
            foreach (var x in base.ObjectsForSubAsset())
            {
                yield return x;
            }

            yield return AvatarDescription;
            yield return HumanoidAvatar;

            if (BlendShapeAvatar != null && BlendShapeAvatar.Clips != null)
            {
                foreach (var x in BlendShapeAvatar.Clips)
                {
                    yield return x;
                }
            }
            yield return BlendShapeAvatar;

            yield return Meta;
        }

#if UNITY_EDITOR
        public override bool AvoidOverwriteAndLoad(UnityPath assetPath, UnityEngine.Object o)
        {
            if (o is BlendShapeAvatar)
            {
                var loaded = assetPath.LoadAsset<BlendShapeAvatar>();
                var proxy = Root.GetComponent<VRMBlendShapeProxy>();
                proxy.BlendShapeAvatar = loaded;

                return true;
            }

            if (o is BlendShapeClip)
            {
                return true;
            }

            return base.AvoidOverwriteAndLoad(assetPath, o);
        }

        protected override UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o)
        {
            if (o is BlendShapeAvatar
                || o is BlendShapeClip)
            {
                var dir = prefabPath.GetAssetFolder(".BlendShapes");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is Avatar)
            {
                var dir = prefabPath.GetAssetFolder(".Avatar");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is VRMMetaObject)
            {
                var dir = prefabPath.GetAssetFolder(".MetaObject");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is UniHumanoid.AvatarDescription)
            {
                var dir = prefabPath.GetAssetFolder(".AvatarDescription");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else
            {
                return base.GetAssetPath(prefabPath, o);
            }
        }
#endif
    }
}
