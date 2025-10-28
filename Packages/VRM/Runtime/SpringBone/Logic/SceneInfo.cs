using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    readonly struct SceneInfo
    {
        public readonly IReadOnlyList<Transform> RootBones;
        public readonly Transform Center;
        public readonly VRMSpringBoneColliderGroup[] ColliderGroups;
        public readonly Vector3 ExternalForce;

        public SceneInfo(
            IReadOnlyList<Transform> rootBones,
            Transform center,
            VRMSpringBoneColliderGroup[] colliderGroups,
            Vector3 externalForce) =>
                (RootBones, Center, ColliderGroups, ExternalForce)
                = (rootBones, center, colliderGroups, externalForce);
    }
}