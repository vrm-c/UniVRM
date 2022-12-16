using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// ControlRig に対する入力インタフェース
    /// </summary>
    public interface IControlRigInput
    {
        Vector3 RootPosition { get; }
        bool TryGetBoneLocalRotation(HumanBodyBones bone, Quaternion parent, out Quaternion rotation);
    }
}