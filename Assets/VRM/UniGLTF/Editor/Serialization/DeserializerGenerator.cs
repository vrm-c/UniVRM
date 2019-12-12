using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class DeserializerGenerator
    {
        public const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "VRM/UniGLTF/Scripts/IO/GltfDeserializer.g.cs");
            }
        }

        /// <summary>
        /// AOT向けにデシリアライザを生成する
        /// </summary>
        [MenuItem(VRM.VRMVersion.MENU + "/Generate Deserializer")]
        static void GenerateSerializer()
        {
            var info = new ObjectSerialization(typeof(glTF), "gltf");
            Debug.Log(info);

            using (var s = File.Open(OutPath, FileMode.Create))
            using (var w = new StreamWriter(s, Encoding.UTF8))
            {
                // header
                w.Write(@"
using UniJSON;
using System;
using System.Collections.Generic;
using VRM;
using UnityEngine;

namespace UniGLTF {

public static class GltfDeserializer
{

");

                info.GenerateDeserializer(w, "Deserialize");

                // footer
                w.Write(@"
} // GltfDeserializer
} // UniGLTF 
");

                Debug.LogFormat("write: {0}", OutPath);
            }
        }
    }
}
