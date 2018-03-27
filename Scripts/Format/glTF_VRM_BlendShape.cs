using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// BlendShapeBind
    /// </summary>
    [Serializable]
    public class glTF_VRM_BlendShapeBind : UniGLTF.JsonSerializableBase
    {
        public int mesh = -1;
        public int index = -1;
        public float weight = 0;

        protected override void SerializeMembers(UniGLTF.JsonFormatter f)
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

    public static class BlendShpaePresetExtensions
    {
        /*
        static string ToCamel(this string src)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            return src.Substring(0, 1).ToUpper() + src.Substring(1);
        }
        */

        public static BlendShapePreset ToBlendShapePreset(this string preset)
        {
            try
            {
                return (BlendShapePreset)Enum.Parse(typeof(BlendShapePreset), preset, true);
            }
            catch (Exception)
            {
                return default(BlendShapePreset);
            }
        }
    }

    [Serializable]
    public class glTF_VRM_BlendShapeGroup : UniGLTF.JsonSerializableBase
    {
        public string name;
        public string presetName;
        [Obsolete]
        public string extendedName;
        public List<glTF_VRM_BlendShapeBind> binds = new List<glTF_VRM_BlendShapeBind>();

        protected override void SerializeMembers(JsonFormatter f)
        {
            f.KeyValue(() => name);
            f.KeyValue(() => presetName);
            /*
            if (name == "extended")
            {
                f.KeyValue(() => extendedName);
            }
            f.Key("flags"); f.Value((int)flags);
            */
            f.KeyValue(() => binds);
        }
    }

    [Serializable]
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
            var group = new glTF_VRM_BlendShapeGroup
            {
                name = clip.BlendShapeName,
                presetName = clip.Preset.ToString().ToLower(),
                binds = list,
            };
            blendShapeGroups.Add(group);
        }

        protected override void SerializeMembers(UniGLTF.JsonFormatter f)
        {
            f.KeyValue(() => blendShapeGroups);
        }
    }
}
