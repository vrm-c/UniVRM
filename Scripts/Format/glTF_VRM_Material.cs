using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using UniJSON;
using UniGLTF.ShaderPropExporter;

namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm.material")]
    public class glTF_VRM_Material : JsonSerializableBase
    {
        public string name;
        public string shader;
        public int renderQueue = -1;

        public Dictionary<string, float> floatProperties = new Dictionary<string, float>();
        public Dictionary<string, float[]> vectorProperties = new Dictionary<string, float[]>();
        public Dictionary<string, int> textureProperties = new Dictionary<string, int>();
        public Dictionary<string, bool> keywordMap = new Dictionary<string, bool>();
        public Dictionary<string, string> tagMap = new Dictionary<string, string>();

        static readonly string[] TAGS = new string[]{
            "RenderType",
            // "Queue",
        };

        private static readonly string[] VRMExtensionShaders = new string[]
        {
            "VRM/UnlitTransparentZWrite",
            "VRM/MToon"
        };

        private static readonly string VRM_USE_GLTFSHADER = "VRM_USE_GLTFSHADER";

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => name);
            f.KeyValue(() => renderQueue);
            f.KeyValue(() => shader);
            {
                f.Key("floatProperties"); f.BeginMap();
                foreach (var kv in floatProperties)
                {
                    f.Key(kv.Key); f.Value(kv.Value);
                }
                f.EndMap();
            }
            {
                f.Key("vectorProperties"); f.BeginMap();
                foreach (var kv in vectorProperties)
                {
                    f.Key(kv.Key); f.Value(kv.Value.ToArray());
                }
                f.EndMap();
            }
            {
                f.Key("textureProperties"); f.BeginMap();
                foreach (var kv in textureProperties)
                {
                    f.Key(kv.Key); f.Value(kv.Value);
                }
                f.EndMap();
            }
            {
                f.Key("keywordMap"); f.BeginMap();
                foreach (var kv in keywordMap)
                {
                    f.Key(kv.Key); f.Value(kv.Value);
                }
                f.EndMap();
            }
            {
                f.Key("tagMap"); f.BeginMap();
                foreach (var kv in tagMap)
                {
                    f.Key(kv.Key); f.Value(kv.Value);
                }
                f.EndMap();
            }
        }

        public static List<glTF_VRM_Material> Parse(string src)
        {
            var json = JsonParser.Parse(src)["extensions"]["VRM"]["materialProperties"];
            return Parse(json);
        }

        public static List<glTF_VRM_Material> Parse(JsonNode json)
        {
            var materials = json.DeserializeList<glTF_VRM_Material>();
            var jsonItems = json.ArrayItems.ToArray();
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].floatProperties =
                    jsonItems[i]["floatProperties"].ObjectItems.ToDictionary(x => x.Key, x => x.Value.Value.GetSingle());
                materials[i].vectorProperties =
                    jsonItems[i]["vectorProperties"].ObjectItems.ToDictionary(x => x.Key, x =>
                    {
                        return x.Value.ArrayItems.Select(y => y.Value.GetSingle()).ToArray();
                    });
                materials[i].keywordMap =
                    jsonItems[i]["keywordMap"].ObjectItems.ToDictionary(x => x.Key, x => x.Value.GetBoolean());
                materials[i].tagMap =
                    jsonItems[i]["tagMap"].ObjectItems.ToDictionary(x => x.Key, x => x.Value.GetString());
                materials[i].textureProperties =
                    jsonItems[i]["textureProperties"].ObjectItems.ToDictionary(x => x.Key, x => x.Value.GetInt32());
            }
            return materials;
        }

        public static glTF_VRM_Material CreateFromMaterial(Material m, List<Texture> textures)
        {
            var material = new glTF_VRM_Material
            {
                name = m.name,
                shader = m.shader.name,
                renderQueue = m.renderQueue,
            };

            if (!VRMExtensionShaders.Contains(m.shader.name))
            {
                material.shader = VRM_USE_GLTFSHADER;
                return material;
            }

            var prop = PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            if (prop == null)
            {
                Debug.LogWarningFormat("Fail to export shader: {0}", m.shader.name);
            }
            else
            {
                foreach (var keyword in m.shaderKeywords)
                {
                    material.keywordMap.Add(keyword, m.IsKeywordEnabled(keyword));
                }

                // get properties
                //material.SetProp(prop);
                foreach (var kv in prop.Properties)
                {
                    switch (kv.ShaderPropertyType)
                    {
                        case ShaderPropertyType.Color:
                            {
                                var value = m.GetColor(kv.Key).ToArray();
                                material.vectorProperties.Add(kv.Key, value);
                            }
                            break;

                        case ShaderPropertyType.Range:
                        case ShaderPropertyType.Float:
                            {
                                var value = m.GetFloat(kv.Key);
                                material.floatProperties.Add(kv.Key, value);
                            }
                            break;

                        case ShaderPropertyType.TexEnv:
                            {
                                var texture = m.GetTexture(kv.Key);
                                if (texture != null)
                                {
                                    var value = textures.IndexOf(texture);
                                    if (value == -1)
                                    {
                                        Debug.LogFormat("not found {0}", texture.name);
                                    }
                                    else
                                    {
                                        material.textureProperties.Add(kv.Key, value);
                                    }
                                }

                                // offset & scaling
                                var offset = m.GetTextureOffset(kv.Key);
                                var scaling = m.GetTextureScale(kv.Key);
                                material.vectorProperties.Add(kv.Key,
                                    new float[] { offset.x, offset.y, scaling.x, scaling.y });
                            }
                            break;

                        case ShaderPropertyType.Vector:
                            {
                                var value = m.GetVector(kv.Key).ToArray();
                                material.vectorProperties.Add(kv.Key, value);
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            foreach (var tag in TAGS)
            {
                var value = m.GetTag(tag, false);
                if (!String.IsNullOrEmpty(value))
                {
                    material.tagMap.Add(tag, value);
                }
            }

            return material;
        }
    }
}
