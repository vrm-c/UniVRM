using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10ControllerExpression : IDisposable
    {
        [SerializeField]
        public VRM10ExpressionAvatar ExpressionAvatar;

        bool m_ignoreBlink;
        bool m_ignoreLookAt;
        bool m_ignoreMouth;

        #region for CustomEditor
        public bool IgnoreBlink => m_ignoreBlink;
        public bool IgnoreLookAt => m_ignoreLookAt;
        public bool IgnoreMouth => m_ignoreMouth;
        #endregion

        public Dictionary<ExpressionKey, float> expressionKeyWeights = new Dictionary<ExpressionKey, float>();

        List<ExpressionKey> m_blinkExpressionKeys = new List<ExpressionKey>();
        List<ExpressionKey> m_lookAtExpressionKeys = new List<ExpressionKey>();
        List<ExpressionKey> m_mouthExpressionKeys = new List<ExpressionKey>();

        ExpressionMerger m_merger;

        public void Dispose()
        {
            if (m_merger != null)
            {
                m_merger.RestoreMaterialInitialValues();
            }
        }

        public void OnStart(Transform transform)
        {
            if (ExpressionAvatar != null)
            {
                if (m_merger == null)
                {
                    m_merger = new ExpressionMerger(ExpressionAvatar.Clips, transform);
                }
            }

            expressionKeyWeights = m_merger.ExpressionKeys.ToDictionary(x => x, x => 0.0f);

            m_blinkExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Blink));
            m_blinkExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.BlinkLeft));
            m_blinkExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.BlinkRight));
            m_lookAtExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookUp));
            m_lookAtExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookDown));
            m_lookAtExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookLeft));
            m_lookAtExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.LookRight));
            m_mouthExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Aa));
            m_mouthExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ih));
            m_mouthExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ou));
            m_mouthExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Ee));
            m_mouthExpressionKeys.Add(m_merger.ExpressionKeys.FirstOrDefault(x => x.Preset == VrmLib.ExpressionPreset.Oh));
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

        #region Setter and Getter
        public float GetPresetValue(VrmLib.ExpressionPreset key)
        {
            var expressionKey = new ExpressionKey(key);
            if (this.expressionKeyWeights.ContainsKey(expressionKey))
            {
                return this.expressionKeyWeights[expressionKey];
            }
            else
            {
                return 0.0f;
            }
        }

        public float GetCustomValue(String key)
        {
            var expressionKey = ExpressionKey.CreateCustom(key);
            if (this.expressionKeyWeights.ContainsKey(expressionKey))
            {
                return this.expressionKeyWeights[expressionKey];
            }
            else
            {
                return 0.0f;
            }
        }

        public void SetPresetValue(VrmLib.ExpressionPreset key, float value)
        {
            var expressionKey = new ExpressionKey(key);
            if (this.expressionKeyWeights.ContainsKey(expressionKey))
            {
                this.expressionKeyWeights[expressionKey] = value;
            }
        }

        /// <parameter>key</parameter>    
        public void SetCustomValue(String key, float value)
        {
            var expressionKey = ExpressionKey.CreateCustom(key);
            if (this.expressionKeyWeights.ContainsKey(expressionKey))
            {
                this.expressionKeyWeights[expressionKey] = value;
            }
        }
        #endregion

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
                if (!ignoreBlink && m_merger.GetClip(keyWeight.Key).IgnoreBlink && !m_blinkExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreBlink = true;
                }

                // LookAt
                if (!ignoreLookAt && m_merger.GetClip(keyWeight.Key).IgnoreLookAt && !m_lookAtExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreLookAt = true;
                }

                // Mouth
                if (!ignoreMouth && m_merger.GetClip(keyWeight.Key).IgnoreMouth && !m_mouthExpressionKeys.Contains(keyWeight.Key))
                {
                    ignoreMouth = true;
                }
            }

            return (ignoreBlink, ignoreLookAt, ignoreMouth);
        }

        public void Apply()
        {
            var validateState = GetValidateState();
            m_ignoreBlink = validateState.ignoreBlink;
            m_ignoreLookAt = validateState.ignoreLookAt;
            m_ignoreMouth = validateState.ignoreMouth;

            m_merger.SetValues(expressionKeyWeights
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
                .Select(x => new KeyValuePair<ExpressionKey, float>(x.Key, x.Value)));

            m_merger.Apply();
        }
    }
}
