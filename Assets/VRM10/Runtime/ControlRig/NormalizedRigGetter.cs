using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 正規化済みのヒエラルキーの getter。
    /// 回転の変換はしないが、ボーンの有無(upperChestなど)の対応はする。
    /// </summary>
    public class NormalizedRigGetter : IControlRigGetter
    {
        public NormalizedRigGetter(Animator animator)
        {

        }

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
