using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// JsonSchema から vrm10 のシリアライザーを生成する。
    /// 
    /// * glTF
    /// * vrm-specification
    /// 
    /// は SubModuleになった。 `$ git submodule update --init` しておくこと。
    /// 
    /// </summary>
    public static class Vrm10SerializerGenerator
    {
        private struct GenerateInfo
        {
            public string JsonSchema;
            public string FormatDir;
            public string SerializerDir;

            public GenerateInfo(string jsonSchema, string formatDir, string serializerDir)
            {
                JsonSchema = jsonSchema;
                FormatDir = formatDir;
                SerializerDir = serializerDir;
            }

            public GenerateInfo(string jsonSchema, string formatDir) : this(jsonSchema, formatDir, formatDir)
            {
            }
        }

        private const string Vrm10SpecDir = "vrm-specification/specification";
        private const string Vrm10FormatGeneratedDir = "Assets/VRM10/Runtime/Format";

        public static void Run(bool debug)
        {
            var projectRoot = new DirectoryInfo(Path.GetFullPath(Path.Combine(Application.dataPath, "../")));

            var gltf = new FileInfo(Path.Combine(projectRoot.FullName, "glTF/specification/2.0/schema/glTF.schema.json"));

            var args = new GenerateInfo[]
            {
                // VRMC_hdr_emissiveMultiplier
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_materials_hdr_emissiveMultiplier-1.0/schema/VRMC_materials_hdr_emissiveMultiplier.json",
                    "Assets/UniGLTF/Runtime/UniGLTF/Format/ExtensionsAndExtras/EmissiveMultiplier"
                ),

                // VRMC_vrm
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_vrm-1.0/schema/VRMC_vrm.schema.json",
                    $"{Vrm10FormatGeneratedDir}/Vrm"
                ),

                // VRMC_materials_mtoon
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_materials_mtoon-1.0/schema/VRMC_materials_mtoon.schema.json",
                    $"{Vrm10FormatGeneratedDir}/MaterialsMToon"
                ),

                // VRMC_springBone
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_springBone-1.0/schema/VRMC_springBone.schema.json",
                    $"{Vrm10FormatGeneratedDir}/SpringBone"
                ),

                // VRMC_node_constraint
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_node_constraint-1.0/schema/VRMC_node_constraint.schema.json",
                    $"{Vrm10FormatGeneratedDir}/Constraints"
                ),

                // VRMC_animation
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_vrm_animation-1.0/schema/VRMC_vrm_animation.schema.json",
                    $"{Vrm10FormatGeneratedDir}/Animation"
                ),

                // VRMC_animation
                new GenerateInfo(
                    $"{Vrm10SpecDir}/VRMC_springBone_extended_collider-1.0/schema/VRMC_springBone_extended_collider.schema.json",
                    $"{Vrm10FormatGeneratedDir}/SpringBoneExtendedCollider"
                ),
            };

            foreach (var arg in args)
            {
                var extensionSchemaPath = new FileInfo(Path.Combine(projectRoot.FullName, arg.JsonSchema));
                var parser = new UniGLTF.JsonSchema.JsonSchemaParser(gltf.Directory, extensionSchemaPath.Directory);
                var extensionSchema = parser.Load(extensionSchemaPath, "");

                var formatDst = new DirectoryInfo(Path.Combine(projectRoot.FullName, arg.FormatDir));
                Debug.Log($"Format.g Dir: {formatDst}");

                var serializerDst = new DirectoryInfo(Path.Combine(projectRoot.FullName, arg.SerializerDir));
                Debug.Log($"Serializer/Deserializer.g Dir: {serializerDst}");

                if (debug)
                {
                    Debug.Log(extensionSchema.Dump());
                }
                else
                {
                    GenerateUniGLTFSerialization.Generator.GenerateTo(extensionSchema, formatDst, serializerDst);
                }
            }
        }
    }
}
