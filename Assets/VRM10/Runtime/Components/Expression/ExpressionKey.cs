using System;
using System.Collections.Generic;

namespace UniVRM10
{
    [Serializable]
    public struct ExpressionKey : IEquatable<ExpressionKey>, IComparable<ExpressionKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<VrmLib.ExpressionPreset, string> m_presetNameDictionary =
            new Dictionary<VrmLib.ExpressionPreset, string>();


        /// <summary>
        ///  ExpressionPreset と同名の名前を持つ独自に追加した Expression を区別するための prefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        private string m_customName;

        public string Name
        {
            get { return m_customName.ToUpper(); }
        }

        public VrmLib.ExpressionPreset Preset;

        public bool IsBlink
        {
            get
            {
                switch (Preset)
                {
                    case VrmLib.ExpressionPreset.Blink:
                    case VrmLib.ExpressionPreset.BlinkLeft:
                    case VrmLib.ExpressionPreset.BlinkRight:
                        return true;
                }
                return false;
            }
        }

        public bool IsLookAt
        {
            get
            {
                switch (Preset)
                {
                    case VrmLib.ExpressionPreset.LookUp:
                    case VrmLib.ExpressionPreset.LookDown:
                    case VrmLib.ExpressionPreset.LookLeft:
                    case VrmLib.ExpressionPreset.LookRight:
                        return true;
                }
                return false;
            }
        }

        public bool IsMouth
        {
            get
            {
                switch (Preset)
                {
                    case VrmLib.ExpressionPreset.Aa:
                    case VrmLib.ExpressionPreset.Ih:
                    case VrmLib.ExpressionPreset.Ou:
                    case VrmLib.ExpressionPreset.Ee:
                    case VrmLib.ExpressionPreset.Oh:
                        return true;
                }
                return false;
            }
        }

        public bool IsProcedual => IsBlink || IsLookAt || IsMouth;

        string m_id;

        string ID
        {
            get
            {
                if (string.IsNullOrEmpty(m_id))
                {
                    // Unknown was deleted
                    if (Preset != VrmLib.ExpressionPreset.Custom)
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
                        m_id = UnknownPresetPrefix + m_customName;
                    }
                }

                return m_id;
            }
        }

        public ExpressionKey(VrmLib.ExpressionPreset preset, string customName = null)
        {
            Preset = preset;
            m_customName = customName;

            if (Preset != VrmLib.ExpressionPreset.Custom)
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
                if (string.IsNullOrEmpty(m_customName))
                {
                    throw new ArgumentException("name is required for VrmLib.ExpressionPreset.Custom");
                }
                m_id = UnknownPresetPrefix + m_customName;
            }
        }

        public static ExpressionKey CreateCustom(String key)
        {
            return new ExpressionKey(VrmLib.ExpressionPreset.Custom, key);
        }

        public static ExpressionKey CreateFromPreset(VrmLib.ExpressionPreset preset)
        {
            return new ExpressionKey(preset);
        }

        public static ExpressionKey CreateFromClip(VRM10Expression clip)
        {
            if (clip == null)
            {
                return default(ExpressionKey);
            }

            return new ExpressionKey(clip.Preset, clip.ExpressionName);
        }

        public override string ToString()
        {
            return ID.Replace(UnknownPresetPrefix, "").ToUpper();
        }

        public bool Equals(ExpressionKey other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExpressionKey)
            {
                return Equals((ExpressionKey)obj);
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

        public bool Match(VRM10Expression clip)
        {
            return this.Equals(CreateFromClip(clip));
        }

        public int CompareTo(ExpressionKey other)
        {
            if (Preset != other.Preset)
            {
                return Preset - other.Preset;
            }

            return 0;
        }
    }

}
