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

    if(value.Roll!=null){
        f.Key("roll");                
        __constraint_Serialize_Roll(f, value.Roll);
    }

    if(value.Aim!=null){
        f.Key("aim");                
        __constraint_Serialize_Aim(f, value.Aim);
    }

    if(value.Rotation!=null){
        f.Key("rotation");                
        __constraint_Serialize_Rotation(f, value.Rotation);
    }

    f.EndMap();
}

public static void __constraint_Serialize_Roll(JsonFormatter f, RollConstraint value)
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
        f.Key("rollAxis");                
        f.Value(value.RollAxis.ToString());
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
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
        f.Key("aimAxis");                
        f.Value(value.AimAxis.ToString());
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
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

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
}

    } // class
} // namespace
