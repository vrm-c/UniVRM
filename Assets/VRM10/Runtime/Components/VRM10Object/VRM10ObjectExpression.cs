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
        #region Preset
        [SerializeField, Header("Emotion")]
        public VRM10Expression Happy;

        [SerializeField]
        public VRM10Expression Angry;

        [SerializeField]
        public VRM10Expression Sad;

        [SerializeField]
        public VRM10Expression Relaxed;

        [SerializeField]
        public VRM10Expression Surprised;

        [SerializeField, Header("LipSync")]
        public VRM10Expression Aa;

        [SerializeField]
        public VRM10Expression Ih;

        [SerializeField]
        public VRM10Expression Ou;

        [SerializeField]
        public VRM10Expression Ee;

        [SerializeField]
        public VRM10Expression Oh;

        [SerializeField, Header("Blink")]
        public VRM10Expression Blink;

        [SerializeField]
        public VRM10Expression BlinkLeft;

        [SerializeField]
        public VRM10Expression BlinkRight;

        [SerializeField, Header("LookAt")]
        public VRM10Expression LookUp;

        [SerializeField]
        public VRM10Expression LookDown;

        [SerializeField]
        public VRM10Expression LookLeft;

        [SerializeField]
        public VRM10Expression LookRight;

        [SerializeField, Header("Other")]
        public VRM10Expression Neutral;
        #endregion

        [SerializeField]
        public List<VRM10Expression> CustomClips = new List<VRM10Expression>();

        public IEnumerable<VRM10Expression> Clips
        {
            get
            {
                if (Happy != null) yield return Happy;
                if (Angry != null) yield return Angry;
                if (Sad != null) yield return Sad;
                if (Relaxed != null) yield return Relaxed;
                if (Surprised != null) yield return Surprised;
                if (Aa != null) yield return Aa;
                if (Ih != null) yield return Ih;
                if (Ou != null) yield return Ou;
                if (Ee != null) yield return Ee;
                if (Oh != null) yield return Oh;
                if (Blink != null) yield return Blink;
                if (BlinkLeft != null) yield return BlinkLeft;
                if (BlinkRight != null) yield return BlinkRight;
                if (LookUp != null) yield return LookUp;
                if (LookDown != null) yield return LookDown;
                if (LookLeft != null) yield return LookLeft;
                if (LookRight != null) yield return LookRight;
                if (Neutral != null) yield return Neutral;
                foreach (var clip in CustomClips)
                {
                    if (clip != null)
                    {
                        yield return clip;
                    }
                }
            }
        }

        public void AddClip(VRM10Expression clip)
        {
            switch (clip.Preset)
            {
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.happy: Happy = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.angry: Angry = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.sad: Sad = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.relaxed: Relaxed = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.surprised: Surprised = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa: Aa = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih: Ih = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou: Ou = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee: Ee = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh: Oh = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blink: Blink = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkLeft: BlinkLeft = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkRight: BlinkRight = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookUp: LookUp = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookDown: LookDown = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookLeft: LookLeft = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookRight: LookRight = clip; break;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.neutral: Neutral = clip; break;
                default: CustomClips.Add(clip); break;
            }
        }

        public void Replace(IDictionary<VRM10Expression, VRM10Expression> map)
        {
            foreach (var (k, v) in map)
            {
                Replace(k, v);
            }
        }

        void Replace(VRM10Expression src, VRM10Expression dst)
        {
            if (Happy == src)
            {
                Happy = dst; return;
            }
            if (Angry == src)
            {
                Angry = dst; return;
            }
            if (Sad == src)
            {
                Sad = dst; return;
            }
            if (Relaxed == src)
            {
                Relaxed = dst; return;
            }
            if (Surprised == src)
            {
                Surprised = dst; return;
            }
            if (Aa == src)
            {
                Aa = dst; return;
            }
            if (Ih == src)
            {
                Ih = dst; return;
            }
            if (Ou == src)
            {
                Ou = dst; return;
            }
            if (Ee == src)
            {
                Ee = dst; return;
            }
            if (Oh == src)
            {
                Oh = dst; return;
            }
            if (Blink == src)
            {
                Blink = dst; return;
            }
            if (BlinkLeft == src)
            {
                BlinkLeft = dst; return;
            }
            if (BlinkRight == src)
            {
                BlinkRight = dst; return;
            }
            if (LookUp == src)
            {
                LookUp = dst; return;
            }
            if (LookDown == src)
            {
                LookDown = dst; return;
            }
            if (LookLeft == src)
            {
                LookLeft = dst; return;
            }
            if (LookRight == src)
            {
                LookRight = dst; return;
            }
            if (Neutral == src)
            {
                Neutral = dst; return;
            }

            for (int i = 0; i < CustomClips.Count; ++i)
            {
                if (CustomClips[i] == src)
                {
                    CustomClips[i] = dst;
                    return;
                }
            }
        }

        /// <summary>
        /// NullのClipを削除して詰める
        /// </summary>
        public void RemoveNullClip()
        {
            if (CustomClips == null)
            {
                return;
            }
            for (int i = CustomClips.Count - 1; i >= 0; --i)
            {
                if (CustomClips[i] == null)
                {
                    CustomClips.RemoveAt(i);
                }
            }
        }

        // /// <summary>
        // /// Unknown以外で存在しないものを全て作る
        // /// </summary>
        // public void CreateDefaultPreset()
        // {
        //     foreach (var preset in ((UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)))
        //         .Where(x => x != UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
        //         )
        //     {
        //         CreateDefaultPreset(preset);
        //     }
        // }

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
