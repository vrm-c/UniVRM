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

        static List<string> Deserialize(ListTreeNode<JsonValue> json)
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
            if (mesh.extras != null)
            {
                foreach (var kv in mesh.extras.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                    {
                        targetNames = Deserialize(kv.Value);
                        return true;
                    }
                }
            }

            // use first primitive
            if (mesh.primitives.Count > 0 && mesh.primitives[0].extras != null)
            {
                foreach (var kv in mesh.primitives[0].extras.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                    {
                        targetNames = Deserialize(kv.Value);
                        return true;
                    }
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

            return glTFExtension.Create(ExtraName, f.GetStore().ToString());
        }

        public static void Serialize(glTFMesh gltfMesh, IEnumerable<string> targetNames)
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

            if (gltfMesh.extras == null)
            {
                gltfMesh.extras = new glTFExtension();
            }
            gltfMesh.extras.Serialized.Add(ExtraName, targetNamesJson);

            foreach (var prim in gltfMesh.primitives)
            {
                if (prim.extras == null)
                {
                    prim.extras = new glTFExtension();
                }
                prim.extras.Serialized.Add(ExtraName, targetNamesJson);
            }
        }
    }
}
