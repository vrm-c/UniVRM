using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    struct SceneInfo
    {
        public IReadOnlyList<Transform> RootBones;
        public Transform Center;
        public VRMSpringBoneColliderGroup[] ColliderGroups;
    }
}