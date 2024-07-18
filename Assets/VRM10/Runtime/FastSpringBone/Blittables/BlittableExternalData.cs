using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// 外力等の毎フレーム更新されうる外部から与えられる情報
    /// </summary>
    public struct BlittableExternalData
    {
        public Vector3 ExternalForce;

        /// <summary>
        /// if false, spring bone is paused.
        /// </summary>
        public bool IsSpringBoneEnabled;
    }
}