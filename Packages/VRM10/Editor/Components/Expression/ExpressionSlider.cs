using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public class ExpressionSlider
    {
        Dictionary<ExpressionKey, float> m_expressionKeys;
        ExpressionKey m_key;

        public ExpressionSlider(Dictionary<ExpressionKey, float> expressionKeys, ExpressionKey key)
        {
            m_expressionKeys = expressionKeys;
            m_key = key;
        }

        public KeyValuePair<ExpressionKey, float> Slider()
        {
            var oldValue = m_expressionKeys[m_key];
            var enable = GUI.enabled;
            GUI.enabled = Application.isPlaying;
            var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
            GUI.enabled = enable;
            return new KeyValuePair<ExpressionKey, float>(m_key, newValue);
        }
    }
}
