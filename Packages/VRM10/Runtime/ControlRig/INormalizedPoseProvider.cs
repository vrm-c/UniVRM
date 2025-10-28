using UnityEngine;

namespace UniVRM10
{
    public interface INormalizedPoseProvider
    {
        /// <summary>
        /// Get hips position in model root space
        /// </summary>
        Vector3 GetRawHipsPosition();

        /// <summary>
        /// Get normalized local rotation
        /// </summary>
        Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone);
    }
}
