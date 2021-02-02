using System;
using System.Collections.Generic;

namespace UniVRM10
{
    [Serializable]
    public readonly struct ExpressionKey : IEquatable<ExpressionKey>, IComparable<ExpressionKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<VrmLib.ExpressionPreset, string> PresetNameDictionary =
            new Dictionary<VrmLib.ExpressionPreset, string>();

        /// <summary>
        ///  ExpressionPreset と同名の名前を持つ独自に追加した Expression を区別するための prefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        /// <summary>
        /// Preset of this ExpressionKey.
        /// </summary>
        public readonly VrmLib.ExpressionPreset Preset;
        
        /// <summary>
        /// Custom Name of this ExpressionKey.
        /// This works if Preset was Custom.
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// Id for comparison of ExpressionKey.
        /// </summary>
        private readonly string _id;

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

        public ExpressionKey(VrmLib.ExpressionPreset preset, string customName = null)
        {
            Preset = preset;

            if (Preset != VrmLib.ExpressionPreset.Custom)
            {
                if (PresetNameDictionary.ContainsKey((Preset)))
                {
                    _id = Name = PresetNameDictionary[Preset];
                }
                else
                {
                    PresetNameDictionary.Add(Preset, Preset.ToString());
                    _id = Name = PresetNameDictionary[Preset];
                }
            }
            else
            {
                if (string.IsNullOrEmpty(customName))
                {
                    throw new ArgumentException("name is required for VrmLib.ExpressionPreset.Custom");
                }

                _id = $"{UnknownPresetPrefix}{customName}";
                Name = customName;
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
            return _id.Replace(UnknownPresetPrefix, "");
        }

        public bool Equals(ExpressionKey other)
        {
            return _id == other._id;
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
            return _id.GetHashCode();
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
