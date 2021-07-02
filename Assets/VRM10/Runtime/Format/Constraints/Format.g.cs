// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_node_constraint
{

    public enum ObjectSpace
    {
        model,
        local,

    }

    public class PositionConstraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The source node will be evaluated in this space.
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        public ObjectSpace DestinationSpace;

        // Axes be constrained by this constraint, in X-Y-Z order.
        public bool[] FreezeAxes;

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

        // The source node will be evaluated in this space.
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        public ObjectSpace DestinationSpace;

        // Axes be constrained by this constraint, in X-Y-Z order.
        public bool[] FreezeAxes;

        // The weight of the constraint.
        public float? Weight;
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

        // The source node will be evaluated in this space.
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        public ObjectSpace DestinationSpace;

        // An axis which faces the direction of its source.
        public float[] AimVector;

        // An up axis of the constraint.
        public float[] UpVector;

        // Axes be constrained by this constraint, in Yaw-Pitch order.
        public bool[] FreezeAxes;

        // The weight of the constraint.
        public float? Weight;
    }

    public class Constraint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // A constraint that links the position with a source.
        public PositionConstraint Position;

        // A constraint that links the rotation with a source.
        public RotationConstraint Rotation;

        // A constraint that rotates the node to face a source.
        public AimConstraint Aim;
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
