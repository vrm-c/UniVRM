using System;
using System.Collections.Generic;
using VRMShaders;

namespace UniVRM10
{
    [Serializable]
    public readonly struct ExpressionKey : IEquatable<ExpressionKey>, IComparable<ExpressionKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<ExpressionPreset, string> PresetNameDictionary =
            new Dictionary<ExpressionPreset, string>();

        /// <summary>
        ///  ExpressionPreset と同名の名前を持つ独自に追加した Expression を区別するための prefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        /// <summary>
        /// Preset of this ExpressionKey.
        /// </summary>
        public readonly ExpressionPreset Preset;

        /// <summary>
        /// Custom Name of this ExpressionKey.
        /// This works if Preset was Custom.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Key's hashcode for comparison.
        /// </summary>
        private readonly int _hashCode;

        public bool IsBlink
        {
            get
            {
                switch (Preset)
                {
                    case ExpressionPreset.blink:
                    case ExpressionPreset.blinkLeft:
                    case ExpressionPreset.blinkRight:
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
                    case ExpressionPreset.lookUp:
                    case ExpressionPreset.lookDown:
                    case ExpressionPreset.lookLeft:
                    case ExpressionPreset.lookRight:
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
                    case ExpressionPreset.aa:
                    case ExpressionPreset.ih:
                    case ExpressionPreset.ou:
                    case ExpressionPreset.ee:
                    case ExpressionPreset.oh:
                        return true;
                }
                return false;
            }
        }

        public bool IsProcedual => IsBlink || IsLookAt || IsMouth;

        public ExpressionKey(ExpressionPreset preset, string customName = null)
        {
            Preset = preset;

            if (Preset != ExpressionPreset.custom)
            {
                if (PresetNameDictionary.ContainsKey((Preset)))
                {
                    Name = PresetNameDictionary[Preset];
                    _hashCode = Name.GetHashCode();
                }
                else
                {
                    PresetNameDictionary.Add(Preset, Preset.ToString());
                    Name = PresetNameDictionary[Preset];
                    _hashCode = Name.GetHashCode();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(customName))
                {
                    throw new ArgumentException("name is required for ExpressionPreset.Custom");
                }

                Name = customName;
                _hashCode = $"{UnknownPresetPrefix}{customName}".GetHashCode();
            }
        }

        public static ExpressionKey CreateCustom(String key)
        {
            return new ExpressionKey(ExpressionPreset.custom, key);
        }

        public static ExpressionKey CreateFromPreset(ExpressionPreset preset)
        {
            return new ExpressionKey(preset);
        }

        public static ExpressionKey Happy => CreateFromPreset(ExpressionPreset.happy);
        public static ExpressionKey Angry => CreateFromPreset(ExpressionPreset.angry);
        public static ExpressionKey Sad => CreateFromPreset(ExpressionPreset.sad);
        public static ExpressionKey Relaxed => CreateFromPreset(ExpressionPreset.relaxed);
        public static ExpressionKey Surprised => CreateFromPreset(ExpressionPreset.surprised);
        public static ExpressionKey Aa => CreateFromPreset(ExpressionPreset.aa);
        public static ExpressionKey Ih => CreateFromPreset(ExpressionPreset.ih);
        public static ExpressionKey Ou => CreateFromPreset(ExpressionPreset.ou);
        public static ExpressionKey Ee => CreateFromPreset(ExpressionPreset.ee);
        public static ExpressionKey Oh => CreateFromPreset(ExpressionPreset.oh);
        public static ExpressionKey Blink => CreateFromPreset(ExpressionPreset.blink);
        public static ExpressionKey BlinkLeft => CreateFromPreset(ExpressionPreset.blinkLeft);
        public static ExpressionKey BlinkRight => CreateFromPreset(ExpressionPreset.blinkRight);
        public static ExpressionKey LookUp => CreateFromPreset(ExpressionPreset.lookUp);
        public static ExpressionKey LookDown => CreateFromPreset(ExpressionPreset.lookDown);
        public static ExpressionKey LookLeft => CreateFromPreset(ExpressionPreset.lookLeft);
        public static ExpressionKey LookRight => CreateFromPreset(ExpressionPreset.lookRight);
        public static ExpressionKey Neutral => CreateFromPreset(ExpressionPreset.neutral);


        public override string ToString()
        {
            return Name;
        }

        public bool Equals(ExpressionKey other)
        {
            // Early pruning
            if (_hashCode != other._hashCode) return false;

            if (Preset != other.Preset) return false;
            if (Preset != ExpressionPreset.custom) return true;
            return Name.Equals(other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is ExpressionKey key)
            {
                return Equals(key);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public int CompareTo(ExpressionKey other)
        {
            if (Preset != other.Preset)
            {
                return Preset - other.Preset;
            }

            return 0;
        }

        public SubAssetKey SubAssetKey
        {
            get
            {
                return new SubAssetKey(typeof(VRM10Expression), this.ToString());
            }
        }

        public static IEqualityComparer<ExpressionKey> Comparer { get; } = new EqualityComparer();

        internal sealed class EqualityComparer : IEqualityComparer<ExpressionKey>
        {
            public bool Equals(ExpressionKey x, ExpressionKey y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(ExpressionKey obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
