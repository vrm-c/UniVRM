using System;
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

        const string Begin = @"using System;
using System.Collections.Generic;
using UniJSON;
using System.Linq;

namespace UniGLTF {

    static public class GltfSerializer
    {

";

        const string End = @"
    } // class
} // namespace
";

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "UniGLTF/Runtime/UniGLTF/Format/GltfSerializer.g.cs");
            }
        }

        public static void GenerateSerializer()
        {
            var info = new ObjectSerialization(typeof(glTF), "gltf", "Serialize_");
            Debug.Log(info);

            using (var s = File.Open(OutPath, FileMode.Create))
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                w.Write(Begin);
                info.GenerateSerializer(w, "Serialize");
                w.Write(End);
            }

            Debug.LogFormat("write: {0}", OutPath);
            UnityPath.FromFullpath(OutPath).ImportAsset();
        }
    }
}
