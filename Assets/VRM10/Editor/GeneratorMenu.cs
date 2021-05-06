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
    public static class Menu
    {
#if VRM_DEVELOP        
        [MenuItem(UniVRM10.VRMVersion.MENU + "/Generate from JsonSchema")]
        public static void Generate()
        {
            Run(false);
        }

        [MenuItem(UniVRM10.VRMVersion.MENU + "/Generate from JsonSchema(debug)")]
        public static void Parse()
        {
            Run(true);
        }
#endif

        static void Run(bool debug)
        {
            var projectRoot = new DirectoryInfo(Path.GetFullPath(Path.Combine(Application.dataPath, "../")));

            var gltf = new FileInfo(Path.Combine(projectRoot.FullName, "glTF/specification/2.0/schema/glTF.schema.json"));

            var args = new string[]
            {
                // VRMC_vrm
                "vrm-specification/specification/VRMC_vrm-1.0_draft/schema/VRMC_vrm.schema.json",
                "Assets/VRM10/Runtime/Format/Vrm", // format
                "Assets/VRM10/Runtime/Format/Vrm", // serializer
                // VRMC_node_constraint
                "vrm-specification/specification/VRMC_node_constraint-1.0_draft/schema/VRMC_node_constraint.schema.json",
                "Assets/VRM10/Runtime/Format/Constraints", // format
                "Assets/VRM10/Runtime/Format/Constraints", // serializer
                // VRMC_materials_mtoon
                "vrm-specification/specification/VRMC_materials_mtoon-1.0_draft/schema/VRMC_materials_mtoon.schema.json",
                "Assets/VRMShaders/VRM10/Format/Runtime/MaterialsMToon", // format
                "Assets/VRM10/Runtime/Format/MaterialsMToon", // serializer
                // VRMC_springBone
                "vrm-specification/specification/VRMC_springBone-1.0_draft/schema/VRMC_springBone.schema.json",
                "Assets/VRM10/Runtime/Format/SpringBone", // format
                "Assets/VRM10/Runtime/Format/SpringBone", // serializer
            };

            for (int i = 0; i < args.Length; i += 3)
            {
                var extensionSchemaPath = new FileInfo(Path.Combine(projectRoot.FullName, args[i]));
                var parser = new UniGLTF.JsonSchema.JsonSchemaParser(gltf.Directory, extensionSchemaPath.Directory);
                var extensionSchema = parser.Load(extensionSchemaPath, "");

                var formatDst = new DirectoryInfo(Path.Combine(projectRoot.FullName, args[i + 1]));
                Debug.Log($"Format.g Dir: {formatDst}");
                
                var serializerDst = new DirectoryInfo(Path.Combine(projectRoot.FullName, args[i + 2]));
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
