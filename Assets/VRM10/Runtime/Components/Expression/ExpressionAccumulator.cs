using System.Collections.Generic;
using System.Linq;

namespace UniVRM10
{
    public class DefaultExpressionAccumulator : IExpressionAccumulator
    {
        Dictionary<ExpressionKey, VRM10Expression> m_clipMap;

        Dictionary<ExpressionKey, float> expressionKeyWeights = new Dictionary<ExpressionKey, float>();

        public bool IgnoreBlink { get; private set; }
        public bool IgnoreLookAt { get; private set; }
        public bool IgnoreMouth { get; private set; }

        /// <summary>
        /// initilaize
        /// </summary>
        /// <param name="avatar"></param>
        public void OnStart(VRM10ExpressionAvatar avatar)
        {
            m_clipMap = avatar.Clips.ToDictionary(x => ExpressionKey.CreateFromClip(x), x => x);
            expressionKeyWeights = m_clipMap.Keys.ToDictionary(x => x, x => 0.0f);
        }

        /// <summary>
        /// each frame
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<ExpressionKey, float>> FrameExpression()
        {
            IgnoreBlink = false;
            IgnoreLookAt = false;
            IgnoreMouth = false;

            //
            // except blink, lookat, mouth
            //
            foreach (var kv in expressionKeyWeights)
            {
                if (kv.Key.IsProcedual)
                {
                    // 後で
                    continue;
                }

                var expression = m_clipMap[kv.Key];

                if (expression.IgnoreBlink && kv.Value > 0)
                {
                    IgnoreBlink = true;
                }
                if (expression.IgnoreLookAt && kv.Value > 0)
                {
                    IgnoreLookAt = true;
                }
                if (expression.IgnoreMouth && kv.Value > 0)
                {
                    IgnoreMouth = true;
                }

                yield return kv;
            }

            //
            // blink, lookat, mouth
            //
            foreach (var kv in expressionKeyWeights)
            {
                if (kv.Key.IsBlink)
                {
                    if (IgnoreBlink)
                    {
                        // skip
                        continue;
                    }
                }
                else if (kv.Key.IsLookAt)
                {
                    if (IgnoreLookAt)
                    {
                        // skip
                        continue;
                    }
                }
                else if (kv.Key.IsMouth)
                {
                    if (IgnoreMouth)
                    {
                        // skip
                        continue;
                    }
                }
                else
                {
                    // already return
                    continue;
                }

                yield return kv;
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
    }
}
