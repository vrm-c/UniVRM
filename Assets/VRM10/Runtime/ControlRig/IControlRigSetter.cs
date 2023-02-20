using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public interface IControlRigSetter
    {
        /// TODO: 何らかのスケーリングが必用
        void SetRootPosition(Vector3 position);

        /// <param name="normalizedLocalRotation">初期姿勢(TPose) が Quaternion.Identity である相対回転</param>
        void SetNormalizedLocalRotation(HumanBodyBones bone, Quaternion normalizedLocalRotation);

        /// <returns>ボーンを親ボーンとセットで返す</returns>
        IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBones();
    }
}
