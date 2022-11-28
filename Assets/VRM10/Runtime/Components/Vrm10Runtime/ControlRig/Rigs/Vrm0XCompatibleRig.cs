using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public static class Vrm0XCompatibleRig
    {
        /// <summary>
        /// 空の Dictionary を返します。
        /// HumanBodyBones に対応する Quaternion が無い場合は Quaternion.Identity を採用する仕様です。
        /// VRM-0.X では T-Pose のときにすべてのボーンが Quaternion.Identity です。
        /// </summary>
        public static IReadOnlyDictionary<HumanBodyBones, Quaternion> InitialRotations => new Dictionary<HumanBodyBones, Quaternion>();
    }
}
