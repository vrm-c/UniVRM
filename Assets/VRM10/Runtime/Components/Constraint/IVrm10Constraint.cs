using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10Constraint
    {
        internal void Process();

        GameObject ConstraintTarget { get; }
    }
}
