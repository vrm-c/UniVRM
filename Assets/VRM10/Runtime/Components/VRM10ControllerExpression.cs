using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    [Serializable]
    public sealed class VRM10ControllerExpression : IDisposable
    {
        public static IExpressionValidatorFactory ExpressionValidatorFactory = new DefaultExpressionValidator.Factory();
        
        [SerializeField]
        public VRM10ExpressionAvatar ExpressionAvatar;

        private List<ExpressionKey> _keys = new List<ExpressionKey>();
        private Dictionary<ExpressionKey, float> _inputWeights = new Dictionary<ExpressionKey, float>();
        private Dictionary<ExpressionKey, float> _actualWeights = new Dictionary<ExpressionKey, float>();
        private ExpressionMerger _merger;
        private IExpressionValidator _validator;

        public IReadOnlyList<ExpressionKey> ExpressionKeys => _keys;
        public IReadOnlyDictionary<ExpressionKey, float> ActualWeights => _actualWeights;
        
        public float OverrideBlinkWeight { get; private set; }
        public float OverrideLookAtWeight { get; private set; }
        public float OverrideMouthWeight { get; private set; }
        
        public void Dispose()
        {
            _merger?.RestoreMaterialInitialValues();
        }

        internal void Setup(Transform transform)
        {
            if (ExpressionAvatar == null)
            {
                Debug.LogError($"{nameof(VRM10ControllerExpression)}.{nameof(ExpressionAvatar)} is null.");
                return;
            }
            
            _merger = new ExpressionMerger(ExpressionAvatar.Clips, transform);
            _keys = ExpressionAvatar.Clips.Select(ExpressionKey.CreateFromClip).ToList();
            _inputWeights = _keys.ToDictionary(x => x, x => 0f);
            _actualWeights = _keys.ToDictionary(x => x, x => 0f);
            _validator = ExpressionValidatorFactory.Create(ExpressionAvatar);
        }

        internal void Process()
        {
            
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
        /// </summary>
        private void Apply()
        {
            _validator.Validate(_inputWeights, _actualWeights);
            _merger.SetValues(_actualWeights);
        }
    }
}
