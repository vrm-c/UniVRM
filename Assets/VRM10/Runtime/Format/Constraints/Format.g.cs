// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_node_constraint
{

    public class RotationConstraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // Axes be constrained by this constraint, in X-Y-Z order.
        public bool[] Axes;

        // The weight of the constraint.
        public float? Weight;
    }

    public class Constraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // A constraint that links the rotation with a source.
        public RotationConstraint Rotation;
    }

    public class VRMC_node_constraint
    {
        public const string ExtensionName = "VRMC_node_constraint";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_node_constraint
        public string SpecVersion;

        // Contains position, rotation, or aim
        public Constraint Constraint;
    }
}
