// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_springBone_extended_collider {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_springBone_extended_collider extension)
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
            extensions.Add(VRMC_springBone_extended_collider.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_springBone_extended_collider value)
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

    if(value.Shape!=null){
        f.Key("shape");                
        Serialize_Shape(f, value.Shape);
    }

    f.EndMap();
}

public static void Serialize_Shape(JsonFormatter f, ExtendedColliderShape value)
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

    if(value.Sphere!=null){
        f.Key("sphere");                
        __shape_Serialize_Sphere(f, value.Sphere);
    }

    if(value.Capsule!=null){
        f.Key("capsule");                
        __shape_Serialize_Capsule(f, value.Capsule);
    }

    if(value.Plane!=null){
        f.Key("plane");                
        __shape_Serialize_Plane(f, value.Plane);
    }

    f.EndMap();
}

public static void __shape_Serialize_Sphere(JsonFormatter f, ExtendedColliderShapeSphere value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        __shape__sphere_Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    if(value.Inside.HasValue){
        f.Key("inside");                
        f.Value(value.Inside.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __shape__sphere_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __shape_Serialize_Capsule(JsonFormatter f, ExtendedColliderShapeCapsule value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        __shape__capsule_Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    if(value.Tail!=null&&value.Tail.Count()>=3){
        f.Key("tail");                
        __shape__capsule_Serialize_Tail(f, value.Tail);
    }

    if(value.Inside.HasValue){
        f.Key("inside");                
        f.Value(value.Inside.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __shape__capsule_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __shape__capsule_Serialize_Tail(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __shape_Serialize_Plane(JsonFormatter f, ExtendedColliderShapePlane value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        __shape__plane_Serialize_Offset(f, value.Offset);
    }

    if(value.Normal!=null&&value.Normal.Count()>=3){
        f.Key("normal");                
        __shape__plane_Serialize_Normal(f, value.Normal);
    }

    f.EndMap();
}

public static void __shape__plane_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __shape__plane_Serialize_Normal(JsonFormatter f, float[] value)
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
