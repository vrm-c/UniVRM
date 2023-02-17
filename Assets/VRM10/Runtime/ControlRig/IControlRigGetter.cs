using UnityEngine;

namespace UniVRM10
{
    public interface IControlRigGetter
    {
        Vector3 GetRootPosition();

        Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone);
    }
}
