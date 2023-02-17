using UnityEngine;

namespace UniVRM10
{
    public interface IControlRigGetter
    {
        Vector3 GetRootPosition();

        Quaternion GetNormalizedRotation(HumanBodyBones bone, HumanBodyBones parentBone);
    }
}
