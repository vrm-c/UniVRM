using System;
using UnityEngine;

namespace UniGLTF
{
    public static class UniGLTFLogger
    {
        class DefaultUnityLogger : ILogHandler
        {
            public void LogException(System.Exception exception, UnityEngine.Object context)
            {
                Debug.LogException(exception, context);
            }

            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                switch (logType)
                {
                    case LogType.Error:
                        Debug.LogError(string.Format(format, args), context);
                        break;
                    case LogType.Assert:
                        Debug.LogAssertion(string.Format(format, args), context);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(string.Format(format, args), context);
                        break;
                    case LogType.Log:
                        Debug.Log(string.Format(format, args), context);
                        break;
                    case LogType.Exception:
                        // where exception ?
                        Debug.LogError(string.Format(format, args), context);
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        static ILogHandler s_logger = new DefaultUnityLogger();

        public static void SetLogger(ILogHandler logger)
        {
            if (logger != null)
            {
                s_logger = logger;
            }
            else
            {
                s_logger = new DefaultUnityLogger();
            }
        }

        [System.Diagnostics.Conditional("VRM_DEVELOP")]
#if UNITY_2022_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log(string msg, UnityEngine.Object context = null) => s_logger.LogFormat(LogType.Log, context, msg);

#if UNITY_2022_OR_NEWER
        [HideInCallstack]
#endif
        public static void Warning(string msg, UnityEngine.Object context = null) => s_logger.LogFormat(LogType.Warning, context, msg);

#if UNITY_2022_OR_NEWER
        [HideInCallstack]
#endif
        public static void Error(string msg, UnityEngine.Object context = null) => s_logger.LogFormat(LogType.Error, context, msg);

#if UNITY_2022_OR_NEWER
        [HideInCallstack]
#endif
        public static void Exception(Exception exception, UnityEngine.Object context = null) => s_logger.LogException(exception, context);
    }
}