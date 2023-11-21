// namespace UniGLTF.MeshUtility
// {
//         [Serializable]
//         struct MaterialKey
//         {
//             public string Shader;
//             public KeyValuePair<string, object>[] Properties;

//             public override bool Equals(object obj)
//             {
//                 if (!(obj is MaterialKey))
//                 {
//                     return base.Equals(obj);
//                 }

//                 var key = (MaterialKey)obj;

//                 return Shader == key.Shader
//                     && Properties.SequenceEqual(key.Properties)
//                     ;
//             }

//             public override int GetHashCode()
//             {
//                 return base.GetHashCode();
//             }
//         }

//         [Serializable]
//         struct MaterialList
//         {
//             public Material[] Materials;

//             public MaterialList(Material[] list)
//             {
//                 Materials = list;
//             }
//         }

//         static object GetPropertyValue(Shader shader, int i, Material m)
//         {
//             var propType = ShaderUtil.GetPropertyType(shader, i);
//             switch (propType)
//             {
//                 case ShaderUtil.ShaderPropertyType.Color:
//                     return m.GetColor(ShaderUtil.GetPropertyName(shader, i));

//                 case ShaderUtil.ShaderPropertyType.Range:
//                 case ShaderUtil.ShaderPropertyType.Float:
//                     return m.GetFloat(ShaderUtil.GetPropertyName(shader, i));

//                 case ShaderUtil.ShaderPropertyType.Vector:
//                     return m.GetVector(ShaderUtil.GetPropertyName(shader, i));

//                 case ShaderUtil.ShaderPropertyType.TexEnv:
//                     return m.GetTexture(ShaderUtil.GetPropertyName(shader, i));

//                 default:
//                     throw new NotImplementedException(propType.ToString());
//             }
//         }

//         static MaterialKey GetMaterialKey(Material m)
//         {
//             var key = new MaterialKey
//             {
//                 Shader = m.shader.name,
//             };

//             key.Properties = Enumerable.Range(0, ShaderUtil.GetPropertyCount(m.shader))
//                 .Select(x => new KeyValuePair<string, object>(
//                     ShaderUtil.GetPropertyName(m.shader, x),
//                     GetPropertyValue(m.shader, x, m))
//                     )
//                 .OrderBy(x => x.Key)
//                 .ToArray()
//                     ;

//             return key;
//         }

// }