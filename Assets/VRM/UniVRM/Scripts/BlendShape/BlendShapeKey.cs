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

        private readonly string m_name;

        public string Name
        {
            get { return m_name; }
        }

        public readonly BlendShapePreset Preset;

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

        /// <summary>
        /// name と preset のペアからBlendShapeKeyを生成するが、
        /// BlendShapePreset.Unknown のときと、それ以外のときで挙動が異なることを知っている必要があって、わかりにくいので private に変更。
        /// v0.56
        /// 
        /// 代わりに、public static 関数を使って生成します
        /// 
        /// CreateFromPreset(BlendShapePreset)
        /// CreateFromClip(BlendShapeClip)
        /// CreateUnknown(string)
        /// 
        /// TODO ?
        /// 旧仕様(GC発生などでパフォーマンスは、あまりよろしくない)
        /// CreateLegacyFromString(string);
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        private BlendShapeKey(string name, BlendShapePreset preset)
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

        /// <summary>
        /// PresetからBlendShapeKeyを生成
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        public static BlendShapeKey CreateFromPreset(BlendShapePreset preset)
        {
            return new BlendShapeKey(preset.ToString(), preset);
        }

        /// <summary>
        /// BlendShapeClipからBlendShapeKeyを生成
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static BlendShapeKey CreateFromClip(BlendShapeClip clip)
        {
            if (clip == null)
            {
                return default(BlendShapeKey);
            }

            return new BlendShapeKey(clip.BlendShapeName, clip.Preset);
        }

        /// <summary>
        /// BlendShapePreset.Unknown である BlendShapeKey を name から作成する
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BlendShapeKey CreateUnknown(string name)
        {
            return new BlendShapeKey(name, BlendShapePreset.Unknown);
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

        public bool Match(BlendShapeClip clip)
        {
            return this.Equals(CreateFromClip(clip));
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
