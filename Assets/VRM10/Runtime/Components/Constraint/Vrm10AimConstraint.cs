using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_draft/schema/VRMC_node_constraint.aimConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10AimConstraint : Vrm10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public UniGLTF.Extensions.VRMC_node_constraint.AimAxis AimAxis;

        public override void Process()
        {
            throw new System.NotImplementedException();
        }
    }
}
