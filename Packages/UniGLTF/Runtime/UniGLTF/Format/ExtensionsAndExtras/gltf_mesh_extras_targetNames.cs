using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// work around
    /// 
    /// https://github.com/KhronosGroup/glTF/issues/1036
    /// 
    /// * meshes[].primitives[].extras.targetNames
    /// * meshes[].extras.targetNames
    /// 
    /// </summary>
    public static class gltf_mesh_extras_targetNames
    {
        public const string ExtraName = "targetNames";

        public static readonly Utf8String ExtraNameUtf8 = Utf8String.From(ExtraName);

        static List<string> Deserialize(JsonNode json)
        {
            var targetNames = new List<string>();
            if (json.Value.ValueType == ValueNodeType.Array)
            {
                foreach (var name in json.ArrayItems())
                {
                    targetNames.Add(name.GetString());
                }
            }
            return targetNames;
        }

        public static bool TryGet(glTFMesh mesh, out List<string> targetNames)
        {
            if (mesh.extras is glTFExtensionImport meshExtras)
            {
                foreach (var kv in meshExtras.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                    {
                        targetNames = Deserialize(kv.Value);
                        return true;
                    }
                }
            }

            // use first primitive
            if (mesh.primitives.Count > 0 && mesh.primitives[0].extras is glTFExtensionImport primExtras)
            {
                foreach (var kv in primExtras.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                    {
                        targetNames = Deserialize(kv.Value);
                        return true;
                    }
                }
            }

            if (mesh.primitives.Count > 0)
            {
                var prim = mesh.primitives[0];
                if (prim.targets.Count > 0)
                {
                    // 名無しには連番を付ける
                    targetNames = new List<string>();
                    for (int i = 0; i < prim.targets.Count; ++i)
                    {
                        targetNames.Add($"{i}");
                    }
                    return true;
                }
            }

            targetNames = default;
            return false;
        }

        public static glTFExtension Serialize(params string[] args)
        {
            var f = new JsonFormatter();
            f.BeginList();
            foreach (var arg in args)
            {
                // エスケープとかあるし
                f.Value(arg);
            }
            f.EndList();

            return new glTFExtensionExport().Add(ExtraName, f.GetStore().Bytes);
        }

        public static void Serialize(glTFMesh gltfMesh, IEnumerable<string> targetNames, BlendShapeTargetNameLocationFlags blendShapeTargetNameLocation)
        {
            // targetNames
            var f = new JsonFormatter();
            f.BeginList();
            foreach (var n in targetNames)
            {
                f.Value(n);
            }
            f.EndList();
            var targetNamesJson = f.GetStore().Bytes;

            // mesh
            if (blendShapeTargetNameLocation.HasFlag(BlendShapeTargetNameLocationFlags.Mesh))
            {
                var meshExtras = glTFExtensionExport.GetOrCreate(ref gltfMesh.extras);
                meshExtras.Add(ExtraName, targetNamesJson);
            }

            // primitive
            if (blendShapeTargetNameLocation.HasFlag(BlendShapeTargetNameLocationFlags.Primitives))
            {
                foreach (var prim in gltfMesh.primitives)
                {
                    var primExtras = glTFExtensionExport.GetOrCreate(ref prim.extras);
                    primExtras.Add(ExtraName, targetNamesJson);
                }
            }
        }
    }
}
