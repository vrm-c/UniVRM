using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UniJSON;

namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm.blendshape.materialbind")]
    public class glTF_VRM_MaterialValueBind : UniGLTF.JsonSerializableBase
    {
        public string materialName;
        public string propertyName;
        public float[] targetValue;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => materialName);
            f.KeyValue(() => propertyName);
            f.KeyValue(() => targetValue);
        }
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshape.bind")]
    public class glTF_VRM_BlendShapeBind : UniGLTF.JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int mesh = -1;

        [JsonSchema(Required = true, Minimum = 0)]
        public int index = -1;

        [JsonSchema(Required =true, Minimum = 0, Maximum = 100, Description = @"SkinnedMeshRenderer.SetBlendShapeWeight")]
        public float weight = 0;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => mesh);
            f.KeyValue(() => index);
            f.KeyValue(() => weight);
        }

        public static glTF_VRM_BlendShapeBind Cerate(Transform root, List<Mesh> meshes, BlendShapeBinding binding)
        {
            var transform = UniGLTF.UnityExtensions.GetFromPath(root.transform, binding.RelativePath);
            var renderer = transform.GetComponent<SkinnedMeshRenderer>();
            var mesh = renderer.sharedMesh;
            var meshIndex = meshes.IndexOf(mesh);

            return new glTF_VRM_BlendShapeBind
            {
                mesh = meshIndex,
                index = binding.Index,
                weight = binding.Weight,
            };
        }
    }

    public enum BlendShapePreset
    {
        Unknown,

        Neutral,

        A,
        I,
        U,
        E,
        O,

        Blink,

        // 喜怒哀楽
        Joy,
        Angry,
        Sorrow,
        Fun,

        // LookAt
        LookUp,
        LookDown,
        LookLeft,
        LookRight,

        Blink_L,
        Blink_R,
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshape.group", Description = "BlendShapeClip of UniVRM")]
    public class glTF_VRM_BlendShapeGroup : UniGLTF.JsonSerializableBase
    {
        [JsonSchema(Description = "Expression name")]
        public string name;

        [JsonSchema(Description = "Predefined Expression name", EnumValues = new object[] {
            "unknown",
            "neutral",
            "a",
            "i",
            "u",
            "e",
            "o",
            "blink",
            "joy",
            "angry",
            "sorrow",
            "fun",
            "lookup",
            "lookdown",
            "lookleft",
            "lookright",
            "blink_l",
            "blink_r",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string presetName;

        [JsonSchema(Description = "Low level blendshape references. ")]
        public List<glTF_VRM_BlendShapeBind> binds = new List<glTF_VRM_BlendShapeBind>();

        [JsonSchema(Description = "Material animation references.")]
        public List<glTF_VRM_MaterialValueBind> materialValues = new List<glTF_VRM_MaterialValueBind>();

        [JsonSchema(Description = "0 or 1. Do not allow an intermediate value. Value should rounded")]
        public bool isBinary;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => name);
            f.KeyValue(() => presetName);
            f.KeyValue(() => isBinary);
            f.Key("binds"); f.GLTFValue(binds);
            f.Key("materialValues"); f.GLTFValue(materialValues);
        }
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshape", Description = "BlendShapeAvatar of UniVRM")]
    public class glTF_VRM_BlendShapeMaster : UniGLTF.JsonSerializableBase
    {
        public List<glTF_VRM_BlendShapeGroup> blendShapeGroups = new List<glTF_VRM_BlendShapeGroup>();

        public void Add(BlendShapeClip clip, Transform transform, List<Mesh> meshes)
        {
            var list = new List<glTF_VRM_BlendShapeBind>();
            if (clip.Values != null)
            {
                list.AddRange(clip.Values.Select(y => glTF_VRM_BlendShapeBind.Cerate(transform, meshes.ToList(), y)));
            }

            var materialList = new List<glTF_VRM_MaterialValueBind>();
            if (clip.MaterialValues != null)
            {
                materialList.AddRange(clip.MaterialValues.Select(y => new glTF_VRM_MaterialValueBind
                {
                    materialName = y.MaterialName,
                    propertyName = y.ValueName,
                    targetValue = y.TargetValue.ToArray(),
                }));
            }

            var group = new glTF_VRM_BlendShapeGroup
            {
                name = clip.BlendShapeName,
                presetName = clip.Preset.ToString().ToLower(),
                isBinary = clip.IsBinary,
                binds = list,
                materialValues = materialList,
            };
            blendShapeGroups.Add(group);
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.Key("blendShapeGroups"); f.GLTFValue(blendShapeGroups);
        }
    }
}
