using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// JsonSchema から vrm10 のシリアライザーを生成する。
    /// 
    /// * https://github.com/KhronosGroup/glTF
    /// * https://github.com/vrm-c/vrm-specification
    /// 
    /// を UniVRM の隣に clone しておくこと。
    /// 
    /// * UniVRM
    /// * glTF
    /// * vrm-specification
    /// 
    /// </summary>
    public static class Menu
    {
        [MenuItem(UniVRM10.VRMVersion.MENU + "/Generate from JsonSchema")]
        public static void Main()
        {
            var projectRoot = new DirectoryInfo(Path.GetFullPath(Path.Combine(Application.dataPath, "../")));

            var gltf = new FileInfo(Path.Combine(projectRoot.FullName, "glTF/specification/2.0/schema/glTF.schema.json"));

            var args = new string[]
            {
                // VRMC_vrm
                "vrm-specification/specification/VRMC_vrm-1.0_draft/schema/VRMC_vrm.schema.json",
                "Assets/VRM10/Runtime/Format/Vrm", // dst
                // VRMC_constraints
                "vrm-specification/specification/VRMC_constraints-1.0_draft/schema/VRMC_constraints.schema.json",
                "Assets/VRM10/Runtime/Format/Constraints", // dst
                //
                "vrm-specification/specification/VRMC_materials_mtoon-1.0_draft/schema/VRMC_materials_mtoon.schema.json",
                "Assets/VRM10/Runtime/Format/MaterialsMToon", // dst
                //
                "vrm-specification/specification/VRMC_node_collider_1.0_draft/schema/VRMC_node_collider.json",
                "Assets/VRM10/Runtime/Format/NodeCollider", // dst
                //
                "vrm-specification/specification/VRMC_springBone-1.0_draft/schema/VRMC_springBone.schema.json",
                "Assets/VRM10/Runtime/Format/SpringBone", // dst
            };

            for (int i = 0; i < args.Length; i += 2)
            {
                var extensionSchemaPath = new FileInfo(Path.Combine(projectRoot.FullName, args[i]));
                var parser = new UniGLTF.JsonSchema.JsonSchemaParser(gltf.Directory, extensionSchemaPath.Directory);
                var extensionSchema = parser.Load(extensionSchemaPath, "");
                // extensionSchema.Dump();

                var dst = new DirectoryInfo(Path.Combine(projectRoot.FullName, args[i + 1]));
                Debug.Log(dst);
                GenerateUniGLTFSerialization.Generator.GenerateTo(extensionSchema, dst, clearFolder: true);
            }
        }
    }
}
