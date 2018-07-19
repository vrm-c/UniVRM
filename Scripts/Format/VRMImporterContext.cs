using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public class VRMImporterContext : ImporterContext
    {
        public VRMImporterContext()
        {

        }
        public VRMImporterContext(string path)
        {
            Path = path;
        }

        public UniHumanoid.AvatarDescription AvatarDescription;
        public Avatar HumanoidAvatar;
        public BlendShapeAvatar BlendShapeAvatar;
        public VRMMetaObject Meta;

        public VRMMetaObject ReadMeta(bool createThumbnail=false)
        {
            var meta=ScriptableObject.CreateInstance<VRMMetaObject>();
            meta.name = "Meta";
            meta.ExporterVersion = GLTF.extensions.VRM.exporterVersion;

            var gltfMeta = GLTF.extensions.VRM.meta;
            meta.Version = gltfMeta.version; // model version
            meta.Author = gltfMeta.author;
            meta.ContactInformation = gltfMeta.contactInformation;
            meta.Reference = gltfMeta.reference;
            meta.Title = gltfMeta.title;

            if (gltfMeta.texture >= 0 && gltfMeta.texture < Textures.Count)
            {
                // ロード済み
                meta.Thumbnail = Textures[gltfMeta.texture].Texture;
            }
            else if (createThumbnail)
            {
                // 作成する(先行ロード用)
                if(gltfMeta.texture >= 0 && gltfMeta.texture < GLTF.textures.Count)
                {
                    var t = new TextureItem(GLTF, gltfMeta.texture);
                    t.Process(GLTF, Storage);
                    meta.Thumbnail=t.Texture;
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

#if UNITY_EDITOR
        protected override IEnumerable<Object> ObjectsForSubAsset()
        {
            foreach (var x in base.ObjectsForSubAsset())
            {
                yield return x;
            }

            yield return AvatarDescription;
            yield return HumanoidAvatar;
            yield return BlendShapeAvatar;
            if (BlendShapeAvatar != null && BlendShapeAvatar.Clips != null)
            {
                foreach (var x in BlendShapeAvatar.Clips)
                {
                    yield return x;
                }
            }

            yield return Meta;
        }
#endif
    }
}
