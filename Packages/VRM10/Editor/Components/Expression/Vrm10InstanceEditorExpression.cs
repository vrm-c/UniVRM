using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    //
    // Expression 向けの Inspector
    // 
    // Runtime に Expression 操作用の Slider を表示する
    //
    class Vrm10InstanceEditorExpression
    {
        Vrm10Instance m_target;
        Dictionary<ExpressionKey, float> m_expressionKeyWeights = new Dictionary<ExpressionKey, float>();
        List<ExpressionSlider> m_sliders;

        public Vrm10InstanceEditorExpression(Vrm10Instance target)
        {
            m_target = target;

            m_expressionKeyWeights = m_target.Vrm.Expression.Clips.ToDictionary(x => target.Vrm.Expression.CreateKey(x.Clip), x => 0.0f);
            m_sliders = m_target.Vrm.Expression.Clips
                .Where(x => x.Clip != null)
                .Select(x => new ExpressionSlider(m_expressionKeyWeights, target.Vrm.Expression.CreateKey(x.Clip)))
                .ToList()
                ;
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enable when playing", MessageType.Info);
                return;
            }

            if (m_sliders != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Expression Weights", EditorStyles.boldLabel);

                var sliders = m_sliders.Select(x => x.Slider());
                foreach (var slider in sliders)
                {
                    m_expressionKeyWeights[slider.Key] = slider.Value;
                }
                m_target.Runtime.Expression.SetWeights(m_expressionKeyWeights);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Override rates", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.Slider("Blink override rate", m_target.Runtime.Expression.BlinkOverrideRate, 0f, 1f);
                EditorGUILayout.Slider("LookAt override rate", m_target.Runtime.Expression.LookAtOverrideRate, 0f, 1f);
                EditorGUILayout.Slider("Mouth override rate", m_target.Runtime.Expression.MouthOverrideRate, 0f, 1f);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
