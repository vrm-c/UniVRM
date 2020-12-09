using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniJSON;


namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm.material")]
    public class glTF_VRM_Material
    {
        public string name;
        public string shader;
        public int renderQueue = -1;

        public Dictionary<string, float> floatProperties = new Dictionary<string, float>();
        public Dictionary<string, float[]> vectorProperties = new Dictionary<string, float[]>();
        public Dictionary<string, int> textureProperties = new Dictionary<string, int>();
        public Dictionary<string, bool> keywordMap = new Dictionary<string, bool>();
        public Dictionary<string, string> tagMap = new Dictionary<string, string>();

        public static readonly string VRM_USE_GLTFSHADER = "VRM_USE_GLTFSHADER";

        static Utf8String s_floatProperties = Utf8String.From("floatProperties");
        static Utf8String s_vectorProperties = Utf8String.From("vectorProperties");
        static Utf8String s_keywordMap = Utf8String.From("keywordMap");
        static Utf8String s_tagMap = Utf8String.From("tagMap");
        static Utf8String s_textureProperties = Utf8String.From("textureProperties");
    }
}
