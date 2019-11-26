using System;
using System.Collections.Generic;

namespace VRM
{
    [Serializable]
    public struct BlendShapeKey : IEquatable<BlendShapeKey>, IComparable<BlendShapeKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<BlendShapePreset, string> m_presetNameDictionary =
            new Dictionary<BlendShapePreset, string>();


        /// <summary>
        ///  BlendShapePresetと同名の名前を持つ独自に追加したBlendShapeを区別するためのprefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        private string m_name;

        public string Name
        {
            get { return m_name.ToUpper(); }
        }

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
                        if (m_presetNameDictionary.ContainsKey(Preset))
                        {
                            m_id = m_presetNameDictionary[Preset];
                        }
                        else
                        {
                            m_presetNameDictionary.Add(Preset, Preset.ToString());
                            m_id = m_presetNameDictionary[Preset];
                        }
                    }
                    else
                    {
                        m_id = UnknownPresetPrefix + m_name;
                    }
                }

                return m_id;
            }
        }

        public BlendShapeKey(BlendShapePreset preset) : this(preset.ToString(), preset)
        {
        }

        public BlendShapeKey(string name, BlendShapePreset preset = BlendShapePreset.Unknown)
        {
            m_name = name;
            Preset = preset;

            if (Preset != BlendShapePreset.Unknown)
            {
                if (m_presetNameDictionary.ContainsKey((Preset)))
                {
                    m_id = m_presetNameDictionary[Preset];
                }
                else
                {
                    m_presetNameDictionary.Add(Preset, Preset.ToString());
                    m_id = m_presetNameDictionary[Preset];
                }
            }
            else
            {
                m_id = UnknownPresetPrefix + m_name;
            }
        }

        public override string ToString()
        {
            return ID.Replace(UnknownPresetPrefix, "").ToUpper();
        }

        public bool Equals(BlendShapeKey other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj is BlendShapeKey)
            {
                return Equals((BlendShapeKey) obj);
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
