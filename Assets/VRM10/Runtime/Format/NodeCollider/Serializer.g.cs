// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_node_collider {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_node_collider extension)
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
            extensions.Add(VRMC_node_collider.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_node_collider value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
    }

    if(value.Shapes!=null&&value.Shapes.Count()>=0){
        f.Key("shapes");                
        Serialize_Shapes(f, value.Shapes);
    }

    f.EndMap();
}

public static void Serialize_Shapes(JsonFormatter f, List<ColliderShape> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_Shapes_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_Shapes_ITEM(JsonFormatter f, ColliderShape value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
    }

    if(value.Sphere!=null){
        f.Key("sphere");                
        Serialize_Sphere(f, value.Sphere);
    }

    if(value.Capsule!=null){
        f.Key("capsule");                
        Serialize_Capsule(f, value.Capsule);
    }

    f.EndMap();
}

public static void Serialize_Sphere(JsonFormatter f, ColliderShapeSphere value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_Capsule(JsonFormatter f, ColliderShapeCapsule value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    if(value.Tail!=null&&value.Tail.Count()>=3){
        f.Key("tail");                
        Serialize_Tail(f, value.Tail);
    }

    f.EndMap();
}

public static void Serialize_Tail(JsonFormatter f, float[] value)
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
