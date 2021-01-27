using System.Collections.Generic;
using System.Linq;

namespace UniVRM10
{
    /// <summary>
    /// １フレーム分の Expression を蓄える
    /// </summary>
    public interface IExpressionAccumulator
    {
        /// <summary>
        /// 開始時に初期化する
        /// </summary>
        /// <param name="avatar"></param>
        void OnStart(VRM10ExpressionAvatar avatar);

        /// <summary>
        /// 蓄えて処理(ignoreフラグなど)した結果を得る
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<ExpressionKey, float>> FrameExpression();

        void SetValue(ExpressionKey key, float value);
        void SetValues(IEnumerable<KeyValuePair<ExpressionKey, float>> values);
        float GetValue(ExpressionKey key);
        IEnumerable<KeyValuePair<ExpressionKey, float>> GetValues();

        bool IgnoreBlink { get; }
        bool IgnoreLookAt { get; }
        bool IgnoreMouth { get; }
    }

    public static class ExpressionAccumulatorExtensions
    {
        public static float GetPresetValue(this IExpressionAccumulator self, VrmLib.ExpressionPreset key)
        {
            var expressionKey = new ExpressionKey(key);
            return self.GetValue(expressionKey);
        }

        public static float GetCustomValue(this IExpressionAccumulator self, string key)
        {
            var expressionKey = ExpressionKey.CreateCustom(key);
            return self.GetValue(expressionKey);
        }

        public static void SetPresetValue(this IExpressionAccumulator self, VrmLib.ExpressionPreset key, float value)
        {
            var expressionKey = new ExpressionKey(key);
            self.SetValue(expressionKey, value);
        }

        public static void SetCustomValue(this IExpressionAccumulator self, string key, float value)
        {
            var expressionKey = ExpressionKey.CreateCustom(key);
            self.SetValue(expressionKey, value);
        }
    }

    public class DefaultExpressionAccumulator : IExpressionAccumulator
    {
        bool m_ignoreBlink;
        bool m_ignoreLookAt;
        bool m_ignoreMouth;

        public bool IgnoreBlink => m_ignoreBlink;
        public bool IgnoreLookAt => m_ignoreLookAt;
        public bool IgnoreMouth => m_ignoreMouth;

        public Dictionary<ExpressionKey, float> expressionKeyWeights = new Dictionary<ExpressionKey, float>();

        List<ExpressionKey> m_blinkExpressionKeys = new List<ExpressionKey>();
        List<ExpressionKey> m_lookAtExpressionKeys = new List<ExpressionKey>();
        List<ExpressionKey> m_mouthExpressionKeys = new List<ExpressionKey>();

        /// <summary>
        /// Key から Expression を得る
        /// </summary>
        Dictionary<ExpressionKey, VRM10Expression> m_clipMap;

        public void OnStart(VRM10ExpressionAvatar avatar)
        {
            m_clipMap = avatar.Clips.ToDictionary(x => ExpressionKey.CreateFromClip(x), x => x);

            expressionKeyWeights = m_clipMap.Keys.ToDictionary(x => x, x => 0.0f);

            m_blinkExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Blink));
            m_blinkExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.BlinkLeft));
            m_blinkExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.BlinkRight));
            m_lookAtExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookUp));
            m_lookAtExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookDown));
            m_lookAtExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookLeft));
            m_lookAtExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookRight));
            m_mouthExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Aa));
            m_mouthExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ih));
            m_mouthExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ou));
            m_mouthExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ee));
            m_mouthExpressionKeys.Add(m_clipMap.Keys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Oh));
        }

        public VRM10Expression GetClip(ExpressionKey key)
        {
            if (m_clipMap.ContainsKey(key))
            {
                return m_clipMap[key];
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<ExpressionKey, float>> FrameExpression()
        {
            var validateState = GetValidateState();
            m_ignoreBlink = validateState.ignoreBlink;
            m_ignoreLookAt = validateState.ignoreLookAt;
            m_ignoreMouth = validateState.ignoreMouth;

            return expressionKeyWeights
                .Where(x =>
                {
                    // skip ignore
                    if (validateState.ignoreBlink)
                    {
                        switch (x.Key.Preset)
                        {
                            case VrmLib.ExpressionPreset.Blink:
                            case VrmLib.ExpressionPreset.BlinkLeft:
                            case VrmLib.ExpressionPreset.BlinkRight:
                                return false;
                        }
                    }
                    if (validateState.ignoreMouth)
                    {
                        switch (x.Key.Preset)
                        {
                            case VrmLib.ExpressionPreset.Aa:
                            case VrmLib.ExpressionPreset.Ih:
                            case VrmLib.ExpressionPreset.Ou:
                            case VrmLib.ExpressionPreset.Ee:
                            case VrmLib.ExpressionPreset.Oh:
                                return false;
                        }
                    }
                    if (validateState.ignoreLookAt)
                    {
                        switch (x.Key.Preset)
                        {
                            case VrmLib.ExpressionPreset.LookUp:
                            case VrmLib.ExpressionPreset.LookDown:
                            case VrmLib.ExpressionPreset.LookLeft:
                            case VrmLib.ExpressionPreset.LookRight:
                                return false;
                        }
                    }

                    return true;
                })
                .Select(x => new KeyValuePair<ExpressionKey, float>(x.Key, x.Value));
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(ExpressionKey key, float value)
        {
            if (expressionKeyWeights.ContainsKey(key))
            {
                expressionKeyWeights[key] = value;
            }
        }

        /// <summary>
        /// Get a expression value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetValue(ExpressionKey key)
        {
            if (expressionKeyWeights.ContainsKey(key))
            {
                return expressionKeyWeights[key];
            }
            else
            {
                return 0.0f;
            }
        }

        public IEnumerable<KeyValuePair<ExpressionKey, float>> GetValues()
        {
            return expressionKeyWeights.Select(x => new KeyValuePair<ExpressionKey, float>(x.Key, x.Value));
        }

        /// <summary>
        /// Set expression values.
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(IEnumerable<KeyValuePair<ExpressionKey, float>> values)
        {
            foreach (var keyValue in values)
            {
                if (expressionKeyWeights.ContainsKey(keyValue.Key))
                {
                    expressionKeyWeights[keyValue.Key] = keyValue.Value;
                }
            }
        }

        public IEnumerable<ExpressionKey> GetKeys()
        {
            return expressionKeyWeights.Keys;
        }

        private (bool ignoreBlink, bool ignoreLookAt, bool ignoreMouth) GetValidateState()
        {
            var ignoreBlink = false;
            var ignoreLookAt = false;
            var ignoreMouth = false;

            foreach (var keyWeight in expressionKeyWeights)
            {
                if (keyWeight.Value <= 0.0f)
                    continue;

                // Blink
                if (!ignoreBlink && GetClip(keyWeight.Key).IgnoreBlink && !m_blinkExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreBlink = true;
                }

                // LookAt
                if (!ignoreLookAt && GetClip(keyWeight.Key).IgnoreLookAt && !m_lookAtExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreLookAt = true;
                }

                // Mouth
                if (!ignoreMouth && GetClip(keyWeight.Key).IgnoreMouth && !m_mouthExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreMouth = true;
                }
            }

            return (ignoreBlink, ignoreLookAt, ignoreMouth);
        }

    }

}
