using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRM
{
    public struct Validation
    {
        /// <summary>
        /// エクスポート可能か否か。
        /// true のメッセージは警告
        /// false のメッセージはエラー
        /// </summary>
        public readonly bool CanExport;
        public readonly String Message;

        Validation(bool canExport, string message)
        {
            CanExport = canExport;
            Message = message;
        }

#if UNITY_EDITOR
        public void DrawGUI()
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            if (CanExport)
            {
                // warning
                EditorGUILayout.HelpBox(Message, MessageType.Warning);
            }
            else
            {
                // error
                EditorGUILayout.HelpBox(Message, MessageType.Error);
            }
        }
#endif

        public static Validation Error(string msg)
        {
            return new Validation(false, msg);
        }

        public static Validation Warning(string msg)
        {
            return new Validation(true, msg);
        }
    }
}