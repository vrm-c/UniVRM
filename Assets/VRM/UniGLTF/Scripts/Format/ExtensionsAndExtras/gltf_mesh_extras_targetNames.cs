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
    public class gltf_mesh_extras_targetNames
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
            {
                if (mesh.extras is ListTreeNode<JsonValue> json)
                {
                    if (json.Value.ValueType == ValueNodeType.Object)
                    {
                        foreach (var kv in json.ObjectItems())
                        {
                            if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                            {
                                targetNames = Deserialize(kv.Value);
                                return true;
                            }
                        }
                    }
                }
            }

            {
                // use first primitive
                if (mesh.primitives.Count > 0 && mesh.primitives[0].extras is ListTreeNode<JsonValue> json)
                {
                    if (json.Value.ValueType == ValueNodeType.Object)
                    {
                        foreach (var kv in json.ObjectItems())
                        {
                            if (kv.Key.GetUtf8String() == ExtraNameUtf8)
                            {
                                targetNames = Deserialize(kv.Value);
                                return true;
                            }
                        }
                    }
                }
            }

            targetNames = default;
            return false;
        }
    }
}
