using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    [Serializable]
    public sealed class VRM10ControllerExpression
    {
        public static IExpressionValidatorFactory ExpressionValidatorFactory = new DefaultExpressionValidator.Factory();

        [SerializeField]
        public VRM10ExpressionAvatar ExpressionAvatar;

        private List<ExpressionKey> _keys = new List<ExpressionKey>();
        private Dictionary<ExpressionKey, float> _inputWeights = new Dictionary<ExpressionKey, float>();
        private Dictionary<ExpressionKey, float> _actualWeights = new Dictionary<ExpressionKey, float>();
        private ExpressionMerger _merger;
        private IExpressionValidator _validator;
        private LookAtEyeDirection _inputEyeDirection;
        private LookAtEyeDirection _actualEyeDirection;
        private ILookAtEyeDirectionProvider _eyeDirectionProvider;
        private ILookAtEyeDirectionApplicable _eyeDirectionApplicable;

        public IReadOnlyList<ExpressionKey> ExpressionKeys => _keys;
        public IReadOnlyDictionary<ExpressionKey, float> ActualWeights => _actualWeights;
        public LookAtEyeDirection ActualEyeDirection => _actualEyeDirection;
        public float BlinkOverrideRate { get; private set; }
        public float LookAtOverrideRate { get; private set; }
        public float MouthOverrideRate { get; private set; }

        int m_debugCount;

        internal void Setup(Transform transform, ILookAtEyeDirectionProvider eyeDirectionProvider, ILookAtEyeDirectionApplicable eyeDirectionApplicable)
        {
            if (ExpressionAvatar == null)
            {
#if VRM_DEVELOP          
                if (m_debugCount++ == 0)
                {
                    Debug.LogWarning($"{nameof(VRM10ControllerExpression)}.{nameof(ExpressionAvatar)} is null.");
                }
#endif                
                return;
            }

            Restore();

            _merger = new ExpressionMerger(ExpressionAvatar.Clips, transform);
            _keys = ExpressionAvatar.Clips.Select(ExpressionKey.CreateFromClip).ToList();
            var oldInputWeights = _inputWeights;
            _inputWeights = _keys.ToDictionary(x => x, x => 0f);
            foreach (var key in _keys)
            {
                // remain user input weights.
                if (oldInputWeights.ContainsKey(key)) _inputWeights[key] = oldInputWeights[key];
            }
            _actualWeights = _keys.ToDictionary(x => x, x => 0f);
            _validator = ExpressionValidatorFactory.Create(ExpressionAvatar);
            _eyeDirectionProvider = eyeDirectionProvider;
            _eyeDirectionApplicable = eyeDirectionApplicable;
        }

        internal void Restore()
        {
            _merger?.RestoreMaterialInitialValues();
            _merger = null;

            _eyeDirectionApplicable?.Restore();
            _eyeDirectionApplicable = null;
        }

        public void Process()
        {
            Apply();
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

        public LookAtEyeDirection GetEyeDirection()
        {
            return _inputEyeDirection;
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
            Apply();
        }

        public void SetWeight(ExpressionKey expressionKey, float weight)
        {
            if (_inputWeights.ContainsKey(expressionKey))
            {
                _inputWeights[expressionKey] = weight;
            }
            Apply();
        }

        /// <summary>
        /// 入力 Weight を基に、Validation を行い実際にモデルに適用される Weights を計算し、Merger を介して適用する。
        /// この際、LookAt の情報を pull してそれも適用する。
        /// </summary>
        private void Apply()
        {
            // 1. Get eye direction from provider.
            _inputEyeDirection = _eyeDirectionProvider?.EyeDirection ?? default;

            // 2. Validate user input, and Output as actual weights.
            _validator.Validate(_inputWeights, _actualWeights,
                _inputEyeDirection, out _actualEyeDirection,
                out var blink, out var lookAt, out var mouth);

            // 3. Set eye direction expression weights or any other side-effects (ex. eye bone).
            _eyeDirectionApplicable?.Apply(_actualEyeDirection, _actualWeights);

            // 4. Set actual weights to raw blendshapes.
            _merger.SetValues(_actualWeights);

            BlinkOverrideRate = blink;
            LookAtOverrideRate = lookAt;
            MouthOverrideRate = mouth;
        }
    }
}
