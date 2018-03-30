using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public class VRMImporterContext : ImporterContext
    {
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
