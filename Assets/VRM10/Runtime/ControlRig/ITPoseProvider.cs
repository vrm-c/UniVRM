using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public interface ITPoseProvider
    {
        /// <summary>
        /// ボーンを親ボーンとセットで列挙する</returns>
        /// </summary>
        /// <returns>hips の場合は parent は HumanBodyBones.LastBone で null を表す</returns>
        IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBoneParentPairs();

        /// <summary>
        /// * bone 無いときは throw するべし
        /// * EnumerateBoneParentPairs で事前に有無を確認できる
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        Quaternion GetBoneWorldRotation(HumanBodyBones bone);

        /// <summary>
        /// * bone 無いときは throw するべし
        /// * EnumerateBoneParentPairs で事前に有無を確認できる
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        Vector3 GetBoneWorldPosition(HumanBodyBones bone);
    }
}
