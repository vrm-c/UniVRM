using System;
using System.Collections.Generic;

namespace VRM
{
    [Serializable]
    public struct BlendShapeKey : IEquatable<BlendShapeKey>
    {
        public string Name;
        public BlendShapePreset Preset;

        string m_id;
        string ID
        {
            get
            {
                if (string.IsNullOrEmpty(m_id))
                {
                    if (Preset != BlendShapePreset.Unknown)
                    {
                        m_id = Preset.ToString().ToUpper();
                    }
                    else
                    {
                        m_id = Name;
                    }
                }
                return m_id;
            }
        }

        public BlendShapeKey(string name) : this(name, BlendShapePreset.Unknown)
        {
        }

        public BlendShapeKey(BlendShapePreset preset) : this(preset.ToString(), BlendShapePreset.Unknown)
        {
        }

        public BlendShapeKey(string name, BlendShapePreset preset)
        {
            Name = name.ToUpper();
            Preset = preset;
            if (Preset != BlendShapePreset.Unknown)
            {
                m_id = Preset.ToString().ToUpper();
            }
            else
            {
                m_id = Name;
            }
        }

        public override string ToString()
        {
            return ID;
        }

        public bool Equals(BlendShapeKey other)
        {
            /*
            if (Preset != BlendShapePreset.Unknown && Preset == other.Preset)
            {
                // Unknown以外のPresetが一致したら一致とみなす
                return true;
            }
            // .ToUpper()済み
            return Name == other.Name;
            */
            return ID == other.ID;
        }

        public class CustomerEqualityComparer : IEqualityComparer<BlendShapeKey>
        {
            public bool Equals(BlendShapeKey x, BlendShapeKey y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(BlendShapeKey obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        public static BlendShapeKey CreateFrom(BlendShapeClip clip)
        {
            return new BlendShapeKey(clip.BlendShapeName, clip.Preset);
        }

        public bool Match(BlendShapeClip clip)
        {
            return this.Equals(CreateFrom(clip));
        }
    }
}
