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

        public glTF_VRM VRM
        {
            get
            {
                return (glTF_VRM)GLTF;
            }
        }

        public System.ArraySegment<byte> ParseVrm(byte[] bytes)
        {
            return ParseGlb<glTF_VRM>(bytes);
        }

        public VRMMetaObject ReadMeta()
        {
            var meta=ScriptableObject.CreateInstance<VRMMetaObject>();
            meta.name = "Meta";
            var gltfMeta = VRM.extensions.VRM.meta;
            meta.Author = gltfMeta.author;
            meta.ContactInformation = gltfMeta.contactInformation;
            meta.Title = gltfMeta.title;
            if (gltfMeta.texture != -1)
            {
                meta.Thumbnail = Textures[gltfMeta.texture].Texture;
            }
            meta.LicenseType = gltfMeta.licenseType;
            meta.OtherLicenseUrl = gltfMeta.otherLicenseUrl;
            meta.Reference = gltfMeta.reference;
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
