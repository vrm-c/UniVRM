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
            Assert.AreEqual(@"{""firstPersonBone"":-1,""firstPersonBoneOffset"":{""x"":0,""y"":0,""z"":0},""meshAnnotations"":[],""lookAtTypeName"":""Bone"",""lookAtHorizontalInner"":{""xRange"":90,""yRange"":10},""lookAtHorizontalOuter"":{""xRange"":90,""yRange"":10},""lookAtVerticalDown"":{""xRange"":90,""yRange"":10},""lookAtVerticalUp"":{""xRange"":90,""yRange"":10}}", json);
            Debug.Log(json);

            var c = new JsonSchemaValidationContext("")
            {
                EnableDiagnosisForNotRequiredFields = true,
            };
            var json2 = JsonSchema.FromType<glTF_VRM_Firstperson>().Serialize(model, c);
            Assert.AreEqual(@"{""firstPersonBone"":-1,""firstPersonBoneOffset"":{""x"":0,""y"":0,""z"":0},""meshAnnotations"":[],""lookAtTypeName"":""Bone"",""lookAtHorizontalInner"":{""xRange"":90,""yRange"":10},""lookAtHorizontalOuter"":{""xRange"":90,""yRange"":10},""lookAtVerticalDown"":{""xRange"":90,""yRange"":10},""lookAtVerticalUp"":{""xRange"":90,""yRange"":10}}", json2);
      }
    }
}
