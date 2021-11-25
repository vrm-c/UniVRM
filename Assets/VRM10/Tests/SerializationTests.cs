using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UniGLTF;
using UniJSON;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    public class SerializationTests
    {
        static string Serialize<T>(T value, Action<JsonFormatter, T> seri)
        {
            var f = new JsonFormatter();
            seri(f, value);
            var b = f.GetStoreBytes();
            return Encoding.UTF8.GetString(b.Array, b.Offset, b.Count);
        }

        [Test]
        public void MaterialTest()
        {
            var q = "\"";

            {
                var data = new UniGLTF.glTFMaterial
                {
                    name = "Some",
                };

                var json = Serialize(data, UniGLTF.GltfSerializer.Serialize_gltf_materials_ITEM);
                Assert.AreEqual($"{{{q}name{q}:{q}Some{q},{q}pbrMetallicRoughness{q}:{{{q}baseColorFactor{q}:[1,1,1,1],{q}metallicFactor{q}:1,{q}roughnessFactor{q}:1}},{q}doubleSided{q}:false}}", json);
            }

            {
                var data = new UniGLTF.glTF();
                data.textures.Add(new UniGLTF.glTFTexture
                {

                });

                var json = Serialize(data, UniGLTF.GltfSerializer.Serialize);
                // Assert.Equal($"{{ {q}name{q}: {q}Some{q} }}", json);
            }

            {
                var data = new UniGLTF.glTFMaterial
                {
                    name = "Alicia_body",
                    pbrMetallicRoughness = new UniGLTF.glTFPbrMetallicRoughness
                    {
                        // BaseColorFactor = new[] { 1, 1, 1, 1 },
                        // BaseColorTexture= { }, 
                        metallicFactor = 0,
                        roughnessFactor = 0.9f
                    },
                    alphaMode = "OPAQUE",
                    alphaCutoff = 0.5f,
                    extensions = new UniGLTF.glTFExtensionExport().Add(
                        UniGLTF.glTF_KHR_materials_unlit.ExtensionName,
                        new ArraySegment<byte>(UniGLTF.glTF_KHR_materials_unlit.Raw))
                };

                var json = Serialize(data, UniGLTF.GltfSerializer.Serialize_gltf_materials_ITEM);
                // Assert.Equal($"{{ {q}name{q}: {q}Some{q} }}", json);
            }
        }

        static void CompareUnityMaterial(Material lhs, Material rhs)
        {
            Assert.AreEqual(lhs.name, rhs.name);
            Assert.AreEqual(lhs.shader, rhs.shader);
            var sb = new StringBuilder();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(lhs.shader); ++i)
            {
                var prop = ShaderUtil.GetPropertyName(lhs.shader, i);
                if (s_ignoreProps.Contains(prop))
                {
                    continue;
                }

                switch (ShaderUtil.GetPropertyType(lhs.shader, i))
                {
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
                        {
                            var l = lhs.GetVector(prop);
                            var r = rhs.GetVector(prop);
                            if (l != r)
                            {
                                sb.AppendLine($"{prop} {l}!={r}");
                            }
                        }
                        break;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
                        {
                            var l = lhs.GetFloat(prop);
                            var r = rhs.GetFloat(prop);
                            if (l != r)
                            {
                                sb.AppendLine($"{prop} {l}!={r}");
                            }
                        }
                        break;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
                        {
                            var l = lhs.GetTextureOffset(prop);
                            var r = rhs.GetTextureOffset(prop);
                            if (l != r)
                            {
                                sb.AppendLine($"{prop} {l}!={r}");
                            }
                        }
                        break;

                    default:
                        throw new NotImplementedException(prop);
                }
            }
            if (sb.Length > 0)
            {
                Debug.LogWarning(sb.ToString());
            }
            Assert.AreEqual(0, sb.Length);
        }

        static string[] s_ignoreKeys = new string[]
        {
            "(MToonMaterial).Definition.MetaDefinition.VersionNumber",
        };

        static string[] s_ignoreProps = new string[]
        {
            "_ReceiveShadowRate",
            "_ShadingGradeRate",
            "_MToonVersion",
            "_Glossiness", // Gloss is burned into the texture and changed to the default value (1.0)
        };

        // /// Unity material を export => import して元の material と一致するか
        // [Test]
        // [TestCase("TestMToon", typeof(UniGLTF.Extensions.VRMC_vrm.MToonMaterial))]
        // [TestCase("TestUniUnlit", typeof(UniGLTF.Extensions.VRMC_vrm.UnlitMaterial))]
        // [TestCase("TestStandard", typeof(UniGLTF.Extensions.VRMC_vrm.PBRMaterial))]
        // [TestCase("TestUnlitColor", typeof(UniGLTF.Extensions.VRMC_vrm.UnlitMaterial), false)]
        // [TestCase("TestUnlitTexture", typeof(UniGLTF.Extensions.VRMC_vrm.UnlitMaterial), false)]
        // [TestCase("TestUnlitTransparent", typeof(UniGLTF.Extensions.VRMC_vrm.UnlitMaterial), false)]
        // [TestCase("TestUnlitCutout", typeof(UniGLTF.Extensions.VRMC_vrm.UnlitMaterial), false)]
        // public void UnityMaterialTest(string materialName, Type vrmLibMaterialType, bool sameShader = true)
        // {
        //     // asset (cerate copy for avoid modify asset)
        //     var src = new Material(Resources.Load<Material>(materialName));

        //     // asset => vrmlib
        //     var converter = new UniVRM10.RuntimeVrmConverter();
        //     // var vrmLibMaterial = converter.Export10(src, (a, b, c, d) => null);
        //     // Assert.AreEqual(vrmLibMaterialType, vrmLibMaterial.GetType());

        //     // // vrmlib => gltf
        //     // var textures = new List<UniGLTF.Extensions.VRMC_vrm.Texture>();
        //     // var (gltfMaterial, hasKhrUnlit) = ToProtobufMaterial(vrmLibMaterial, textures);
        //     // if (gltfMaterial.extensions != null)
        //     // {
        //     //     gltfMaterial.extensions = gltfMaterial.extensions.Deserialize();
        //     // }
        //     // Assert.AreEqual(hasKhrUnlit, glTF_KHR_materials_unlit.IsEnable(gltfMaterial));

        //     // // gltf => json
        //     // var jsonMaterial = Serialize(gltfMaterial, UniGLTF.GltfSerializer.Serialize_gltf_materials_ITEM);

        //     // // gltf <= json
        //     // var deserialized = UniGLTF.GltfDeserializer.Deserialize_gltf_materials_LIST(jsonMaterial.ParseAsJson());

        //     // // vrmlib <= gltf
        //     // var loaded = deserialized.FromGltf(textures);
        //     // // var context = ModelDiffContext.Create();
        //     // // ModelDiffExtensions.MaterialEquals(context, vrmLibMaterial, loaded);
        //     // // var diff = context.List
        //     // // .Where(x => !s_ignoreKeys.Contains(x.Context))
        //     // // .ToArray();
        //     // // if (diff.Length > 0)
        //     // // {
        //     // //     Debug.LogWarning(string.Join("\n", diff.Select(x => $"{x.Context}: {x.Message}")));
        //     // // }
        //     // // Assert.AreEqual(0, diff.Length);

        //     // // <= vrmlib
        //     // var map = new Dictionary<UniGLTF.Extensions.VRMC_vrm.Texture, Texture2D>();
        //     // var dst = UniVRM10.RuntimeUnityMaterialBuilder.CreateMaterialAsset(loaded, hasVertexColor: false, map);
        //     // dst.name = src.name;

        //     // if (sameShader)
        //     // {
        //     //     CompareUnityMaterial(src, dst);
        //     // }
        // }

        [Test]
        public void ExpressionTest()
        {
            var q = "\"";

            {
                var data = new UniGLTF.Extensions.VRMC_vrm.Expression();
                data.OverrideBlink = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block;
                data.MorphTargetBinds = new List<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind>{
                    new UniGLTF.Extensions.VRMC_vrm.MorphTargetBind{
                        Weight=1.0f
                    }
                };

                var json = Serialize(data, UniGLTF.Extensions.VRMC_vrm.GltfSerializer.__expressions_Serialize_Custom_ITEM);
                Assert.AreEqual($"{{{q}morphTargetBinds{q}:[{{{q}weight{q}:1}}],{q}overrideBlink{q}:{q}block{q},{q}overrideLookAt{q}:{q}none{q},{q}overrideMouth{q}:{q}none{q}}}", json);
            }

            {
                // var expression = new UniGLTF.Extensions.VRMC_vrm.Expression(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.Blink, "blink", true)
                // {
                //     OverrideBlink = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.None,
                //     OverrideLookAt = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.Block,
                //     OverrideMouth = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.Blend,
                // };

                // // export
                // var gltf = UniVRM10.ExpressionAdapter.ToGltf(expression, new List<UniGLTF.Extensions.VRMC_vrm.Node>(), new List<UniGLTF.Extensions.VRMC_vrm.Material>());
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none, gltf.OverrideBlink);
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block, gltf.OverrideLookAt);
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend, gltf.OverrideMouth);

                // // import
                // var imported = UniVRM10.ExpressionAdapter.FromGltf(gltf, new List<UniGLTF.Extensions.VRMC_vrm.Node>(), new List<UniGLTF.Extensions.VRMC_vrm.Material>());
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.None, imported.OverrideBlink);
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.Block, imported.OverrideLookAt);
                // Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.Blend, imported.OverrideMouth);
            }

            {
                // // export
                // foreach (var preset in Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)) as UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])
                // {
                //     var expression = new UniGLTF.Extensions.VRMC_vrm.Expression(preset, "", false);

                //     // expect no exception
                //     var gltf = ExpressionAdapter.ToGltf(
                //         expression, 
                //         new List<UniGLTF.Extensions.VRMC_vrm.Node>(),
                //         new List<UniGLTF.Extensions.VRMC_vrm.Material>());
                // }

                // // import 
                // foreach (var preset in Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)) as UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])
                // {
                //     var gltf = new UniGLTF.Extensions.VRMC_vrm.Expression
                //     {
                //         Preset = preset,
                //     };

                //     // expect no exception
                //     ExpressionAdapter.FromGltf(
                //         gltf,
                //         new List<UniGLTF.Extensions.VRMC_vrm.Node>(),
                //         new List<UniGLTF.Extensions.VRMC_vrm.Material>());
                // }
            }
        }
    }
}
