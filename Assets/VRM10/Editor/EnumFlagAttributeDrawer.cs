using System;
using UnityEngine;
using UnityEditor;

namespace UniVRM10
{
    /// <summary>
    /// Flag設定したEnumのインスペクター表示を変えるクラス
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public sealed class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            SerializedProperty prop,
            GUIContent label
        )
        {
            var buttonsIntValue = 0;
            var enumLength = prop.enumNames.Length;
            var labelWidth = EditorGUIUtility.labelWidth;
            var buttonPressed = new bool[enumLength];
            var buttonWidth = (position.width - labelWidth) / enumLength;

            var labelPos = new Rect(
                position.x,
                position.y,
                labelWidth,
                position.height
            );
            EditorGUI.LabelField(labelPos, label);
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < enumLength; i++)
            {
                buttonPressed[i] = (prop.intValue & (1 << i)) == 1 << i;

                var buttonPos = new Rect(
                    position.x + labelWidth + buttonWidth * i,
                    position.y,
                    buttonWidth,
                    position.height
                );

                buttonPressed[i] = GUI.Toggle(
                    buttonPos,
                    buttonPressed[i],
                    prop.enumNames[i],
                    "Button"
                );

                if (buttonPressed[i])
                {
                    buttonsIntValue += 1 << i;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = buttonsIntValue;
            }
        }
    }
}
