using System;
using System.Collections.Generic;

namespace VRM
{
    [Serializable]
    public readonly struct BlendShapeKey : IEquatable<BlendShapeKey>, IComparable<BlendShapeKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<BlendShapePreset, string> PresetNameCacheDictionary =
            new Dictionary<BlendShapePreset, string>();

        /// <summary>
        ///  BlendShapePresetと同名の名前を持つ独自に追加したBlendShapeを区別するためのprefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        public string Name { get; }

        public readonly BlendShapePreset Preset;

        private readonly string m_id;

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
            Preset = preset;

            if (Preset != BlendShapePreset.Unknown)
            {
                if (PresetNameCacheDictionary.TryGetValue(Preset, out var presetName))
                {
                    m_id = Name = presetName;
                }
                else
                {
                    // BlendShapePreset.Unknown 以外の場合、 name は捨てられる
                    m_id = Name = Preset.ToString();

                    PresetNameCacheDictionary.Add(Preset, Name);
                }
            }
            else
            {
                Name = !string.IsNullOrEmpty(name) ? name : "";
                m_id = UnknownPresetPrefix + Name;
            }
        }

        /// <summary>
        /// PresetからBlendShapeKeyを生成
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        public static BlendShapeKey CreateFromPreset(BlendShapePreset preset)
        {
            return new BlendShapeKey("", preset);
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
            return m_id.Replace(UnknownPresetPrefix, "");
        }

        public bool Equals(BlendShapeKey other)
        {
            return m_id == other.m_id;
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
            return m_id.GetHashCode();
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
