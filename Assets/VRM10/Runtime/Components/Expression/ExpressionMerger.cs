using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{

    /// <summary>
    /// ブレンドシェイプを蓄えてまとめて適用するクラス
    /// </summary>
    internal sealed class ExpressionMerger
    {
        /// <summary>
        /// Key から Expression を得る
        /// </summary>
        Dictionary<ExpressionKey, VRM10Expression> m_clipMap;

        /// <summary>
        /// Expression のWeightを記録する
        /// </summary>
        Dictionary<ExpressionKey, float> m_valueMap;

        MorphTargetBindingMerger m_morphTargetBindingMerger;
        MaterialValueBindingMerger m_materialValueBindingMerger;


        public ExpressionMerger(VRM10ObjectExpression expressions, Transform root)
        {
            m_clipMap = expressions.Clips.ToDictionary(
                x => expressions.CreateKey(x.Clip),
                x => x.Clip,
                ExpressionKey.Comparer
            );
            m_valueMap = new Dictionary<ExpressionKey, float>(ExpressionKey.Comparer);
            m_morphTargetBindingMerger = new MorphTargetBindingMerger(m_clipMap, root);
            m_materialValueBindingMerger = new MaterialValueBindingMerger(m_clipMap, root);
        }

        /// <summary>
        /// まとめて反映する。1フレームに1回呼び出されることを想定
        /// </summary>
        /// <param name="expressionWeights"></param>
        public void SetValues(Dictionary<ExpressionKey, float> expressionWeights)
        {
            foreach (var (key, weight) in expressionWeights)
            {
                AccumulateValue(key, weight);
            }

            m_morphTargetBindingMerger.Apply();
            m_materialValueBindingMerger.Apply();
        }

        private void AccumulateValue(ExpressionKey key, float value)
        {
            m_valueMap[key] = value;

            VRM10Expression clip;
            if (!m_clipMap.TryGetValue(key, out clip))
            {
                return;
            }

            if (clip.IsBinary)
            {
                value = value > 0 ? 1 : 0;
            }

            m_morphTargetBindingMerger.AccumulateValue(key, value);
            m_materialValueBindingMerger.AccumulateValue(clip, value);
        }

        public void RestoreMaterialInitialValues()
        {
            m_materialValueBindingMerger.RestoreMaterialInitialValues();
        }
    }
}
