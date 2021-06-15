using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    [Serializable]
    public sealed class VRM10ObjectExpression
    {
        [SerializeField]
        public List<VRM10Expression> Clips = new List<VRM10Expression>();
        /// <summary>
        /// NullのClipを削除して詰める
        /// </summary>
        public void RemoveNullClip()
        {
            if (Clips == null)
            {
                return;
            }
            for (int i = Clips.Count - 1; i >= 0; --i)
            {
                if (Clips[i] == null)
                {
                    Clips.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Unknown以外で存在しないものを全て作る
        /// </summary>
        public void CreateDefaultPreset()
        {
            foreach (var preset in ((UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)))
                .Where(x => x != UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
                )
            {
                CreateDefaultPreset(preset);
            }
        }

        void CreateDefaultPreset(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset)
        {
            var clip = GetClip(new ExpressionKey(preset));
            if (clip != null) return;
            clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.name = preset.ToString();
            clip.ExpressionName = preset.ToString();
            clip.Preset = preset;
            Clips.Add(clip);
        }

        public void SetClip(ExpressionKey key, VRM10Expression clip)
        {
            int index = -1;
            try
            {
                index = Clips.FindIndex(x => key.Match(x));
            }
            catch (Exception)
            {

            }
            if (index == -1)
            {
                Clips.Add(clip);
            }
            else
            {
                Clips[index] = clip;
            }
        }

        public VRM10Expression GetClip(ExpressionKey key)
        {
            if (Clips == null) return null;
            return Clips.FirstOrDefault(x => key.Match(x));
        }

        public static IExpressionValidatorFactory ExpressionValidatorFactory = new DefaultExpressionValidator.Factory();

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

        internal void Setup(VRM10Controller target, ILookAtEyeDirectionProvider eyeDirectionProvider, ILookAtEyeDirectionApplicable eyeDirectionApplicable)
        {
            Restore();

            _merger = new ExpressionMerger(Clips, target.transform);
            _keys = Clips.Select(ExpressionKey.CreateFromClip).ToList();
            var oldInputWeights = _inputWeights;
            _inputWeights = _keys.ToDictionary(x => x, x => 0f);
            foreach (var key in _keys)
            {
                // remain user input weights.
                if (oldInputWeights.ContainsKey(key)) _inputWeights[key] = oldInputWeights[key];
            }
            _actualWeights = _keys.ToDictionary(x => x, x => 0f);
            _validator = ExpressionValidatorFactory.Create(this);
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
