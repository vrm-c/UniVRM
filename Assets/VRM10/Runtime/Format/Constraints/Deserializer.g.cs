// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_node_constraint {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_node_constraint.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_node_constraint extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}


public static VRMC_node_constraint Deserialize(JsonNode parsed)
{
    var value = new VRMC_node_constraint();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="specVersion"){
            value.SpecVersion = kv.Value.GetString();
            continue;
        }

        if(key=="constraint"){
            value.Constraint = Deserialize_Constraint(kv.Value);
            continue;
        }

    }
    return value;
}

public static Constraint Deserialize_Constraint(JsonNode parsed)
{
    var value = new Constraint();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="position"){
            value.Position = __constraint_Deserialize_Position(kv.Value);
            continue;
        }

        if(key=="rotation"){
            value.Rotation = __constraint_Deserialize_Rotation(kv.Value);
            continue;
        }

        if(key=="aim"){
            value.Aim = __constraint_Deserialize_Aim(kv.Value);
            continue;
        }

    }
    return value;
}

public static PositionConstraint __constraint_Deserialize_Position(JsonNode parsed)
{
    var value = new PositionConstraint();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="source"){
            value.Source = kv.Value.GetInt32();
            continue;
        }

        if(key=="sourceSpace"){
            value.SourceSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="destinationSpace"){
            value.DestinationSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="freezeAxes"){
            value.FreezeAxes = __constraint__position_Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static bool[] __constraint__position_Deserialize_FreezeAxes(JsonNode parsed)
{
    var value = new bool[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetBoolean();
    }
	return value;
} 

public static RotationConstraint __constraint_Deserialize_Rotation(JsonNode parsed)
{
    var value = new RotationConstraint();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="source"){
            value.Source = kv.Value.GetInt32();
            continue;
        }

        if(key=="sourceSpace"){
            value.SourceSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="destinationSpace"){
            value.DestinationSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="freezeAxes"){
            value.FreezeAxes = __constraint__rotation_Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static bool[] __constraint__rotation_Deserialize_FreezeAxes(JsonNode parsed)
{
    var value = new bool[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetBoolean();
    }
	return value;
} 

public static AimConstraint __constraint_Deserialize_Aim(JsonNode parsed)
{
    var value = new AimConstraint();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="source"){
            value.Source = kv.Value.GetInt32();
            continue;
        }

        if(key=="sourceSpace"){
            value.SourceSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="destinationSpace"){
            value.DestinationSpace = (ObjectSpace)Enum.Parse(typeof(ObjectSpace), kv.Value.GetString(), true);
            continue;
        }

        if(key=="aimVector"){
            value.AimVector = __constraint__aim_Deserialize_AimVector(kv.Value);
            continue;
        }

        if(key=="upVector"){
            value.UpVector = __constraint__aim_Deserialize_UpVector(kv.Value);
            continue;
        }

        if(key=="freezeAxes"){
            value.FreezeAxes = __constraint__aim_Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] __constraint__aim_Deserialize_AimVector(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __constraint__aim_Deserialize_UpVector(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static bool[] __constraint__aim_Deserialize_FreezeAxes(JsonNode parsed)
{
    var value = new bool[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetBoolean();
    }
	return value;
} 

} // GltfDeserializer
} // UniGLTF 
