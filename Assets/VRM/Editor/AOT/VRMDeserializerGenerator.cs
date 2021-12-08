using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    internal static class VRMDeserializerGenerator
    {
        public const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "VRM/UniVRM/Scripts/Format/VRMDeserializer.g.cs");
            }
        }

        /// <summary>
        /// AOT向けにデシリアライザを生成する
        /// </summary>
        public static void GenerateCode()
        {
            var info = new UniGLTF.ObjectSerialization(typeof(glTF_VRM_extensions), "vrm", "_Deserialize");
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

namespace VRM {

public static class VrmDeserializer
{

");

                info.GenerateDeserializer(w, "Deserialize");

                // footer
                w.Write(@"
} // VrmfDeserializer
} // VRM
");

                Debug.LogFormat("write: {0}", OutPath);
            }
        }
    }
}
