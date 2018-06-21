using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VRM
{
    public class glTF_VRM_Material : UniGLTF.JsonSerializableBase
    {
        public string name;
        public string shader;
        public int renderQueue=-1;

        public Dictionary<string, float> floatProperties = new Dictionary<string, float>();
        public Dictionary<string, float[]> vectorProperties = new Dictionary<string, float[]>();
        public Dictionary<string, int> textureProperties = new Dictionary<string, int>();
        public Dictionary<string, bool> keywordMap = new Dictionary<string, bool>();
        public Dictionary<string, string> tagMap = new Dictionary<string, string>();

        static readonly string[] TAGS = new string[]{
            "RenderType",
            // "Queue",
        };

        protected override void SerializeMembers(JsonFormatter f)
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
                foreach(var kv in keywordMap)
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
            var json = src.ParseAsJson()["extensions"]["VRM"]["materialProperties"];
            var materials = json.DeserializeList<glTF_VRM_Material>();
            var jsonItems = json.ListItems.ToArray();
            for(int i=0; i<materials.Count; ++i)
            {
                materials[i].floatProperties = 
                    jsonItems[i]["floatProperties"].ObjectItems.ToDictionary(x => x.Key, x => x.Value.GetSingle());
                materials[i].vectorProperties =
                    jsonItems[i]["vectorProperties"].ObjectItems.ToDictionary(x => x.Key, x =>
                    {
                        return x.Value.ListItems.Select(y => y.GetSingle()).ToArray();
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

#if UNITY_EDITOR
            for (int i = 0; i < ShaderUtil.GetPropertyCount(m.shader); ++i)
            {
                var name = ShaderUtil.GetPropertyName(m.shader, i);
                var propType = ShaderUtil.GetPropertyType(m.shader, i);
                switch (propType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        {
                            var value = m.GetColor(name).ToArray();
                            material.vectorProperties.Add(name, value);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Range:
                    case ShaderUtil.ShaderPropertyType.Float:
                        {
                            var value = m.GetFloat(name);
                            material.floatProperties.Add(name, value);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        {
                            var texture = m.GetTexture(name) as Texture2D;
                            if (texture != null)
                            {
                                var value = textures.IndexOf(texture);
                                if (value == -1)
                                {
                                    Debug.LogFormat("not found {0}", texture.name);
                                }
                                else
                                {
                                    material.textureProperties.Add(name, value);
                                }
                            }

                            // offset & scaling
                            var offset = m.GetTextureOffset(name);
                            var scaling = m.GetTextureScale(name);
                            material.vectorProperties.Add(name, 
                                new float[] { offset.x, offset.y, scaling.x, scaling.y });
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Vector:
                        {
                            var value = m.GetVector(name).ToArray();
                            material.vectorProperties.Add(name, value);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
#else
            Debug.LogWarning("cannot export material properties on runtime");           
#endif

            foreach (var keyword in m.shaderKeywords)
            {
                material.keywordMap.Add(keyword, m.IsKeywordEnabled(keyword));
            }

            foreach(var tag in TAGS)
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
