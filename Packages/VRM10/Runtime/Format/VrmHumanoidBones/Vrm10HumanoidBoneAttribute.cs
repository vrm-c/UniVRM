using UnityEngine;

namespace UniVRM10
{
    public readonly struct Vrm10HumanoidBoneAttribute
    {
        public Vrm10HumanoidBones Bone { get; }
        public bool IsRequired { get; }
        public Vrm10HumanoidBones? ParentBone { get; }
        public bool? NeedsParentBone { get; }
        public HumanBodyBones UnityBone { get; }

        public Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones bone, bool isRequired, Vrm10HumanoidBones? parentBone, bool? needsParentBone, HumanBodyBones unityBone)
        {
            Bone = bone;
            IsRequired = isRequired;
            ParentBone = parentBone;
            NeedsParentBone = needsParentBone;
            UnityBone = unityBone;
        }
    }
}