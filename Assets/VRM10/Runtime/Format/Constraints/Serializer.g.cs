// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_node_constraint {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_node_constraint extension)
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
            extensions.Add(VRMC_node_constraint.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_node_constraint value)
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

    if(value.Constraint!=null){
        f.Key("constraint");                
        Serialize_Constraint(f, value.Constraint);
    }

    f.EndMap();
}

public static void Serialize_Constraint(JsonFormatter f, Constraint value)
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

    if(value.Position!=null){
        f.Key("position");                
        __constraint_Serialize_Position(f, value.Position);
    }

    if(value.Rotation!=null){
        f.Key("rotation");                
        __constraint_Serialize_Rotation(f, value.Rotation);
    }

    if(value.Aim!=null){
        f.Key("aim");                
        __constraint_Serialize_Aim(f, value.Aim);
    }

    f.EndMap();
}

public static void __constraint_Serialize_Position(JsonFormatter f, PositionConstraint value)
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

    if(value.Source.HasValue){
        f.Key("source");                
        f.Value(value.Source.GetValueOrDefault());
    }

    if(true){
        f.Key("sourceSpace");                
        f.Value(value.SourceSpace.ToString());
    }

    if(true){
        f.Key("destinationSpace");                
        f.Value(value.DestinationSpace.ToString());
    }

    if(value.FreezeAxes!=null&&value.FreezeAxes.Count()>=3){
        f.Key("freezeAxes");                
        __constraint__position_Serialize_FreezeAxes(f, value.FreezeAxes);
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __constraint__position_Serialize_FreezeAxes(JsonFormatter f, bool[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __constraint_Serialize_Rotation(JsonFormatter f, RotationConstraint value)
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

    if(value.Source.HasValue){
        f.Key("source");                
        f.Value(value.Source.GetValueOrDefault());
    }

    if(true){
        f.Key("sourceSpace");                
        f.Value(value.SourceSpace.ToString());
    }

    if(true){
        f.Key("destinationSpace");                
        f.Value(value.DestinationSpace.ToString());
    }

    if(value.FreezeAxes!=null&&value.FreezeAxes.Count()>=3){
        f.Key("freezeAxes");                
        __constraint__rotation_Serialize_FreezeAxes(f, value.FreezeAxes);
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __constraint__rotation_Serialize_FreezeAxes(JsonFormatter f, bool[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __constraint_Serialize_Aim(JsonFormatter f, AimConstraint value)
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

    if(value.Source.HasValue){
        f.Key("source");                
        f.Value(value.Source.GetValueOrDefault());
    }

    if(true){
        f.Key("sourceSpace");                
        f.Value(value.SourceSpace.ToString());
    }

    if(true){
        f.Key("destinationSpace");                
        f.Value(value.DestinationSpace.ToString());
    }

    if(value.AimVector!=null&&value.AimVector.Count()>=3){
        f.Key("aimVector");                
        __constraint__aim_Serialize_AimVector(f, value.AimVector);
    }

    if(value.UpVector!=null&&value.UpVector.Count()>=3){
        f.Key("upVector");                
        __constraint__aim_Serialize_UpVector(f, value.UpVector);
    }

    if(value.FreezeAxes!=null&&value.FreezeAxes.Count()>=2){
        f.Key("freezeAxes");                
        __constraint__aim_Serialize_FreezeAxes(f, value.FreezeAxes);
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __constraint__aim_Serialize_AimVector(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __constraint__aim_Serialize_UpVector(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __constraint__aim_Serialize_FreezeAxes(JsonFormatter f, bool[] value)
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
