using System;
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

    [Serializable]
    public struct MaterialValueBinding
    {
        public String RelativePath;
        public int Index;
        public string ValueName;
        public Vector4 TargetValue;

        public Vector4 BaseValue; // <- Get From RelativePath/Materials[Index][ValueName]
    }

    [CreateAssetMenu(menuName = "VRM/BlendShapeClip")]
    public class BlendShapeClip : ScriptableObject
    {
        [SerializeField]
        public string BlendShapeName = "";

        [SerializeField]
        public BlendShapePreset Preset;

        [SerializeField]
        public BlendShapeBinding[] Values = new BlendShapeBinding[] { };

        [SerializeField]
        public MaterialValueBinding[] MaterialValues = new MaterialValueBinding[] { };

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
                        if (sr != null)
                        {
                            sr.SetBlendShapeWeight(x.Index, x.Weight * value);
                        }
                    }
                }
            }

            if (MaterialValues != null)
            {
                foreach (var x in MaterialValues)
                {
                    var target = root.Find(x.RelativePath);
                    if (target != null)
                    {
                        var sr = target.GetComponent<SkinnedMeshRenderer>();
                        if (sr != null)
                        {
                            var m = sr.sharedMaterials[x.Index];
                            var color = x.BaseValue + (x.TargetValue - x.BaseValue) * value;
                            m.SetColor(x.ValueName, color);
                        }
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
