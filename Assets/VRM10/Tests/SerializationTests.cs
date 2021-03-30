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

        static (UniGLTF.glTFMaterial, bool) ToProtobufMaterial(VrmLib.Material vrmlibMaterial, List<VrmLib.Texture> textures)
        {
            if (vrmlibMaterial is VrmLib.MToonMaterial mtoon)
            {
                // MToon
                var protobufMaterial = UniVRM10.MToonAdapter.MToonToGltf(mtoon, textures);
                return (protobufMaterial, true);
            }
            else if (vrmlibMaterial is VrmLib.UnlitMaterial unlit)
            {
                // Unlit
                var protobufMaterial = UniVRM10.MaterialAdapter.UnlitToGltf(unlit, textures);
                return (protobufMaterial, true);
            }
            else if (vrmlibMaterial is VrmLib.PBRMaterial pbr)
            {
                // PBR
                var protobufMaterial = UniVRM10.MaterialAdapter.PBRToGltf(pbr, textures);
                return (protobufMaterial, false);
            }
            else
            {
                throw new NotImplementedException();
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

        /// Unity material を export => import して元の material と一致するか
        [Test]
        [TestCase("TestMToon", typeof(VrmLib.MToonMaterial))]
        [TestCase("TestUniUnlit", typeof(VrmLib.UnlitMaterial))]
        [TestCase("TestStandard", typeof(VrmLib.PBRMaterial))]
        [TestCase("TestUnlitColor", typeof(VrmLib.UnlitMaterial), false)]
        [TestCase("TestUnlitTexture", typeof(VrmLib.UnlitMaterial), false)]
        [TestCase("TestUnlitTransparent", typeof(VrmLib.UnlitMaterial), false)]
        [TestCase("TestUnlitCutout", typeof(VrmLib.UnlitMaterial), false)]
        public void UnityMaterialTest(string materialName, Type vrmLibMaterialType, bool sameShader = true)
        {
            // asset (cerate copy for avoid modify asset)
            var src = new Material(Resources.Load<Material>(materialName));

            // asset => vrmlib
            var converter = new UniVRM10.RuntimeVrmConverter();
            var vrmLibMaterial = converter.Export10(src, (a, b, c, d) => null);
            Assert.AreEqual(vrmLibMaterialType, vrmLibMaterial.GetType());

            // vrmlib => gltf
            var textures = new List<VrmLib.Texture>();
            var (gltfMaterial, hasKhrUnlit) = ToProtobufMaterial(vrmLibMaterial, textures);
            if (gltfMaterial.extensions != null)
            {
                gltfMaterial.extensions = gltfMaterial.extensions.Deserialize();
            }
            Assert.AreEqual(hasKhrUnlit, glTF_KHR_materials_unlit.IsEnable(gltfMaterial));

            // gltf => json
            var jsonMaterial = Serialize(gltfMaterial, UniGLTF.GltfSerializer.Serialize_gltf_materials_ITEM);

            // gltf <= json
            var deserialized = UniGLTF.GltfDeserializer.Deserialize_gltf_materials_LIST(jsonMaterial.ParseAsJson());

            // // vrmlib <= gltf
            // var loaded = deserialized.FromGltf(textures);
            // // var context = ModelDiffContext.Create();
            // // ModelDiffExtensions.MaterialEquals(context, vrmLibMaterial, loaded);
            // // var diff = context.List
            // // .Where(x => !s_ignoreKeys.Contains(x.Context))
            // // .ToArray();
            // // if (diff.Length > 0)
            // // {
            // //     Debug.LogWarning(string.Join("\n", diff.Select(x => $"{x.Context}: {x.Message}")));
            // // }
            // // Assert.AreEqual(0, diff.Length);

            // // <= vrmlib
            // var map = new Dictionary<VrmLib.Texture, Texture2D>();
            // var dst = UniVRM10.RuntimeUnityMaterialBuilder.CreateMaterialAsset(loaded, hasVertexColor: false, map);
            // dst.name = src.name;

            // if (sameShader)
            // {
            //     CompareUnityMaterial(src, dst);
            // }
        }

        [Test]
        public void ExpressionTest()
        {
            var q = "\"";

            {
                var data = new UniGLTF.Extensions.VRMC_vrm.Expression();
                data.OverrideBlink = UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block;

                var json = Serialize(data, UniGLTF.Extensions.VRMC_vrm.GltfSerializer.Serialize_Expressions_ITEM);
                Assert.AreEqual($"{{{q}preset{q}:{q}custom{q},{q}overrideBlink{q}:{q}block{q},{q}overrideLookAt{q}:{q}none{q},{q}overrideMouth{q}:{q}none{q}}}", json);
            }

            {
                var expression = new VrmLib.Expression(VrmLib.ExpressionPreset.Blink, "blink", true)
                {
                    OverrideBlink = VrmLib.ExpressionOverrideType.None,
                    OverrideLookAt = VrmLib.ExpressionOverrideType.Block,
                    OverrideMouth = VrmLib.ExpressionOverrideType.Blend,
                };

                // export
                var gltf = UniVRM10.ExpressionAdapter.ToGltf(expression, new List<VrmLib.Node>(), new List<VrmLib.Material>());
                Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none, gltf.OverrideBlink);
                Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block, gltf.OverrideLookAt);
                Assert.AreEqual(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend, gltf.OverrideMouth);

                // import
                var imported = UniVRM10.ExpressionAdapter.FromGltf(gltf, new List<VrmLib.Node>(), new List<VrmLib.Material>());
                Assert.AreEqual(VrmLib.ExpressionOverrideType.None, imported.OverrideBlink);
                Assert.AreEqual(VrmLib.ExpressionOverrideType.Block, imported.OverrideLookAt);
                Assert.AreEqual(VrmLib.ExpressionOverrideType.Blend, imported.OverrideMouth);
            }

            {
                // export
                foreach (var preset in Enum.GetValues(typeof(VrmLib.ExpressionPreset)) as VrmLib.ExpressionPreset[])
                {
                    var expression = new VrmLib.Expression(preset, "", false);
                    
                    // expect no exception
                    var gltf = ExpressionAdapter.ToGltf(
                        expression, 
                        new List<VrmLib.Node>(),
                        new List<VrmLib.Material>());
                }
                
                // import 
                foreach (var preset in Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)) as UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])
                {
                    var gltf = new UniGLTF.Extensions.VRMC_vrm.Expression
                    {
                        Preset = preset,
                    };

                    // expect no exception
                    ExpressionAdapter.FromGltf(
                        gltf,
                        new List<VrmLib.Node>(),
                        new List<VrmLib.Material>());
                }
            }
        }
    }
}
