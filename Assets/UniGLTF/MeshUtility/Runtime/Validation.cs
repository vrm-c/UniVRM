using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshUtility
{
    public enum ErrorLevels
    {
        // Exportできる。お知らせ
        Info,
        // Exportできる。不具合の可能性
        Warning,
        // Exportするために修正が必用
        Error,
        // Exportの前提を満たさない
        Critical,
    }

    public struct Validation
    {
        /// <summary>
        /// エクスポート可能か否か。
        /// true のメッセージは警告
        /// false のメッセージはエラー
        /// </summary>
        public readonly ErrorLevels ErrorLevel;

        public bool CanExport
        {
            get
            {
                switch (ErrorLevel)
                {
                    case ErrorLevels.Info:
                    case ErrorLevels.Warning:
                        return true;
                    case ErrorLevels.Error:
                    case ErrorLevels.Critical:
                        return false;
                }
                throw new NotImplementedException();
            }
        }

        public readonly String Message;

        Validation(ErrorLevels canExport, string message, Action extended = null)
        {
            ErrorLevel = canExport;
            Message = message;
#if UNITY_EDITOR
            Extended = extended;
#endif
        }

#if UNITY_EDITOR
        public void DrawGUI()
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            switch (ErrorLevel)
            {
                case ErrorLevels.Info:
                    EditorGUILayout.HelpBox(Message, MessageType.Info);
                    break;
                case ErrorLevels.Warning:
                    EditorGUILayout.HelpBox(Message, MessageType.Warning);
                    break;
                case ErrorLevels.Critical:
                case ErrorLevels.Error:
                    EditorGUILayout.HelpBox(Message, MessageType.Error);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (Extended != null)
            {
                Extended();
            }
        }

        public Action Extended;
#endif

        public static Validation Critical(string msg)
        {
            return new Validation(ErrorLevels.Critical, msg);
        }

        public static Validation Error(string msg, Action action = null)
        {
            return new Validation(ErrorLevels.Error, msg, action);
        }

        public static Validation Warning(string msg)
        {
            return new Validation(ErrorLevels.Warning, msg);
        }

        public static Validation Info(string msg)
        {
            return new Validation(ErrorLevels.Info, msg);
        }

        public void AddTo(IList<Validation> dst)
        {
            dst.Add(this);
        }
    }
}
