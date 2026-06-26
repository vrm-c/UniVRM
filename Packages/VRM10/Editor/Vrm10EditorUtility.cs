using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public static class Vrm10EditorUtility
    {
        /// <summary>
        /// スライダーと数値入力で限界値の違う、所謂「限界突破スライダー」を作成する
        /// `EditorGUILayout.PropertyField` の代替として利用する
        /// </summary>
        public static void LimitBreakSlider(SerializedProperty property, float sliderLeft, float sliderRight, float numberLeft, float numberRight)
        {
            var label = new GUIContent(property.displayName);
            var currentValue = property.floatValue;

            var rect = EditorGUILayout.GetControlRect();

            EditorGUI.BeginProperty(rect, label, property);

            rect = EditorGUI.PrefixLabel(rect, label);

            // slider
            {
                EditorGUI.BeginChangeCheck();

                var sliderRect = rect;
                sliderRect.width -= 55.0f;
                rect.xMin += rect.width - 50.0f;

                var clampedvalue = Mathf.Clamp(currentValue, sliderLeft, sliderRight);
                var sliderValue = GUI.HorizontalSlider(sliderRect, clampedvalue, sliderLeft, sliderRight);

                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = sliderValue;
                }
            }

            // number
            {
                EditorGUI.BeginChangeCheck();

                var numberValue = Mathf.Clamp(EditorGUI.FloatField(rect, currentValue), numberLeft, numberRight);

                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = numberValue;
                }
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Control ID を取得
        /// </summary>
        /// <param name="component">対象となるコンポーネント</param>
        /// <returns>コンポーネントの Control ID</returns>
        /// <remarks>Control ID とは、Handles.CubeHandleCap()の第一引数等で使用可能な、コンポーネント単位でユニークなint値のこと</remarks>
        internal static int GetControlId(this Component component)
#if UNITY_6000_5_OR_NEWER
            => component.GetEntityId().GetHashCode();
#else
            => component.GetInstanceID();
#endif
    }
}