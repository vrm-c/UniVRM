using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10Constraint
    {
        void Process();

        GameObject GameObject { get; }
    }
}
