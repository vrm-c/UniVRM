using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Generate deserializer from JsonNode to glTF using type reflection
    /// </summary>
    public static class DeserializerGenerator
    {
        public const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        const string Begin = @"using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF {

public static class GltfDeserializer
{

";

        const string End = @"
} // GltfDeserializer
} // UniGLTF 
";

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "UniGLTF/UniGLTF/Scripts/IO/GltfDeserializer.g.cs");
            }
        }

        public static void GenerateSerializer()
        {
            var info = new ObjectSerialization(typeof(glTF), "gltf", "Deserialize_");
            Debug.Log(info);

            using (var s = File.Open(OutPath, FileMode.Create))
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                w.Write(Begin);
                info.GenerateDeserializer(w, "Deserialize");
                w.Write(End);
            }

            Debug.LogFormat("write: {0}", OutPath);
            UnityPath.FromFullpath(OutPath).ImportAsset();
        }
    }
}
