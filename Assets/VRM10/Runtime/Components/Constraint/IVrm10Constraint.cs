using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10Constraint
    {
        Transform ConstraintDst { get; }
        Transform ConstriantSrc { get; }

        /// <param name="targetInitState"></param>
        /// <param name="srcInitState"></param>
        internal void Process(in TransformState dstInitState, in TransformState srcInitState);
    }
}
