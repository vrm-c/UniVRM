using NUnit.Framework;
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace VRM
{
    static class ToJsonExtensions
    {
        public static string ToJson(this glTF self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMesh self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFPrimitives self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes__primitives_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFAttributes self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes__primitives__attributes(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMaterialBaseColorTextureInfo self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMaterial self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_materials_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFNode self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_nodes_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFSkin self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_skins_ITEM(f, self);
            return f.ToString();
        }

        public static string ToJson(this glTF_VRM_MaterialValueBind self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_BlendShapeBind self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_blendShapeMaster_blendShapeGroups__binds_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_BlendShapeGroup self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_blendShapeMaster_blendShapeGroups_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_DegreeMap self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_firstPerson_lookAtHorizontalInner(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_MeshAnnotation self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_firstPerson_meshAnnotations_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_Firstperson self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_firstPerson(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_HumanoidBone self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_humanoid_humanBones_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_Humanoid self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_humanoid(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_Material self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_materialProperties_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_Meta self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_meta(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_SecondaryAnimationCollider self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_secondaryAnimation_colliderGroups__colliders_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTF_VRM_SecondaryAnimationColliderGroup self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_secondaryAnimation_colliderGroups_ITEM(f, self);
            return f.ToString();
        }

        public static string ToJson(this glTF_VRM_SecondaryAnimationGroup self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_secondaryAnimation_boneGroups_ITEM(f, self);
            return f.ToString();
        }

        public static string ToJson(this glTF_VRM_SecondaryAnimation self)
        {
            var f = new JsonFormatter();
            VRMSerializer.Serialize_vrm_secondaryAnimation(f, self);
            return f.ToString();
        }
    }

    public class UniVRMSerializeTests
    {
        [Test]
        public void MaterialValueBindTest()
        {
            var model = new glTF_VRM_MaterialValueBind();

            var json = model.ToJson();
            Assert.AreEqual(@"{}", json);
            Debug.Log(json);
        }

        [Test]
        public void BlendShapeBindTest()
        {
            var model = new glTF_VRM_BlendShapeBind()
            {
                mesh = 1,
                weight = 2,
                index = 3,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""mesh"":1,""index"":3,""weight"":2}", json);
            Debug.Log(json);
        }

        [Test]
        public void BlendShapeBindTestError()
        {
            var model = new glTF_VRM_BlendShapeBind();
        }

        [Test]
        public void BlendShapeGroupTest()
        {
            var model = new glTF_VRM_BlendShapeGroup()
            {
                presetName = "neutral",
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""presetName"":""neutral"",""binds"":[],""materialValues"":[],""isBinary"":false}", json);
            Debug.Log(json);
        }

        [Test]
        public void BlendShapeGroupTestError()
        {
            var model = new glTF_VRM_BlendShapeGroup()
            {
                presetName = "aaaaaaaaaaaa_not_exists_",
            };
        }
        
        [Test]
        public void BlendShapePresetInvariantCultureTest()
        {
            // https://github.com/vrm-c/UniVRM/issues/694
            // Must pass even if this computer's locale was tr-TR.
            var clip2 = ScriptableObject.CreateInstance<BlendShapeClip>();
            clip2.Preset = BlendShapePreset.I;
            var model2 = clip2.Serialize(null);
            var json2 = model2.ToJson();
            Assert.AreEqual(@"{""presetName"":""i"",""binds"":[],""materialValues"":[],""isBinary"":false}", json2);
            Debug.Log(json2);
        }

        [Test]
        public void DegreeMapTest()
        {
            var model = new glTF_VRM_DegreeMap();

            var json = model.ToJson();
            Assert.AreEqual(@"{""xRange"":90,""yRange"":10}", json);
            Debug.Log(json);
        }

        [Test]
        public void MeshAnnotationTest()
        {
            var model = new glTF_VRM_MeshAnnotation();

            var json = model.ToJson();
            Assert.AreEqual(@"{""mesh"":0}", json);
            Debug.Log(json);
        }

        [Test]
        public void FirstPersonTest()
        {
            var model = new glTF_VRM_Firstperson();

            var json = model.ToJson();
            Assert.AreEqual(
                @"{""firstPersonBoneOffset"":{""x"":0,""y"":0,""z"":0},""meshAnnotations"":[],""lookAtTypeName"":""Bone"",""lookAtHorizontalInner"":{""xRange"":90,""yRange"":10},""lookAtHorizontalOuter"":{""xRange"":90,""yRange"":10},""lookAtVerticalDown"":{""xRange"":90,""yRange"":10},""lookAtVerticalUp"":{""xRange"":90,""yRange"":10}}",
                json);
            Debug.Log(json);
        }

        [Test]
        public void HumanoidBoneTest()
        {
            var model = new glTF_VRM_HumanoidBone()
            {
                bone = "hips", // NOTE: This field must not be null?
                node = 0,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""bone"":""hips"",""node"":0,""useDefaultValues"":true}", json);
            Debug.Log(json);
        }

        [Test]
        public void HumanoidBoneTestError()
        {
            var model = new glTF_VRM_HumanoidBone()
            {
                bone = "hips", // NOTE: This field must not be null?
            };
        }

        [Test]
        public void HumanoidTest()
        {
            var model = new glTF_VRM_Humanoid();

            var json = model.ToJson();
            Assert.AreEqual(@"{""humanBones"":[],""armStretch"":0.05,""legStretch"":0.05,""upperArmTwist"":0.5,""lowerArmTwist"":0.5,""upperLegTwist"":0.5,""lowerLegTwist"":0.5,""feetSpacing"":0,""hasTranslationDoF"":false}", json);
            Debug.Log(json);
        }

        [Test]
        public void MaterialTest()
        {
            var model = new glTF_VRM_Material
            {
                floatProperties = new Dictionary<string, float>
                {
                    {"float", 1.0f}
                },
                vectorProperties = new Dictionary<string, float[]>
                {
                    {"vector", new float[]{0, 1, 2, 3 }}
                },
                textureProperties = new Dictionary<string, int>
                {
                    {"texture", 0}
                },
                keywordMap = new Dictionary<string, bool>
                {
                    {"keyword", true}
                },
                tagMap = new Dictionary<string, string>
                {
                    {"tag", "map"}
                },
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""renderQueue"":-1,""floatProperties"":{""float"":1},""vectorProperties"":{""vector"":[0,1,2,3]},""textureProperties"":{""texture"":0},""keywordMap"":{""keyword"":true},""tagMap"":{""tag"":""map""}}", json);
            Debug.Log(json);
        }

        [Test]
        public void MetaTest()
        {
            var model = new glTF_VRM_Meta()
            {
                allowedUserName = "OnlyAuthor",
                violentUssageName = "Disallow",
                sexualUssageName = "Disallow",
                commercialUssageName = "Disallow",
                licenseName = "CC0",
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""allowedUserName"":""OnlyAuthor"",""violentUssageName"":""Disallow"",""sexualUssageName"":""Disallow"",""commercialUssageName"":""Disallow"",""licenseName"":""CC0""}", json);
            Debug.Log(json);
        }

        [Test]
        public void MetaTestError()
        {
            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = null,
                    violentUssageName = null,
                    sexualUssageName = null,
                    commercialUssageName = null,
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    //licenseName = "CC0",
                    licenseName = null,
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    licenseName = "_INVALID_SOME_THING_",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    // allowedUserName = "OnlyAuthor",
                    allowedUserName = null,
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "_INVALID_SOME_THING_",
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    //violentUssageName = "Disallow",
                    violentUssageName = null,
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "_INVALID_SOME_THING_",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    //sexualUssageName = "Disallow",
                    sexualUssageName = null,
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    sexualUssageName = "_INVALID_SOME_THING_",
                    commercialUssageName = "Disallow",
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    //commercialUssageName = "Disallow",
                    commercialUssageName = null,
                    licenseName = "CC0",
                };
            }

            {
                var model = new glTF_VRM_Meta()
                {
                    allowedUserName = "OnlyAuthor",
                    violentUssageName = "Disallow",
                    sexualUssageName = "Disallow",
                    commercialUssageName = "_INVALID_SOME_THING_",
                    licenseName = "CC0",
                };
            }
        }

        // TODO: Move to another suitable location
        [Test]
        public void MetaDeserializeTest()
        {
            var json = @"{}";

            var model = deserialize<glTF_VRM_Meta>(json);

            Assert.AreEqual(-1, model.texture);
        }

        [Test]
        public void SecondaryAnimationColliderTest()
        {
            var model = new glTF_VRM_SecondaryAnimationCollider()
            {
                offset = new Vector3(1, 2, 3),
                radius = 42,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""offset"":{""x"":1,""y"":2,""z"":3},""radius"":42}", json);
            Debug.Log(json);
        }

        [Test]
        public void SecondaryAnimationColliderGroupTest()
        {
            var model = new glTF_VRM_SecondaryAnimationColliderGroup();

            var json = model.ToJson();
            Assert.AreEqual(@"{""node"":0,""colliders"":[]}", json);
            Debug.Log(json);
        }

        [Test]
        public void SecondaryAnimationColliderGroupTestError()
        {
            var model = new glTF_VRM_SecondaryAnimationColliderGroup()
            {
                node = -1,
            };
        }

        [Test]
        public void SecondaryAnimationGroupTest()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup();

            var json = model.ToJson();
            Assert.AreEqual(@"{""stiffiness"":0,""gravityPower"":0,""gravityDir"":{""x"":0,""y"":0,""z"":0},""dragForce"":0,""center"":0,""hitRadius"":0,""bones"":[],""colliderGroups"":[]}", json);
            Debug.Log(json);
        }

        [Test]
        public void SecondaryAnimationGroupTestErrorBones()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup()
            {
                bones = new int[] { -1 }
            };
        }

        [Test]
        public void SecondaryAnimationGroupTestErrorColliderGroups()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup()
            {
                colliderGroups = new int[] { -1 }
            };
        }

        [Test]
        public void SecondaryAnimationTest()
        {
            var model = new glTF_VRM_SecondaryAnimation();

            var json = model.ToJson();
            Assert.AreEqual(@"{""boneGroups"":[],""colliderGroups"":[]}", json);
            Debug.Log(json);
        }

        [Test]
        public void ExtensionsTest()
        {
            var model = new glTF_VRM_extensions()
            {
                meta = null,
                humanoid = null,
                firstPerson = null,
                blendShapeMaster = null,
                secondaryAnimation = null,
                materialProperties = null,
            };
        }

        // TODO: Move to another suitable location
        T deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
