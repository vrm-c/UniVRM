using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UniJSON;

namespace VRM
{
    [Serializable]
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
    public class glTF_VRM_BlendShapeBind : UniGLTF.JsonSerializableBase
    {
        public int mesh = -1;
        public int index = -1;
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
    public class glTF_VRM_BlendShapeGroup : UniGLTF.JsonSerializableBase
    {
        public string name;
        public string presetName;
        public List<glTF_VRM_BlendShapeBind> binds = new List<glTF_VRM_BlendShapeBind>();
        public List<glTF_VRM_MaterialValueBind> materialValues = new List<glTF_VRM_MaterialValueBind>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => name);
            f.KeyValue(() => presetName);
            f.KeyValue(() => binds);
            f.KeyValue(() => materialValues);
        }
    }

    [Serializable]
    [JsonSchema(Title = "vrm.blendshapemaster")]
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
                binds = list,
                materialValues = materialList,
            };
            blendShapeGroups.Add(group);
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => blendShapeGroups);
        }
    }
}
