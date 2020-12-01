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

        public static List<glTF_VRM_Material> Parse(string src)
        {
            var json = JsonParser.Parse(src)["extensions"]["VRM"]["materialProperties"];
            return Parse(json);
        }

        static Utf8String s_floatProperties = Utf8String.From("floatProperties");
        static Utf8String s_vectorProperties = Utf8String.From("vectorProperties");
        static Utf8String s_keywordMap = Utf8String.From("keywordMap");
        static Utf8String s_tagMap = Utf8String.From("tagMap");
        static Utf8String s_textureProperties = Utf8String.From("textureProperties");

        public static List<glTF_VRM_Material> Parse(ListTreeNode<JsonValue> json)
        {
            var materials = json.DeserializeList<glTF_VRM_Material>();
            var jsonItems = json.ArrayItems().ToArray();
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].floatProperties =
                    jsonItems[i][s_floatProperties].ObjectItems().ToDictionary(x => x.Key.GetString(), x => x.Value.GetSingle());
                materials[i].vectorProperties =
                    jsonItems[i][s_vectorProperties].ObjectItems().ToDictionary(x => x.Key.GetString(), x =>
                    {
                        return x.Value.ArrayItems().Select(y => y.GetSingle()).ToArray();
                    });
                materials[i].keywordMap =
                    jsonItems[i][s_keywordMap].ObjectItems().ToDictionary(x => x.Key.GetString(), x => x.Value.GetBoolean());
                materials[i].tagMap =
                    jsonItems[i][s_tagMap].ObjectItems().ToDictionary(x => x.Key.GetString(), x => x.Value.GetString());
                materials[i].textureProperties =
                    jsonItems[i][s_textureProperties].ObjectItems().ToDictionary(x => x.Key.GetString(), x => x.Value.GetInt32());
            }
            return materials;
        }
    }
}
