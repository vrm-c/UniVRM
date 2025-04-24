using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10Constraint
    {
        /// <summary>
        /// The object to which the Constraint applies
        /// </summary>
        Transform ConstraintTarget { get; }

        /// <summary>
        /// The object for which the Constraint is the source of the pose
        /// </summary>
        Transform ConstraintSource { get; }

        internal void Process(in TransformState targetInitState, in TransformState sourceInitState);
    }
}