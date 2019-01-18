using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;
using UnityEngine;

namespace VRM
{
    public class UniVRMSerializeTests
    {
        [Test]
        public void MaterialValueBindTest()
        {
            var model = new glTF_VRM_MaterialValueBind();

            var json = model.ToJson();
            Assert.AreEqual(@"{}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_MaterialValueBind>().Serialize(model, c);
            Assert.AreEqual(json, json2);
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

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_BlendShapeBind>().Serialize(model, c);
            Assert.AreEqual(json, json2);
        }

        [Test]
        public void BlendShapeBindTestError()
        {
            var model = new glTF_VRM_BlendShapeBind();

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_BlendShapeBind>().Serialize(model, c)
            );
            Assert.AreEqual("[mesh.String] minimum: ! -1>=0", ex.Message);
        }

        [Test]
        public void BlendShapeGroupTest()
        {
            var model = new glTF_VRM_BlendShapeGroup()
            {
                presetName = "neutral",
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""presetName"":""neutral"",""isBinary"":false,""binds"":[],""materialValues"":[]}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_BlendShapeGroup>().Serialize(model, c);
            Assert.AreEqual(@"{""presetName"":""neutral"",""binds"":[],""materialValues"":[],""isBinary"":false}", json2);
        }

        [Test]
        public void BlendShapeGroupTestError()
        {
            var model = new glTF_VRM_BlendShapeGroup()
            {
                presetName = "aaaaaaaaaaaa_not_exists_",
            };

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_BlendShapeGroup>().Serialize(model, c)
            );
            Assert.AreEqual("[presetName.String] aaaaaaaaaaaa_not_exists_ is not valid enum", ex.Message);
        }

        [Test]
        public void DegreeMapTest()
        {
            var model = new glTF_VRM_DegreeMap();

            var json = model.ToJson();
            Assert.AreEqual(@"{""xRange"":90,""yRange"":10}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_DegreeMap>().Serialize(model, c);
            Assert.AreEqual(json, json2);
        }

        [Test]
        public void MeshAnnotationTest()
        {
            var model = new glTF_VRM_MeshAnnotation();

            var json = model.ToJson();
            Assert.AreEqual(@"{""mesh"":0}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_MeshAnnotation>().Serialize(model, c);
            Assert.AreEqual(json, json2);
        }

        [Test]
        public void FirstPersonTest()
        {
            var model = new glTF_VRM_Firstperson();

            var json = model.ToJson();
            Assert.AreEqual(
                @"{""firstPersonBone"":-1,""firstPersonBoneOffset"":{""x"":0,""y"":0,""z"":0},""meshAnnotations"":[],""lookAtTypeName"":""Bone"",""lookAtHorizontalInner"":{""xRange"":90,""yRange"":10},""lookAtHorizontalOuter"":{""xRange"":90,""yRange"":10},""lookAtVerticalDown"":{""xRange"":90,""yRange"":10},""lookAtVerticalUp"":{""xRange"":90,""yRange"":10}}",
                json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_Firstperson>().Serialize(model, c);
            Assert.AreEqual(
                @"{""firstPersonBoneOffset"":{""x"":0,""y"":0,""z"":0},""meshAnnotations"":[],""lookAtTypeName"":""Bone"",""lookAtHorizontalInner"":{""xRange"":90,""yRange"":10},""lookAtHorizontalOuter"":{""xRange"":90,""yRange"":10},""lookAtVerticalDown"":{""xRange"":90,""yRange"":10},""lookAtVerticalUp"":{""xRange"":90,""yRange"":10}}",
                json2);
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

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_HumanoidBone>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(
                @"{""bone"":""hips"",""node"":0,""useDefaultValues"":true,""min"":{""x"":0,""y"":0,""z"":0},""max"":{""x"":0,""y"":0,""z"":0},""center"":{""x"":0,""y"":0,""z"":0},""axisLength"":0}",
                json2);
        }

        [Test]
        public void HumanoidBoneTestError()
        {
            var model = new glTF_VRM_HumanoidBone()
            {
                bone = "hips", // NOTE: This field must not be null?
            };

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_HumanoidBone>().Serialize(model, c)
            );
            Assert.AreEqual("[node.String] minimum: ! -1>=0", ex.Message);
        }

        [Test]
        public void HumanoidTest()
        {
            var model = new glTF_VRM_Humanoid();

            var json = model.ToJson();
            Assert.AreEqual(@"{""humanBones"":[],""armStretch"":0.05,""legStretch"":0.05,""upperArmTwist"":0.5,""lowerArmTwist"":0.5,""upperLegTwist"":0.5,""lowerLegTwist"":0.5,""feetSpacing"":0,""hasTranslationDoF"":false}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_Humanoid>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        [Test]
        public void MaterialTest()
        {
            var model = new glTF_VRM_Material();

            var json = model.ToJson();
            Assert.AreEqual(@"{""renderQueue"":-1,""floatProperties"":{},""vectorProperties"":{},""textureProperties"":{},""keywordMap"":{},""tagMap"":{}}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_Material>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        [Test]
        public void MetaTest()
        {
            var model = new glTF_VRM_Meta();

            var json = model.ToJson();
            Assert.AreEqual(@"{""texture"":-1}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_Meta>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(@"{}",json2);
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

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_SecondaryAnimationCollider>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        [Test]
        public void SecondaryAnimationColliderGroupTest()
        {
            var model = new glTF_VRM_SecondaryAnimationColliderGroup();

            var json = model.ToJson();
            Assert.AreEqual(@"{""node"":0,""colliders"":[]}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_SecondaryAnimationColliderGroup>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        [Test]
        public void SecondaryAnimationColliderGroupTestError()
        {
            var model = new glTF_VRM_SecondaryAnimationColliderGroup()
            {
                node = -1,
            };

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_SecondaryAnimationColliderGroup>().Serialize(model, c)
            );
            Assert.AreEqual("[node.String] minimum: ! -1>=0", ex.Message);
        }

        [Test]
        public void SecondaryAnimationGroupTest()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup();

            var json = model.ToJson();
            Assert.AreEqual(@"{""stiffiness"":0,""gravityPower"":0,""gravityDir"":{""x"":0,""y"":0,""z"":0},""dragForce"":0,""center"":0,""hitRadius"":0,""bones"":[],""colliderGroups"":[]}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_SecondaryAnimationGroup>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        [Test]
        public void SecondaryAnimationGroupTestErrorBones()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup()
            {
                bones = new int[] { -1 }
            };

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_SecondaryAnimationGroup>().Serialize(model, c)
            );
            Assert.AreEqual("[bones.String] minimum: ! -1>=0", ex.Message);
        }

        [Test]
        public void SecondaryAnimationGroupTestErrorColliderGroups()
        {
            var model = new glTF_VRM_SecondaryAnimationGroup()
            {
                colliderGroups = new int[] { -1 }
            };

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var ex = Assert.Throws<JsonSchemaValidationException>(
                () => JsonSchema.FromType<glTF_VRM_SecondaryAnimationGroup>().Serialize(model, c)
            );
            Assert.AreEqual("[colliderGroups.String] minimum: ! -1>=0", ex.Message);
        }

        [Test]
        public void SecondaryAnimationTest()
        {
            var model = new glTF_VRM_SecondaryAnimation();

            var json = model.ToJson();
            Assert.AreEqual(@"{""boneGroups"":[],""colliderGroups"":[]}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_SecondaryAnimation>().Serialize(model, c);
            // NOTE: New serializer outputs values which will not be used...
            Assert.AreEqual(json,json2);
        }

        // TODO: Move to another suitable location
        T deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
