// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_springBone_limit {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_springBone_limit extension)
        {
            if (dst is glTFExtensionImport)
            {
                throw new NotImplementedException();
            }

            if (!(dst is glTFExtensionExport extensions))
            {
                extensions = new glTFExtensionExport();
                dst = extensions;
            }

            var f = new JsonFormatter();
            Serialize(f, extension);
            extensions.Add(VRMC_springBone_limit.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_springBone_limit value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.SpecVersion)){
        f.Key("specVersion");                
        f.Value(value.SpecVersion);
    }

    if(value.Limit!=null){
        f.Key("limit");                
        Serialize_Limit(f, value.Limit);
    }

    f.EndMap();
}

public static void Serialize_Limit(JsonFormatter f, Limit value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Cone!=null){
        f.Key("cone");                
        __limit_Serialize_Cone(f, value.Cone);
    }

    if(value.Hinge!=null){
        f.Key("hinge");                
        __limit_Serialize_Hinge(f, value.Hinge);
    }

    if(value.Spherical!=null){
        f.Key("spherical");                
        __limit_Serialize_Spherical(f, value.Spherical);
    }

    f.EndMap();
}

public static void __limit_Serialize_Cone(JsonFormatter f, ConeLimit value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Angle.HasValue){
        f.Key("angle");                
        f.Value(value.Angle.GetValueOrDefault());
    }

    if(value.Rotation!=null&&value.Rotation.Count()>=4){
        f.Key("rotation");                
        __limit__cone_Serialize_Rotation(f, value.Rotation);
    }

    f.EndMap();
}

public static void __limit__cone_Serialize_Rotation(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __limit_Serialize_Hinge(JsonFormatter f, HingeLimit value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Angle.HasValue){
        f.Key("angle");                
        f.Value(value.Angle.GetValueOrDefault());
    }

    if(value.Rotation!=null&&value.Rotation.Count()>=4){
        f.Key("rotation");                
        __limit__hinge_Serialize_Rotation(f, value.Rotation);
    }

    f.EndMap();
}

public static void __limit__hinge_Serialize_Rotation(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __limit_Serialize_Spherical(JsonFormatter f, SphericalLimit value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Pitch.HasValue){
        f.Key("pitch");                
        f.Value(value.Pitch.GetValueOrDefault());
    }

    if(value.Yaw.HasValue){
        f.Key("yaw");                
        f.Value(value.Yaw.GetValueOrDefault());
    }

    if(value.Rotation!=null&&value.Rotation.Count()>=4){
        f.Key("rotation");                
        __limit__spherical_Serialize_Rotation(f, value.Rotation);
    }

    f.EndMap();
}

public static void __limit__spherical_Serialize_Rotation(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

    } // class
} // namespace
