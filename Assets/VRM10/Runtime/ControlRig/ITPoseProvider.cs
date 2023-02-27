using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public interface ITPoseProvider
    {
        /// <summary>
        /// このTPoseに含まれるボーンと親ボーンの組み合わせを列挙する。
        ///
        /// * Humanoidの木構造を深さ優先の順番で列挙する
        ///
        /// </summary>
        /// <returns>hips の場合は parent は HumanBodyBones.LastBone で null を表す</returns>
        IEnumerable<(HumanBodyBones Bone, HumanBodyBones Parent)> EnumerateBoneParentPairs();

        Quaternion? GetBoneWorldRotation(HumanBodyBones bone);
        Vector3? GetBoneWorldPosition(HumanBodyBones bone);
    }
}
