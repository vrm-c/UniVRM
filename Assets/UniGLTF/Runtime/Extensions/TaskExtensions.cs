using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UniGLTF
{
    /// <summary>
    /// work around
    /// 
    /// https://forum.unity.com/threads/async-await-in-editor-script.481276/
    /// 
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/UnitySynchronizationContext.cs
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static class TaskExtensions
    {
        delegate void ExecFunc();

        static ExecFunc s_exec;

        static void Invoke()
        {
            if (s_exec == null)
            {
                var context = SynchronizationContext.Current;
                var t = context.GetType();
                var execMethod = t.GetMethod("Exec", BindingFlags.NonPublic | BindingFlags.Instance);
                var exec = execMethod.CreateDelegate(typeof(ExecFunc), context);
                s_exec = (ExecFunc)exec;
            }
            s_exec();
        }

        public static IEnumerable AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;

#if UNITY_EDITOR
                if (!UnityEngine.Application.isPlaying)
                {
                    Invoke();
                }
#endif                
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
}
