using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    class AnimationCurveData
    {
#if UNITY_EDITOR
        public AnimationUtility.TangentMode TangentMode { get; private set; }
        public glTFAnimationTarget.AnimationPropertys AnimationProperty { get; private set; }
        public int SamplerIndex { get; private set; }
        public int ElementCount { get; private set; }
        public readonly List<AnimationKeyframeData> Keyframes = new List<AnimationKeyframeData>();

        public AnimationCurveData(AnimationUtility.TangentMode tangentMode, glTFAnimationTarget.AnimationPropertys property, int samplerIndex, int elementCount)
        {
            TangentMode = tangentMode;
            AnimationProperty = property;
            SamplerIndex = samplerIndex;
            ElementCount = elementCount;
        }

        public string GetInterpolation()
        {
            switch (TangentMode)
            {
                case AnimationUtility.TangentMode.Linear:
                    return glTFAnimationTarget.Interpolations.LINEAR.ToString();
                case AnimationUtility.TangentMode.Constant:
                    return glTFAnimationTarget.Interpolations.STEP.ToString();
                default:
                    return glTFAnimationTarget.Interpolations.LINEAR.ToString();
            }
        }

        /// <summary>
        /// キーフレームのデータを入力する
        /// </summary>
        /// <param name="time"></param>
        /// <param name="value"></param>
        /// <param name="valueOffset"></param>
        public void SetKeyframeData(float time, float value, int valueOffset)
        {
            var existKeyframe = Keyframes.FirstOrDefault(x => x.Time == time);
            if (existKeyframe != null)
            {
                existKeyframe.SetValue(value, valueOffset);
            }
            else
            {
                var newKeyframe = GetKeyframeData(AnimationProperty, ElementCount);
                newKeyframe.Time = time;
                newKeyframe.SetValue(value, valueOffset);
                Keyframes.Add(newKeyframe);
            }
        }

        /// <summary>
        /// キー情報がなかった要素に対して直前のキーの値を入力する
        /// </summary>
        public void RecountEmptyKeyframe()
        {
            if (Keyframes.Count == 0)
            {
                return;
            }

            Keyframes.Sort((x, y) => (x.Time < y.Time) ? -1 : 1);

            for (int i = 1; i < Keyframes.Count; i++)
            {
                var current = Keyframes[i];
                var last = Keyframes[i - 1];
                for (int j = 0; j < current.EnterValues.Length; j++)
                {
                    if (!current.EnterValues[j])
                    {
                        Keyframes[i].SetValue(last.Values[j], j);
                    }
                }

            }
        }

        /// <summary>
        /// アニメーションプロパティに対応したキーフレームを挿入する
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static AnimationKeyframeData GetKeyframeData(glTFAnimationTarget.AnimationPropertys property, int elementCount)
        {
            switch (property)
            {
                case glTFAnimationTarget.AnimationPropertys.Translation:
                    return new AnimationKeyframeData(elementCount, (values) =>
                    {
                        var temp = new Vector3(values[0], values[1], values[2]);
                        return temp.ReverseZ().ToArray();
                    });
                case glTFAnimationTarget.AnimationPropertys.Rotation:
                    return new AnimationKeyframeData(elementCount, (values) =>
                    {
                        var temp = new Quaternion(values[0], values[1], values[2], values[3]);
                        return temp.ReverseZ().ToArray();
                    });
                case glTFAnimationTarget.AnimationPropertys.Scale:
                    return new AnimationKeyframeData(elementCount, null);
                case glTFAnimationTarget.AnimationPropertys.BlendShape:
                    return new AnimationKeyframeData(elementCount, null);
                default:
                    return null;
            }
        }
#endif
    }
}