using System;


namespace VRM
{
    [Serializable]
    public struct BlendShapeKey : IEquatable<BlendShapeKey>, IComparable<BlendShapeKey>
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
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj is BlendShapeKey)
            {
                return Equals((BlendShapeKey)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static BlendShapeKey CreateFrom(BlendShapeClip clip)
        {
            if (clip == null)
            {
                return default(BlendShapeKey);
            }
            return new BlendShapeKey(clip.BlendShapeName, clip.Preset);
        }

        public bool Match(BlendShapeClip clip)
        {
            return this.Equals(CreateFrom(clip));
        }

        public int CompareTo(BlendShapeKey other)
        {
            if (Preset != other.Preset)
            {
                return Preset - other.Preset;
            }

            return 0;
        }
    }
}
