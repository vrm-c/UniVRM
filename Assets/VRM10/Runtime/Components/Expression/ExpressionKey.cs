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
        private static readonly Dictionary<UniGLTF.Extensions.VRMC_vrm.ExpressionPreset, string> PresetNameDictionary =
            new Dictionary<UniGLTF.Extensions.VRMC_vrm.ExpressionPreset, string>();

        /// <summary>
        ///  ExpressionPreset と同名の名前を持つ独自に追加した Expression を区別するための prefix
        /// </summary>
        private static readonly string UnknownPresetPrefix = "Unknown_";

        /// <summary>
        /// Preset of this ExpressionKey.
        /// </summary>
        public readonly UniGLTF.Extensions.VRMC_vrm.ExpressionPreset Preset;

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
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blink:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkLeft:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkRight:
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
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookUp:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookDown:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookLeft:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookRight:
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
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee:
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh:
                        return true;
                }
                return false;
            }
        }

        public bool IsProcedual => IsBlink || IsLookAt || IsMouth;

        public ExpressionKey(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset, string customName = null)
        {
            Preset = preset;

            if (Preset != UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
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
                    throw new ArgumentException("name is required for UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.Custom");
                }

                _id = $"{UnknownPresetPrefix}{customName}";
                Name = customName;
            }
        }

        public static ExpressionKey CreateCustom(String key)
        {
            return new ExpressionKey(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom, key);
        }

        public static ExpressionKey CreateFromPreset(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset)
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

        public string ExtractKey
        {
            get
            {
                return $"Expression.{this}";
            }
        }
    }
}
