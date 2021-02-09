// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_constraints {

public static class GltfDeserializer
{

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_constraints extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == VRMC_constraints.ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}


public static VRMC_constraints Deserialize(JsonNode parsed)
{
    var value = new VRMC_constraints();

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
            value.Position = Deserialize_Position(kv.Value);
            continue;
        }

        if(key=="rotation"){
            value.Rotation = Deserialize_Rotation(kv.Value);
            continue;
        }

        if(key=="aim"){
            value.Aim = Deserialize_Aim(kv.Value);
            continue;
        }

    }
    return value;
}

public static PositionConstraint Deserialize_Position(JsonNode parsed)
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
            value.FreezeAxes = Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static bool[] Deserialize_FreezeAxes(JsonNode parsed)
{
    var value = new bool[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetBoolean();
    }
	return value;
} 

public static RotationConstraint Deserialize_Rotation(JsonNode parsed)
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
            value.FreezeAxes = Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static AimConstraint Deserialize_Aim(JsonNode parsed)
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
            value.AimVector = Deserialize_AimVector(kv.Value);
            continue;
        }

        if(key=="upVector"){
            value.UpVector = Deserialize_UpVector(kv.Value);
            continue;
        }

        if(key=="freezeAxes"){
            value.FreezeAxes = Deserialize_FreezeAxes(kv.Value);
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_AimVector(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] Deserialize_UpVector(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

} // GltfDeserializer
} // UniGLTF 
