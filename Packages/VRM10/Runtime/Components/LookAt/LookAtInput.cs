using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// LookAt を具体的な値に解決する前の入力値
    /// この値を元に LookAtEyeDirection を生成し、
    /// LookAtEyeDirection を Bone もしくは MorphTarget に対して適用する。
    /// </summary>
    public struct LookAtInput
    {
        public LookAtEyeDirection? YawPitch;
        public Vector3? WorldPosition;
    }
}
