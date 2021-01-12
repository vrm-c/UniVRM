// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_constraints
{

    public enum ObjectSpace
    {
        model,
        local,

    }

    public class PositionConstraint
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The source node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace DestinationSpace;

        // Axes be constrained by this constraint, in X-Y-Z order.
        public bool[] FreezeAxes;

        // The weight of the constraint.
        public float? Weight;
    }

    public class RotationConstraint
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The source node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace DestinationSpace;

        // Axes be constrained by this constraint, in X-Y-Z order.
        public bool[] FreezeAxes;

        // The weight of the constraint.
        public float? Weight;
    }

    public class AimConstraint
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // The user-defined name of this object.
        public string Name;

        // The index of the node constrains the node.
        public int? Source;

        // The source node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace SourceSpace;

        // The destination node will be evaluated in this space.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ObjectSpace DestinationSpace;

        // An axis which faces the direction of its sources.
        public float[] AimVector;

        // An up axis of the constraint.
        public float[] UpVector;

        // Axes be constrained by this constraint, in Yaw-Pitch order.
        public bool[] FreezeAxes;

        // The weight of the constraint.
        public float? Weight;
    }

    public class VRMC_constraints
    {
        public const string ExtensionName = "VRMC_constraints";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // A constraint that links the position with sources.
        public PositionConstraint Position;

        // A constraint that links the rotation with sources.
        public RotationConstraint Rotation;

        // A constraint that rotates the node to face sources.
        public AimConstraint Aim;
    }
}
