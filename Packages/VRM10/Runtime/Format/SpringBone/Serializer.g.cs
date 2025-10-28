// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_springBone {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_springBone extension)
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
            extensions.Add(VRMC_springBone.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_springBone value)
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

    if(value.Colliders!=null&&value.Colliders.Count()>=1){
        f.Key("colliders");                
        Serialize_Colliders(f, value.Colliders);
    }

    if(value.ColliderGroups!=null&&value.ColliderGroups.Count()>=1){
        f.Key("colliderGroups");                
        Serialize_ColliderGroups(f, value.ColliderGroups);
    }

    if(value.Springs!=null&&value.Springs.Count()>=1){
        f.Key("springs");                
        Serialize_Springs(f, value.Springs);
    }

    f.EndMap();
}

public static void Serialize_Colliders(JsonFormatter f, List<Collider> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_Colliders_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_Colliders_ITEM(JsonFormatter f, Collider value)
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

    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    if(value.Shape!=null){
        f.Key("shape");                
        __colliders_ITEM_Serialize_Shape(f, value.Shape);
    }

    f.EndMap();
}

public static void __colliders_ITEM_Serialize_Shape(JsonFormatter f, ColliderShape value)
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
        __colliders_ITEM__shape_Serialize_Sphere(f, value.Sphere);
    }

    if(value.Capsule!=null){
        f.Key("capsule");                
        __colliders_ITEM__shape_Serialize_Capsule(f, value.Capsule);
    }

    f.EndMap();
}

public static void __colliders_ITEM__shape_Serialize_Sphere(JsonFormatter f, ColliderShapeSphere value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        __colliders_ITEM__shape__sphere_Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __colliders_ITEM__shape__sphere_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __colliders_ITEM__shape_Serialize_Capsule(JsonFormatter f, ColliderShapeCapsule value)
{
    f.BeginMap();


    if(value.Offset!=null&&value.Offset.Count()>=3){
        f.Key("offset");                
        __colliders_ITEM__shape__capsule_Serialize_Offset(f, value.Offset);
    }

    if(value.Radius.HasValue){
        f.Key("radius");                
        f.Value(value.Radius.GetValueOrDefault());
    }

    if(value.Tail!=null&&value.Tail.Count()>=3){
        f.Key("tail");                
        __colliders_ITEM__shape__capsule_Serialize_Tail(f, value.Tail);
    }

    f.EndMap();
}

public static void __colliders_ITEM__shape__capsule_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __colliders_ITEM__shape__capsule_Serialize_Tail(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_ColliderGroups(JsonFormatter f, List<ColliderGroup> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_ColliderGroups_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_ColliderGroups_ITEM(JsonFormatter f, ColliderGroup value)
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

    if(!string.IsNullOrEmpty(value.Name)){
        f.Key("name");                
        f.Value(value.Name);
    }

    if(value.Colliders!=null&&value.Colliders.Count()>=1){
        f.Key("colliders");                
        __colliderGroups_ITEM_Serialize_Colliders(f, value.Colliders);
    }

    f.EndMap();
}

public static void __colliderGroups_ITEM_Serialize_Colliders(JsonFormatter f, int[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_Springs(JsonFormatter f, List<Spring> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_Springs_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_Springs_ITEM(JsonFormatter f, Spring value)
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

    if(!string.IsNullOrEmpty(value.Name)){
        f.Key("name");                
        f.Value(value.Name);
    }

    if(value.Joints!=null&&value.Joints.Count()>=1){
        f.Key("joints");                
        __springs_ITEM_Serialize_Joints(f, value.Joints);
    }

    if(value.ColliderGroups!=null&&value.ColliderGroups.Count()>=1){
        f.Key("colliderGroups");                
        __springs_ITEM_Serialize_ColliderGroups(f, value.ColliderGroups);
    }

    if(value.Center.HasValue){
        f.Key("center");                
        f.Value(value.Center.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __springs_ITEM_Serialize_Joints(JsonFormatter f, List<SpringBoneJoint> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    __springs_ITEM_Serialize_Joints_ITEM(f, item);

    }
    f.EndList();
}

public static void __springs_ITEM_Serialize_Joints_ITEM(JsonFormatter f, SpringBoneJoint value)
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

    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    if(value.HitRadius.HasValue){
        f.Key("hitRadius");                
        f.Value(value.HitRadius.GetValueOrDefault());
    }

    if(value.Stiffness.HasValue){
        f.Key("stiffness");                
        f.Value(value.Stiffness.GetValueOrDefault());
    }

    if(value.GravityPower.HasValue){
        f.Key("gravityPower");                
        f.Value(value.GravityPower.GetValueOrDefault());
    }

    if(value.GravityDir!=null&&value.GravityDir.Count()>=3){
        f.Key("gravityDir");                
        __springs_ITEM__joints_ITEM_Serialize_GravityDir(f, value.GravityDir);
    }

    if(value.DragForce.HasValue){
        f.Key("dragForce");                
        f.Value(value.DragForce.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __springs_ITEM__joints_ITEM_Serialize_GravityDir(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __springs_ITEM_Serialize_ColliderGroups(JsonFormatter f, int[] value)
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
