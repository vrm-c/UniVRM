using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniVRM10
{

    /// <summary>
    /// ブレンドシェイプを蓄えてまとめて適用するクラス
    /// </summary>
    class ExpressionMerger
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


        public ExpressionMerger(IEnumerable<VRM10Expression> clips, Transform root)
        {
            m_clipMap = clips.ToDictionary(x => ExpressionKey.CreateFromClip(x), x => x);

            m_valueMap = new Dictionary<ExpressionKey, float>();

            m_morphTargetBindingMerger = new MorphTargetBindingMerger(m_clipMap, root);
            m_materialValueBindingMerger = new MaterialValueBindingMerger(m_clipMap, root);
        }

        /// <summary>
        /// 蓄積した値を適用する
        /// </summary>
        public void Apply()
        {
            m_morphTargetBindingMerger.Apply();
            m_materialValueBindingMerger.Apply();
        }

        /// <summary>
        /// まとめて反映する。1フレームに1回呼び出されることを想定
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(IEnumerable<KeyValuePair<ExpressionKey, float>> values)
        {
            foreach (var kv in values)
            {
                AccumulateValue(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// 即時に反映しない。後にApplyによって反映する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AccumulateValue(ExpressionKey key, float value)
        {
            m_valueMap[key] = value;

            VRM10Expression clip;
            if (!m_clipMap.TryGetValue(key, out clip))
            {
                return;
            }

            if (clip.IsBinary)
            {
                value = Mathf.Round(value);
            }

            m_morphTargetBindingMerger.AccumulateValue(clip, value);
            m_materialValueBindingMerger.AccumulateValue(clip, value);
        }

        public void RestoreMaterialInitialValues()
        {
            m_materialValueBindingMerger.RestoreMaterialInitialValues();
        }
    }
}
