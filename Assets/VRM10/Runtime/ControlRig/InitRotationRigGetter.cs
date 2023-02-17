using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// ボーンが初期回転を持っているヒエラルキーから、VRM0互換の回転を取り出す
    /// </summary>
    public class Vrm10InitRotationGetter : IControlRigGetter
    {
        public Quaternion GetNormalizedRotation(HumanBodyBones bone, HumanBodyBones parentBone)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetRootPosition()
        {
            throw new System.NotImplementedException();
        }
    }
}
