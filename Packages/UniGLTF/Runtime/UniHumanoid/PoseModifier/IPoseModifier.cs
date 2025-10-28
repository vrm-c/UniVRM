using UnityEngine;


namespace UniHumanoid
{
    public interface IPoseModifier
    {
        void Modify(ref HumanPose pose);
    }
}
