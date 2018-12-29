using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{

    /// <summary>
    /// ブレンドシェイプを蓄えてまとめて適用するクラス
    /// </summary>
    class BlendShapeMerger
    {
        /// <summary>
        /// Key からBlendShapeClipを得る
        /// </summary>
        Dictionary<BlendShapeKey, BlendShapeClip> m_clipMap;

        /// <summary>
        /// BlendShape のWeightを記録する
        /// </summary>
        Dictionary<BlendShapeKey, float> m_valueMap;

        BlendShapeBindingMerger m_blendShapeBindingMerger;

        MaterialValueBindingMerger m_materialValueBindingMerger;


        public BlendShapeMerger(IEnumerable<BlendShapeClip> clips, Transform root)
        {
            m_clipMap = clips.ToDictionary(x => BlendShapeKey.CreateFrom(x), x => x);

            m_valueMap = new Dictionary<BlendShapeKey, float>();

            m_blendShapeBindingMerger = new BlendShapeBindingMerger(m_clipMap, root);
            m_materialValueBindingMerger = new MaterialValueBindingMerger(m_clipMap, root);
        }

        /*
        public void Clear()
        {
            foreach (var kv in m_valueMap.ToArray())
            {
                SetValue(kv.Key, kv.Value, false);
            }
            Apply();
        }
        */

        /// <summary>
        /// 蓄積した値を適用する
        /// </summary>
        public void Apply()
        {
            m_blendShapeBindingMerger.Apply();
            m_materialValueBindingMerger.Apply();
        }

        /// <summary>
        /// まとめて反映する。1フレームに1回呼び出されることを想定
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(IEnumerable<KeyValuePair<BlendShapeKey, float>> values)
        {
            foreach (var kv in values)
            {
                AccumulateValue(kv.Key, kv.Value);
            }
            Apply();
        }

        /// <summary>
        /// 即時に反映しない。後にApplyによって反映する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AccumulateValue(BlendShapeKey key, float value)
        {
            m_valueMap[key] = value;

            BlendShapeClip clip;
            if (!m_clipMap.TryGetValue(key, out clip))
            {
                return;
            }

            if (clip.IsBinary)
            {
                value = Mathf.Round(value);
            }

            m_blendShapeBindingMerger.AccumulateValue(clip, value);
            m_materialValueBindingMerger.AccumulateValue(clip, value);
        }

        /// <summary>
        /// 即時に反映する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ImmediatelySetValue(BlendShapeKey key, float value)
        {
            m_valueMap[key] = value;

            BlendShapeClip clip;
            if (!m_clipMap.TryGetValue(key, out clip))
            {
                return;
            }

            if (clip.IsBinary)
            {
                value = Mathf.Round(value);
            }

            m_blendShapeBindingMerger.ImmediatelySetValue(clip, value);
            m_materialValueBindingMerger.ImmediatelySetValue(clip, value);
        }

        public void SetValue(BlendShapeKey key, float value, bool immediately)
        {
            if (immediately)
            {
                ImmediatelySetValue(key, value);
            }
            else
            {
                AccumulateValue(key, value);
            }
        }

        public float GetValue(BlendShapeKey key)
        {
            float value;
            if (!m_valueMap.TryGetValue(key, out value))
            {
                return 0;
            }
            return value;
        }

        public void RestoreMaterialInitialValues(IEnumerable<BlendShapeClip> clips)
        {
            m_materialValueBindingMerger.RestoreMaterialInitialValues(clips);
        }
    }
}
