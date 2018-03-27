using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;


namespace VRM
{
    [CreateAssetMenu(menuName = "VRM/BlendShapeAvatar")]
    public class BlendShapeAvatar : ScriptableObject
    {
        [SerializeField]
        public List<BlendShapeClip> Clips = new List<BlendShapeClip>();

        public void CreateDefault()
        {
            foreach (BlendShapePreset preset in Enum.GetValues(typeof(BlendShapePreset)))
            {
                if (preset != BlendShapePreset.Unknown)
                {
                    var clip = GetClip(preset);
                    if (clip == null)
                    {
                        clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                        clip.name = preset.ToString();
                        clip.BlendShapeName = preset.ToString();
                        Clips.Add(clip);
                    }
                }
            }
        }

        public void SetClip(BlendShapeKey key, BlendShapeClip clip)
        {
            int index = -1;
            try
            {
                index = Clips.FindIndex(x => key.Match(x));
            }
            catch (Exception)
            {

            }
            if (index == -1)
            {
                Clips.Add(clip);
            }
            else
            {
                Clips[index] = clip;
            }
        }

        public BlendShapeClip GetClip(BlendShapeKey key)
        {
            if (Clips == null) return null;
            return Clips.FirstOrDefault(x => key.Match(x));
        }

        public BlendShapeClip GetClip(BlendShapePreset preset)
        {
            return GetClip(new BlendShapeKey(preset));
        }

        public BlendShapeClip GetClip(String name)
        {
            return GetClip(new BlendShapeKey(name));
        }

        public bool Apply(string name, Transform transform, float value)
        {
            var clip = GetClip(name);
            if (clip == null) return false;
            clip.Apply(transform, value);
            return true;
        }
    }
}
