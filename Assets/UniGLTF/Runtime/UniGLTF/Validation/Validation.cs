using System;
using System.Collections.Generic;

namespace UniGLTF
{
    public enum ErrorLevels
    {
        /// <summary>
        /// Exportできる。お知らせ       
        /// </summary>
        Info,

        /// <summary>
        /// Exportできる。不具合の可能性
        /// </summary>
        Warning,

        /// <summary>
        /// Exportするために修正が必用
        /// </summary>
        Error,

        /// <summary>
        /// Exportの前提を満たさない
        /// </summary>
        Critical,
    }

    public struct Validation
    {
        public readonly ErrorLevels ErrorLevel;

        /// <summary>
        /// エクスポート可能か否か
        /// </summary>
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

        /// <summary>
        /// DrawGUIから呼び出す。追加のGUIボタンなどを実装する
        /// </summary>
        public Action Extended;

        Validation(ErrorLevels canExport, string message, Action extended = null)
        {
            ErrorLevel = canExport;
            Message = message;
            Extended = extended;
        }

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
