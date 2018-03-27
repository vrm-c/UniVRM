using System;
using System.Linq;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public struct BlendShapeBinding
    {
        public String RelativePath;
        public int Index;
        public float Weight;
    }

    /*
    public struct BlendShapeKey
    {
        public BlendShapePreset Preset;
        public String Name;

        public BlendShapeKey(BlendShapePreset preset, String name)
        {
            Preset = preset;
            Name = name.ToUpper();
        }
        public BlendShapeKey(BlendShapePreset preset)
        {
            Preset = preset;
            Name = preset.ToString().ToUpper();
        }
        public BlendShapeKey(String name)
        {
            Preset = (BlendShapePreset)Enum.Parse(typeof(BlendShapePreset), name, true);
            Name = name.ToUpper();
        }
    }
    */

    [CreateAssetMenu(menuName = "VRM/BlendShapeClip")]
    public class BlendShapeClip : ScriptableObject
    {
        [SerializeField]
        public string BlendShapeName;

        [SerializeField]
        public BlendShapePreset Preset;

        [SerializeField]
        public BlendShapeBinding[] Values = new BlendShapeBinding[] { };

        public void Apply(Transform root, float value)
        {
            if (Values != null)
            {
                foreach (var x in Values)
                {
                    var target = root.Find(x.RelativePath);
                    if (target != null)
                    {
                        var sr = target.GetComponent<SkinnedMeshRenderer>();
                        sr.SetBlendShapeWeight(x.Index, x.Weight * value);
                    }
                }
            }
        }

        public static BlendShapeClip Create(BlendShapePreset preset)
        {
            var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            clip.Preset = preset;
            clip.BlendShapeName = preset.ToString();
            clip.name = preset.ToString();
            return clip;
        }
    }
}
