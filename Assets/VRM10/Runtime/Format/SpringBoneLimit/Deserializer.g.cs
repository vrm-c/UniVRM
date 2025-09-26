// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_springBone_limit {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_springBone_limit.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_springBone_limit extension)
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


public static VRMC_springBone_limit Deserialize(JsonNode parsed)
{
    var value = new VRMC_springBone_limit();

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

        if(key=="limit"){
            value.Limit = Deserialize_Limit(kv.Value);
            continue;
        }

    }
    return value;
}

public static Limit Deserialize_Limit(JsonNode parsed)
{
    var value = new Limit();

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

        if(key=="cone"){
            value.Cone = __limit_Deserialize_Cone(kv.Value);
            continue;
        }

        if(key=="hinge"){
            value.Hinge = __limit_Deserialize_Hinge(kv.Value);
            continue;
        }

        if(key=="spherical"){
            value.Spherical = __limit_Deserialize_Spherical(kv.Value);
            continue;
        }

    }
    return value;
}

public static ConeLimit __limit_Deserialize_Cone(JsonNode parsed)
{
    var value = new ConeLimit();

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

        if(key=="angle"){
            value.Angle = kv.Value.GetSingle();
            continue;
        }

        if(key=="rotation"){
            value.Rotation = __limit__cone_Deserialize_Rotation(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __limit__cone_Deserialize_Rotation(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static HingeLimit __limit_Deserialize_Hinge(JsonNode parsed)
{
    var value = new HingeLimit();

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

        if(key=="angle"){
            value.Angle = kv.Value.GetSingle();
            continue;
        }

        if(key=="rotation"){
            value.Rotation = __limit__hinge_Deserialize_Rotation(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __limit__hinge_Deserialize_Rotation(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static SphericalLimit __limit_Deserialize_Spherical(JsonNode parsed)
{
    var value = new SphericalLimit();

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

        if(key=="pitch"){
            value.Pitch = kv.Value.GetSingle();
            continue;
        }

        if(key=="yaw"){
            value.Yaw = kv.Value.GetSingle();
            continue;
        }

        if(key=="rotation"){
            value.Rotation = __limit__spherical_Deserialize_Rotation(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __limit__spherical_Deserialize_Rotation(JsonNode parsed)
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
