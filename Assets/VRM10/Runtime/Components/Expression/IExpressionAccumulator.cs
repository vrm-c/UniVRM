using System.Collections.Generic;

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
}
