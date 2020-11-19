using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;


namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm.blendshape.materialbind")]
    public class glTF_VRM_MaterialValueBind
    {
        public string materialName;
        public string propertyName;
        public float[] targetValue;
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshape.bind")]
    public class glTF_VRM_BlendShapeBind
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int mesh = -1;

        [JsonSchema(Required = true, Minimum = 0)]
        public int index = -1;

        [JsonSchema(Required = true, Minimum = 0, Maximum = 100, Description = @"SkinnedMeshRenderer.SetBlendShapeWeight")]
        public float weight = 0;
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
    public class glTF_VRM_BlendShapeGroup
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
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshape", Description = "BlendShapeAvatar of UniVRM")]
    public class glTF_VRM_BlendShapeMaster
    {
        public List<glTF_VRM_BlendShapeGroup> blendShapeGroups = new List<glTF_VRM_BlendShapeGroup>();
    }
}
