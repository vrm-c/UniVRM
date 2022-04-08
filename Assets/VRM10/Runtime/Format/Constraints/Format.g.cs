// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_node_constraint
{

    public enum RollAxis
    {
        X,
        Y,
        Z,

    }

    public class RollConstraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The roll axis of the constraint.
        public RollAxis RollAxis;

        // The weight of the constraint.
        public float? Weight;
    }

    public enum AimAxis
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ,

    }

    public class AimConstraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The aim axis of the constraint.
        public AimAxis AimAxis;

        // The weight of the constraint.
        public float? Weight;
    }

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

        // The weight of the constraint.
        public float? Weight;
    }

    public class Constraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // A constraint that transfers a rotation around one axis of a source.
        public RollConstraint Roll;

        // A constraint that makes it look at a source object.
        public AimConstraint Aim;

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

        // Contains roll, aim, or rotation
        public Constraint Constraint;
    }
}
