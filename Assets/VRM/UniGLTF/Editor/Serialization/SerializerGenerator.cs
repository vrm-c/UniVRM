using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UniJSON;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class SerializerGenerator
    {
        const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "VRM/UniGLTF/Scripts/IO/FormatterExtensionsGltf.g.cs");
            }
        }

        /// <summary>
        /// AOT向けにシリアライザを生成する
        /// </summary>
        [MenuItem(VRM.VRMVersion.MENU + "/Generate Serializer")]
        static void GenerateSerializer()
        {
            var path = OutPath;

            using (var g = new Generator(path))
            {
                var rootType = typeof(glTF);
                g.Generate(rootType, "gltf");
            }
        }

        class Generator : IDisposable
        {
            String m_path;
            Stream m_s;
            StreamWriter m_w;

            static Dictionary<string, string> s_snippets = new Dictionary<string, string>
            {
                {"gltf/animations", "if(value.animations!=null && value.animations.Count>0)" },
                {"gltf/cameras", "if(value.cameras!=null && value.cameras.Count>0)" },
                {"gltf/buffers", "if(value.buffers!=null && value.buffers.Count>0)" },
                {"gltf/bufferViews", "if(value.bufferViews!=null && value.bufferViews.Count>0)" },
                {"gltf/bufferViews[]/byteStride", "" },
                {"gltf/bufferViews[]/target", "if(value.target!=0)" },
                {"gltf/animations[]/channels", "if(value.channels!=null && value.channels.Count>0)" },
                {"gltf/animations[]/channels[]/target", "if(value!=null)" },
                {"gltf/animations[]/samplers", "if(value.samplers!=null && value.samplers.Count>0)" },
                {"gltf/accessors", "if(value.accessors!=null && value.accessors.Count>0)" },
                {"gltf/accessors[]/max", "if(value.max!=null && value.max.Length>0)"},
                {"gltf/accessors[]/min", "if(value.min!=null && value.min.Length>0)"},
                {"gltf/accessors[]/sparse", "if(value.sparse!=null && value.sparse.count>0)"},
                {"gltf/accessors[]/bufferView", "if(value.bufferView>=0)"},
                {"gltf/accessors[]/byteOffset", "if(value.bufferView>=0)"},

                {"gltf/images", "if(value.images!=null && value.images.Count>0)" },

                {"gltf/meshes", "if(value.meshes!=null && value.meshes.Count>0)" },
                {"gltf/meshes[]/primitives", "if(value.primitives!=null && value.primitives.Count>0)" },
                {"gltf/meshes[]/primitives[]/targets", "if(value.targets!=null && value.targets.Count>0)" },

                {"gltf/meshes[]/primitives[]/targets[]/POSITION", "if(value.POSITION!=-1)" },
                {"gltf/meshes[]/primitives[]/targets[]/NORMAL", "if(value.NORMAL!=-1)" },
                {"gltf/meshes[]/primitives[]/targets[]/TANGENT", "if(value.TANGENT!=-1)" },

                {"gltf/meshes[]/primitives[]/attributes/POSITION", "if(value.POSITION!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/NORMAL", "if(value.NORMAL!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/TANGENT", "if(value.TANGENT!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/TEXCOORD_0", "if(value.TEXCOORD_0!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/COLOR_0", "if(value.COLOR_0!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/JOINTS_0", "if(value.JOINTS_0!=-1)"},
                {"gltf/meshes[]/primitives[]/attributes/WEIGHTS_0", "if(value.WEIGHTS_0!=-1)"},

                {"gltf/meshes[]/primitives[]/extras", "if(value.extras!=null && value.extras.targetNames!=null && value.extras.targetNames.Count>0)"},
                {"gltf/meshes[]/weights", "if(value.weights!=null && value.weights.Length>0)" },
                {"gltf/materials", "if(value.materials!=null && value.materials.Count>0)" },
                {"gltf/materials[]/alphaCutoff", "if(value.alphaMode == \"MASK\")" },
                {"gltf/nodes", "if(value.nodes!=null && value.nodes.Count>0)" },
                {"gltf/nodes[]/camera", "if(value.camera!=-1)"},
                {"gltf/nodes[]/mesh", "if(value.mesh!=-1)"},
                {"gltf/nodes[]/skin", "if(value.skin!=-1)"},
                {"gltf/nodes[]/children", "if(value.children != null && value.children.Length>0)"},
                {"gltf/samplers", "if(value.samplers!=null && value.samplers.Count>0)" },
                {"gltf/scenes", "if(value.scenes!=null && value.scenes.Count>0)" },
                {"gltf/scenes[]/nodes", "if(value.nodes!=null && value.nodes.Length>0)" },
                {"gltf/skins", "if(value.skins!=null && value.skins.Count>0)" },
                {"gltf/skins[]/skeleton", "if(value.skeleton!=-1)"},
                {"gltf/skins[]/joints", "if(value.joints!=null && value.joints.Length>0)"},
                {"gltf/extensionsUsed", "if(value.extensionsUsed!=null && value.extensionsUsed.Count>0)"}, // dummy
                {"gltf/extensionsRequired", "if(false && value.extensionsRequired!=null && value.extensionsRequired.Count>0)"},
                {"gltf/extensions/VRM/humanoid/humanBones[]/axisLength", "if(value.axisLength>0)"},
                {"gltf/extensions/VRM/humanoid/humanBones[]/center", "if(value.center!=Vector3.zero)"},
                {"gltf/extensions/VRM/humanoid/humanBones[]/max", "if(value.max!=Vector3.zero)"},
                {"gltf/extensions/VRM/humanoid/humanBones[]/min", "if(value.min!=Vector3.zero)"},
                {"gltf/textures", "if(value.textures!=null && value.textures.Count>0)" },
            };

            public Generator(string path)
            {
                m_path = path;
                m_s = File.Open(path, FileMode.Create);
                m_w = new StreamWriter(m_s, Encoding.UTF8);

                // begin
                m_w.Write(@"
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace UniGLTF {

    static public class IFormatterExtensionsGltf
    {

");
            }

            public void Dispose()
            {
                // end
                m_w.Write(@"
    } // class
} // namespace
");

                m_w.Dispose();
                m_s.Dispose();
                UnityPath.FromFullpath(m_path).ImportAsset();
            }

            HashSet<Type> m_used = new HashSet<Type>
            {
                typeof(object),
            };

            public void Generate(Type t, string path, int level = 0)
            {
                if (m_used.Contains(t))
                {
                    // 処理済み
                    return;
                }
                m_used.Add(t);

                // primitive
                try
                {
                    var mi = typeof(IFormatter).GetMethod("Value", new Type[] { t });
                    if (mi != null)
                    {
                        m_w.Write(@"
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.Value(value);
    }
".Replace("$0", t.Name));

                        return;
                    }
                }
                catch (AmbiguousMatchException)
                {
                    // skip
                }

                if (t.IsEnum)
                {
                    m_w.Write(@"
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.Value((int)value);
    }
".Replace("$0", t.Name));
                }
                else if (t.IsArray)
                {
                    var et = t.GetElementType();
                    m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, $0[] value)
    {
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    "
                    .Replace("$0", et.Name)
                    .Replace("$1", path)
                    );
                    Generate(et, path + "[]", level + 1);
                }
                else if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var et = t.GetGenericArguments()[0];
                        m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, List<$0> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
"
.Replace("$0", et.Name)
.Replace("$1", path));
                        Generate(et, path + "[]", level + 1);
                    }
                    else if (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    && t.GetGenericArguments()[0] == typeof(string))
                    {
                        var et = t.GetGenericArguments()[1];
                        m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, Dictionary<string, $0> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }

"
.Replace("$0", et.Name)
.Replace("$1", path));
                        Generate(et, path + "{}", level + 1);
                    }
                    else
                    {
                        Debug.LogWarningFormat("unknown type: {0}", t);
                    }
                }
                else
                {
                    Debug.LogFormat("{0}({1})", path, t.Name);

                    m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.BeginMap(0); // dummy
"
.Replace("$0", t.Name)
.Replace("$1", path)
);

                    foreach (var fi in t.GetFields(FIELD_FLAGS))
                    {
                        if (fi.FieldType == typeof(object))
                        {
                            continue;
                        }
                        if (fi.IsLiteral && !fi.IsInitOnly)
                        {
                            continue;
                        }
                        if (fi.FieldType == typeof(string) || fi.FieldType.IsEnum || fi.FieldType.IsArray || fi.FieldType.IsGenericType)
                        {

                        }
                        else if (fi.FieldType == typeof(glTF_KHR_materials_unlit))
                        {

                        }
                        else if (fi.FieldType.IsClass && fi.FieldType.GetFields(FIELD_FLAGS).Length == 0)
                        {
                            continue;
                        }

                        var snipet = fi.FieldType.IsClass ? "if(value." + fi.Name + "!=null)" : "";
                        var value = default(string);
                        if (s_snippets.TryGetValue(path + "/" + fi.Name, out value))
                        {
                            snipet = value;
                        }

                        if (value == "")
                        {
                            // found, but empty
                        }
                        else
                        {
                            m_w.Write(@"
        $1
        {
            f.Key(""$0""); f.GenSerialize(value.$0);
        }
"
    .Replace("$0", fi.Name)
    .Replace("$1", snipet)
    );
                        }
                    }

                    m_w.Write(@"
        f.EndMap();
    }
");

                    foreach (var fi in t.GetFields(FIELD_FLAGS))
                    {
                        Generate(fi.FieldType, path + "/" + fi.Name, level + 1);
                    }
                }
            }
        }
    }
}
