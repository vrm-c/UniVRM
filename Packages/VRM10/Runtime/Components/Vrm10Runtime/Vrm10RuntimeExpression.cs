using System;
using System.Collections.Generic;
using System.Linq;

namespace UniVRM10
{
    public sealed class Vrm10RuntimeExpression : IDisposable
    {
        public static IExpressionValidatorFactory ExpressionValidatorFactory = new DefaultExpressionValidator.Factory();

        private List<ExpressionKey> _keys = new List<ExpressionKey>();
        private Dictionary<ExpressionKey, float> _inputWeights = new Dictionary<ExpressionKey, float>();
        private Dictionary<ExpressionKey, float> _actualWeights = new Dictionary<ExpressionKey, float>();
        private ExpressionMerger _merger;
        private IExpressionValidator _validator;
        private LookAtEyeDirection _actualEyeDirection;
        private ILookAtEyeDirectionApplicable _eyeDirectionApplicable;

        public IReadOnlyList<ExpressionKey> ExpressionKeys => _keys;
        public IReadOnlyDictionary<ExpressionKey, float> ActualWeights => _actualWeights;
        public LookAtEyeDirection ActualEyeDirection => _actualEyeDirection;
        public float BlinkOverrideRate { get; private set; }
        public float LookAtOverrideRate { get; private set; }
        public float MouthOverrideRate { get; private set; }

        internal Vrm10RuntimeExpression(Vrm10Instance target, ILookAtEyeDirectionApplicable eyeDirectionApplicable, bool isPrefabInstance)
        {
            _merger = new ExpressionMerger(target.Vrm.Expression, target.transform, isPrefabInstance);
            _keys = target.Vrm.Expression.Clips
                .Select(x => target.Vrm.Expression.CreateKey(x.Clip))
                .ToList();
            var oldInputWeights = _inputWeights;
            _inputWeights = _keys.ToDictionary(
                x => x,
                x => 0f,
                ExpressionKey.Comparer
            );
            foreach (var key in _keys)
            {
                // remain user input weights.
                if (oldInputWeights.ContainsKey(key)) _inputWeights[key] = oldInputWeights[key];
            }
            _actualWeights = _keys.ToDictionary(
                x => x,
                x => 0f,
                ExpressionKey.Comparer
            );
            _validator = ExpressionValidatorFactory.Create(target.Vrm.Expression);
            _eyeDirectionApplicable = eyeDirectionApplicable;
        }

        public void Dispose()
        {
            _merger?.Dispose();
            _merger = null;

            _eyeDirectionApplicable?.Restore();
            _eyeDirectionApplicable = null;
        }

        internal void Process(LookAtEyeDirection inputEyeDirection)
        {
            Apply(inputEyeDirection);
        }

        public IDictionary<ExpressionKey, float> GetWeights()
        {
            return _inputWeights;
        }

        public float GetWeight(ExpressionKey expressionKey)
        {
            if (_inputWeights.ContainsKey(expressionKey))
            {
                return _inputWeights[expressionKey];
            }

            return 0f;
        }

        public void SetWeights(IEnumerable<KeyValuePair<ExpressionKey, float>> weights)
        {
            foreach (var (expressionKey, weight) in weights)
            {
                if (_inputWeights.ContainsKey(expressionKey))
                {
                    _inputWeights[expressionKey] = weight;
                }
            }
        }

        public void SetWeightsNonAlloc(Dictionary<ExpressionKey, float> weights)
        {
            foreach (var (expressionKey, weight) in weights)
            {
                if (_inputWeights.ContainsKey(expressionKey))
                {
                    _inputWeights[expressionKey] = weight;
                }
            }
        }

        public void SetWeight(ExpressionKey expressionKey, float weight)
        {
            if (_inputWeights.ContainsKey(expressionKey))
            {
                _inputWeights[expressionKey] = weight;
            }
        }

        /// <summary>
        /// 入力 Weight を基に、Validation を行い実際にモデルに適用される Weights を計算し、Merger を介して適用する。
        /// この際、LookAt の情報を pull してそれも適用する。
        /// </summary>
        private void Apply(LookAtEyeDirection inputEyeDirection)
        {
            // 1. Validate user input, and Output as actual weights.
            _validator.Validate(_inputWeights, _actualWeights,
                inputEyeDirection, out _actualEyeDirection,
                out var blink, out var lookAt, out var mouth);

            // 2. Set eye direction expression weights or any other side-effects (ex. eye bone).
            _eyeDirectionApplicable?.Apply(_actualEyeDirection, _actualWeights);

            // 3. Set actual weights to raw blendshapes.
            _merger.SetValues(_actualWeights);

            BlinkOverrideRate = blink;
            LookAtOverrideRate = lookAt;
            MouthOverrideRate = mouth;
        }
    }
}
