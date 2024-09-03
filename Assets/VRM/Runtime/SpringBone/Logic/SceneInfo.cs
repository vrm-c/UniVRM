using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    readonly struct SceneInfo
    {
        public readonly IReadOnlyList<Transform> RootBones;
        public readonly Transform Center;
        public readonly VRMSpringBoneColliderGroup[] ColliderGroups;

        public SceneInfo(
            IReadOnlyList<Transform> rootBones,
            Transform center,
            VRMSpringBoneColliderGroup[] colliderGroups) =>
                (RootBones, Center, ColliderGroups) = (rootBones, center, colliderGroups);
    }
}