using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public interface INormalizedPoseApplicable
    {
        void SetHipsPosition(Vector3 position);

        void SetNormalizedLocalRotation(HumanBodyBones bone, Quaternion normalizedLocalRotation);
    }
}
