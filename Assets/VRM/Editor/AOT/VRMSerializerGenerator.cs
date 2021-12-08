using System.IO;
using System.Reflection;
using System.Text;
using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    internal static class VRMSerializerGenerator
    {
        const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "VRM/Runtime/Format/VRMSerializer.g.cs");
            }
        }

        const string Begin = @"
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace VRM {

    static public class VRMSerializer
    {

";
        const string End = @"
    } // class
} // namespace
";

        /// <summary>
        /// AOT向けにシリアライザを生成する
        /// </summary>
        public static void GenerateCode()
        {
            var info = new ObjectSerialization(typeof(glTF_VRM_extensions), "vrm", "Serialize_");
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
