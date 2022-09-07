using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// NOTE: 簡便に実装されたものなので、最適化の余地はある.
    /// </summary>
    public sealed class RuntimeOnlyAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;

        public RuntimeOnlyAwaitCaller()
        {
            _scheduler = new NextFrameTaskScheduler();
        }

        public Task NextFrame()
        {
            var tcs = new TaskCompletionSource<object>();
            _scheduler.Enqueue(() => tcs.SetResult(default));
            return tcs.Task;
        }

        public Task Run(Action action)
        {
            return Task.Run(action);
        }

        public Task<T> Run<T>(Func<T> action)
        {
            return Task.Run(action);
        }

        /// <summary>
        /// 指定した時間が経過している場合のみ、NextFrame() を使って1フレーム待つ
        /// </summary>
        /// <param name="timeOutInMilliseconds">タイムアウト時間(ミリ秒単位)</param>
        /// <returns>タイムアウト時はNextFrame()を呼び出す。そうではない場合、Task.CompletedTaskを返す</returns>
        public Task NextFrameIfTimedOut_(float timeOutInMilliseconds = 1f)
        {
            if (!CheckTimeOut(timeOutInMilliseconds))
            {
                return Task.CompletedTask;
            }
            _lastBaseTime = 0f;
            return NextFrame();
        }

        private bool CheckTimeOut(float timeOutInMilliseconds)
        {
            float t = UnityEngine.Time.realtimeSinceStartup;
            if (_lastBaseTime == 0f)
            {
                // Reset base time
                _lastBaseTime = t;
            }
            return (t - _lastBaseTime) >= timeOutInMilliseconds * (1f / 1000f);
        }

        private float _lastBaseTime;
    }

    internal static class RuntimeOnlyAwaitCallerHelper 
    {
        /// <summary>
        /// 指定した時間が経過している場合のみ、NextFrame() を使って1フレーム待つ
        /// </summary>
        /// <param name="iAwaitCaller">IAwaitCallerのインスタンス</param>
        /// <param name="timeOutInMilliseconds">タイムアウト時間(ミリ秒単位)</param>
        /// <returns>タイムアウト時はNextFrame()を呼び出す。そうではない場合、Task.CompletedTaskを返す</returns>
        internal static Task NextFrameIfTimedOut(this IAwaitCaller iAwaitCaller, float timeOutInMilliseconds)
        {
            if (iAwaitCaller is RuntimeOnlyAwaitCaller runtimeOnlyAwaitCaller)
            {
                return runtimeOnlyAwaitCaller.NextFrameIfTimedOut_(timeOutInMilliseconds);
            }
            return Task.CompletedTask;
        }
    }
}
